# Rendering Pipeline — Mark In DeDark (TypeScript / PixiJS 8)

Documento di riferimento aggiornato per la pipeline di rendering TypeScript/PixiJS 8.
Descrive ogni passo, ogni RenderTexture, ogni shader e ogni blend state della pipeline attuale.
Da leggere in parallelo con `RENDERING_PIPELINE_CSHARP.md` per confronto diretto.

---

## Panoramica concettuale

La pipeline produce tre layer sovrapposti:

1. **Ground Layer** — Pavimento illuminato con normal mapping diffuso + ambient tint
2. **Lightmap Layer** — Lightmap ombre morbide (blend multiply), scurisce il pavimento
3. **Objects Layer** — Muri, pilastri, personaggi illuminati con normal mapping diffuso

Il tutto è gestito da **10 RenderTexture** (`1280×720` ciascuna) e **1 filter custom** (`NormalMapFilter`) con dual shader WGSL/GLSL.

---

## File principali

| File | Responsabilità |
|------|---------------|
| `Web/src/lighting/ShadowSystem.ts` | Orchestratore pipeline — gestisce tutte le RT e il loop per-frame |
| `Web/src/lighting/NormalMapFilter.ts` | Filter PixiJS con shader WGSL (WebGPU) + GLSL (WebGL2) |
| `Web/src/lighting/MarkLight.ts` | Sorgente luminosa — posizione, colore animato, parametri shader |
| `Web/src/lighting/Hull.ts` | Occluder rettangolare — calcola poligoni shadow per la lightmap |
| `Web/src/grid/LevelCustomization.ts` | Configurazione per-livello dei colori ambient |
| `Web/src/world/Camera.ts` | Camera 2D — zoom 0.8x, pivot bottom-left, espone worldContainer |

---

## Campi di default di ShadowSystem

Valori dichiarati nella classe (prima che `LevelCustomization` li sovrascriva):

```typescript
ambientColor          = 0x000000   // colore ambient lightmap
ambientAlpha          = 1.0        // opacità ambient lightmap
normalMapAmbienceColor = { r: 0.35, g: 0.35, b: 0.35 }
```

---

## Parametri globali

| Parametro | Valore | Note |
|-----------|--------|------|
| Canvas | 1280×720 px | (C#: 1920×1080) |
| Tutte le RenderTexture | 1280×720 px | Stessa dimensione canvas |
| Camera zoom | 0.8x fisso | `setZoom(0.8)` in `Camera.ts:30` |
| Camera pivot | (0, worldHeight) | Y-up: bottom-left del mondo |
| Light texture size | 256×256 px | Gradient radiale pre-renderizzato |

---

## RenderTexture inventory

```
_groundRT              1280×720  Ground sprites base (opaque black BG)
_groundNormalRT        1280×720  Normal map sprites del pavimento (BG nero)
_normalMappedGroundRT  1280×720  Ground base + illuminazione diffusa accumulata
_objectsRT             1280×720  Muri/pilastri/personaggi base
_objectsNormalRT       1280×720  Normal map muri/pilastri/personaggi (BG neutro 0x8080FF)
_normalMappedObjectsRT 1280×720  Objects base + illuminazione diffusa accumulata
_lightmap              1280×720  Lightmap ombre (ambient fill + luce radiale - ombre)
_perLightRT            1280×720  Buffer temporaneo riusato per-luce (NM + lightmap)
_blurredRT             1280×720  Buffer per il blur pass della lightmap
_lightTexture          256×256   Gradient radiale bianco pre-renderizzato (statico)
```

---

## Stage layout finale

> **Dipendenza di costruzione**: `Camera` deve essere costruita **prima** di `ShadowSystem`.
> Il costruttore di Camera chiama `app.stage.addChild(this._world)` (posizione 0).
> Il costruttore di ShadowSystem usa `addChildAt(x, 0/1/2)`, spostando `_world` in
> posizione [3]. Se l'ordine fosse invertito, il layout sarebbe errato.

Dopo la costruzione di `ShadowSystem`, `app.stage` contiene:

```
app.stage
  [0] _normalMappedGroundSprite   blendMode='normal'    ground + NM lighting
  [1] _lightmapSprite             blendMode='multiply'  lightmap ombre
  [2] _normalMappedObjectsSprite  blendMode='normal'    objects + NM lighting
  [3] camera._world               (scene containers — non usato direttamente dal ShadowSystem)
```

> **Critico**: il lightmap in posizione [1] usa `multiply` quindi scurisce SOLO il ground ([0]).
> Gli objects ([2]) sono sopra il lightmap e **non** vengono scuriti dalle ombre.
> Questo è comportamento intenzionale: i muri/personaggi emergono sempre nel buio.

---

## Container interni della pipeline

```
_groundContainer          Sprites pavimento (aggiunto da GameGrid)
_groundNormalContainer    Sprites normal map pavimento (aggiunto da GameGrid)

_objectsContainer         Sprites muri, pilastri, personaggi
_objectsNormalContainer   Sprites normal map muri, pilastri
```

> **Nota**: nelle fasi 4 e 5 il codice crea un Container temporaneo (`tmp`) con
> `sortableChildren=true` che wrappa `_objectsContainer` / `_objectsNormalContainer`
> per il render. La proprietà è sul `tmp`, non sui container originali. Poiché `tmp`
> ha un unico figlio, `sortableChildren` non ha effetto pratico sul sorting interno;
> l'ordinamento degli sprite dentro `_objectsContainer` dipende dalla proprietà
> `sortableChildren` del container stesso, impostata da chi lo crea (es. `GameGrid`).

---

## Pipeline per-frame: `ShadowSystem.update()`

**File**: `ShadowSystem.ts:156`

### Fase 0 — Animazione luci

```typescript
for (const light of this.lights) light.update(deltaMS);
```

Ogni luce fa ping-pong del colore tra `color1` e `color2` con periodo 3000ms.

---

### Fase 1 — Ground base → `_groundRT`

```
1. Fill nero (0x000000) sul canvas intero → _groundRT (clear=true)
2. Render _groundContainer con camera transform → _groundRT (clear=false)
```

Background opaco nero garantisce che i pixel non coperti dal pavimento siano neri.
La camera transform viene replicata su un Container temporaneo clonando:
- `position`, `scale`, `pivot` da `camera.worldContainer`

---

### Fase 2 — Ground normals → `_groundNormalRT`

```
1. Fill nero (0x000000) → _groundNormalRT (clear=true)
2. Render _groundNormalContainer con camera transform → _groundNormalRT (clear=false)
```

**Colore di clear**: `0x000000` = normale (0,0,0) → decodificata come (-1,-1,-1).
I pixel senza normal map sprite ricevono un vettore normale non valido.
Questo è accettabile: le zone di pavimento non coperte da normal map sprite non
producono illuminazione nello shader (early-out su alpha < 0.01 nel fragment).

---

### Fase 3 — Normal-map ground → `_normalMappedGroundRT`

```
a) Tint ambient:
   ambientTint = ambienceColorHex(this.normalMapAmbienceColor)
   Sprite(_groundRT).tint = ambientTint
   Render → _normalMappedGroundRT (clear=true)

b) _normalMapFilter.setNormalMap(_groundNormalRT)   ← UNA VOLTA prima del loop

c) Per ogni light (enabled=true):
   sx, sy = _worldToScreen(light.position + light.deltaForGround, cameraWorld)
   z = light.z * abs(cameraWorld.scale.x)   ← Z scalato per lo zoom camera!

   _normalMapFilter.setLight(sx, sy, z, light.colorNormalVec3,
                              light.distanceSquaredForGround, light.opacity)

   Render filterContainer → _perLightRT (clear=true)     ← per-light buffer
   Sprite(_perLightRT).blendMode='add'
   Render → _normalMappedGroundRT (clear=false)           ← accumulo additivo
```

**delta offset pavimento**: `deltaForGround = { x:0, y:+100 }` (default per tutte le luci).
Simula che la luce stia sopra e dietro la sorgente visiva rispetto al piano del pavimento.

---

### Fase 4 — Objects base → `_objectsRT`

```
Render _objectsContainer (sortableChildren=true) con camera transform → _objectsRT (clear=true)
```

Uguale alla fase 1 ma per il layer oggetti.

---

### Fase 5 — Objects normals → `_objectsNormalRT`

```
1. Fill neutro (0x8080FF) → _objectsNormalRT (clear=true)
2. Render _objectsNormalContainer con camera transform → _objectsNormalRT (clear=false)
```

**Colore di clear**: `0x8080FF` = normale (128,128,255) → decodificata come (0,0,1).
I pixel senza normal map sprite (candele, chiave, candle packs) ricevono la normale
piatta (0,0,1) = direttamente verso la camera. Questo produce solo luce ambient
nell'equazione: `nDotL` con direzione verticale è piccolo per luci laterali, come da
C# spec (comportamento intenzionale: candele e chiave hanno aspetto piatto).

---

### Fase 6 — Normal-map objects → `_normalMappedObjectsRT`

Identica alla fase 3, con due differenze:

```
- setNormalMap: _objectsNormalRT (invece di _groundNormalRT) — chiamato UNA VOLTA prima del loop
- light offset: light.deltaForWall (invece di light.deltaForGround)
- distance: light.distanceSquaredForWall (invece di distanceSquaredForGround)
```

**delta offset muri**: `deltaForWall = { x:0, y:-60 }` (default).
Simula che la luce stia davanti e sopra per i muri/pilastri verticali.

---

### Fase 7 — Shadow lightmap → `_lightmap`

```
a) Fill ambient:
   Graphics.rect(0, 0, 1280, 720).fill({ color: ambientColor, alpha: ambientAlpha })
   Render → _lightmap (clear=true)

b) Per ogni light (enabled=true):
   1. Crea perLightContainer con camera transform clonata
   2. lightSprite = Sprite(_lightTexture)
      lightSprite.scale = light.radius / 256
      lightSprite.anchor = (0.5, 0.5)
      lightSprite.position = light.position (world-space)
      lightSprite.tint = light.currentColor
      lightSprite.alpha = light.opacity

   3. Se light.castsShadows && hulls.length > 0:
      - Per ogni hull abilitato:
        polys = hull.computeShadowPolygons(light.position.x, light.position.y)
        Disegna trapezoid neri in shadowGfx
      - Disegna bounds neri di ogni hull in hullFillGfx (anti self-shadow)

   4. Render perLightContainer → _perLightRT (clear=true)

   5. Blur pass:
      _blurFilter.strength = light.sourceRadius * 0.15   (quality fisso a 2)
      Render filterContainer con BlurFilter → _blurredRT (clear=true)

   6. Sprite(_blurredRT).blendMode='add'
      Render → _lightmap (clear=false)   ← accumulo additivo
```

---

## Shader: NormalMapFilter

**File**: `NormalMapFilter.ts`

Unico filter — riusato per tutti i pass NM (cambia solo la normal map texture tramite `setNormalMap()`).

**Valori default degli uniform alla costruzione**:

```typescript
uLightPosition: [0, 0, 75, 0]      // z=75 default
uLightColor:    [1, 1, 1, 0]       // bianco
uLightParams:   [640000, 1, 0, 0]  // distanceSq=640000, opacity=1
```

Questi vengono sovrascritti ad ogni frame da `setLight()` prima di ogni render per-luce.

### WGSL (WebGPU)

**Uniform groups**:

```wgsl
// Group 0: Standard PixiJS filter uniforms
struct GlobalFilterUniforms {
    uInputSize:    vec4<f32>,   // xy=texture size, zw=inverse size
    uInputPixel:   vec4<f32>,
    uInputClamp:   vec4<f32>,   // xy=min UV, zw=max UV
    uOutputFrame:  vec4<f32>,   // xy=screen offset, zw=size in pixels
    uGlobalFrame:  vec4<f32>,
    uOutputTexture:vec4<f32>,   // xy=texture size, z=flipY flag
}

// Group 1: Custom per-light uniforms
struct NormalMapUniforms {
    uLightPosition: vec4<f32>,  // xy=screen pos (pixels), z=height, w=unused
    uLightColor:    vec4<f32>,  // rgb=[0-1], a=unused
    uLightParams:   vec4<f32>,  // x=distanceSq, y=opacity, zw=unused
}
```

**Bindings**:
- `@group(0) @binding(0)` — GlobalFilterUniforms
- `@group(0) @binding(1)` — uTexture (diffuse — la RT del layer)
- `@group(0) @binding(2)` — uSampler
- `@group(1) @binding(0)` — NormalMapUniforms
- `@group(1) @binding(1)` — uNormalMap (RT del layer normal map)
- `@group(1) @binding(2)` — uNormalMapSampler

**Vertex shader**: trasforma posizione → clip space, calcola UV per il filter.

**Fragment shader**:

```wgsl
@fragment
fn mainFragment(@location(0) uv: vec2<f32>) -> @location(0) vec4<f32> {
    let clampedUv = clamp(uv, gfu.uInputClamp.xy, gfu.uInputClamp.zw);
    let diffuse = textureSample(uTexture, uSampler, clampedUv);

    // Early-out per pixel trasparenti
    if (diffuse.a < 0.01) { return vec4<f32>(0.0); }

    // Posizione pixel in screen-space (corretta indipendentemente dalla pool texture)
    let pixelPos = clampedUv * gfu.uInputSize.xy;

    // Sample normal map con UV ricalcolati sulla RT separata (1280×720)
    let normalMapSize = vec2<f32>(textureDimensions(uNormalMap, 0));
    let normalMapUv   = pixelPos / normalMapSize;
    let normalSample  = textureSample(uNormalMap, uNormalMapSampler, normalMapUv);

    // Decode: [0,1] → [-1,1], flip Y per convenzione screen-space
    var n = normalSample.rgb * 2.0 - 1.0;
    n.y = -n.y;   // ← CRITICO: flip Y perché MonoGame usa Y verso il basso
    n = normalize(n);

    // Vettore luce: da pixel a luce in screen-space (xy in pixel, z = altezza)
    let lightVec = vec3<f32>(normalMapUniforms.uLightPosition.xy - pixelPos, normalMapUniforms.uLightPosition.z);
    let lightDir = normalize(lightVec);

    // Lambertian
    let nDotL = max(dot(n, lightDir), 0.0);

    // Attenuazione inverse-square (non clamped — replica comportamento C#)
    let actualDistSq = dot(lightVec, lightVec);
    let attenuation  = max(normalMapUniforms.uLightParams.x / max(actualDistSq, 1.0), 0.0);

    // Formula finale: COSENO AL QUADRATO (non Lambertian puro)
    let lit = diffuse.rgb * normalMapUniforms.uLightColor.rgb * nDotL * nDotL * attenuation * normalMapUniforms.uLightParams.y;
    return vec4<f32>(lit, diffuse.a);
}
```

### GLSL (WebGL2)

Identica logica del fragment WGSL, tradotta in GLSL ES 3.0.

Uniforms flat (nessun UBO struct):
```glsl
uniform sampler2D uTexture;
uniform sampler2D uNormalMap;
uniform vec4 uInputSize;
uniform vec4 uInputClamp;
uniform vec4 uLightPosition;   // xy=screen pos, z=height
uniform vec4 uLightColor;      // rgb=[0-1]
uniform vec4 uLightParams;     // x=distanceSq, y=opacity
```

---

## Formula di illuminazione (critica)

```
output_rgb = diffuse.rgb × lightColor × nDotL² × (distanceSq / max(|lightVec|², 1)) × opacity
```

Dove:
- `nDotL = max(dot(normal, lightDir), 0)` — coseno dell'angolo luce-normale
- `nDotL²` — **coseno al quadrato**: contrasto più marcato rispetto a Lambertian puro
- `distanceSq / |lightVec|²` — attenuazione inverse-square non clamped

> **Nota**: Nessun termine ambient nello shader. L'ambient è gestito come tint sull'intera
> RT base prima del loop per-luce (fase 3a e 6a). Non si accumula per numero di luci.

---

## Classe MarkLight

**File**: `MarkLight.ts`

```typescript
position:       { x: number; y: number }  // World-space
color1, color2: number                     // Hex, per animazione ping-pong
currentColor:   number                     // Interpolato, usato per lightmap tint
opacity:        number                     // [0-1], moltiplicatore intensità
radius:         number                     // Raggio lightmap in world-space px
castsShadows:   boolean                    // Solo per lightmap (Hull occlusion)
enabled:        boolean                    // Toggle on/off
z:              number                     // Altezza simulata per NM (prima del zoom)
sourceRadius:   number                     // Raggio sorgente — blur = sourceRadius * 0.15

deltaForGround: { x:0, y:+100 }           // Default offset per normal map pavimento
deltaForWall:   { x:0, y:-60  }           // Default offset per normal map muri

_distanceSquared:        number            // Attenuation per ground NM
_distanceSquaredForWall: number            // Attenuation per wall NM (default = _distanceSquared)
_originalColor2:         number            // Backup del color2 originale
```

**Metodi**:

```typescript
setTemporaryColor(color: number): void  // Sovrascrive color2 (Skeleton rage, Taurus charge)
resetColor(): void                      // Ripristina _originalColor2 in color2
getPenumbraPosition(): { x, y }         // Ritorna { x: position.x - 10, y: position.y + 75 }
```

**Animazione colore**: linear ping-pong tra `color1` e `color2`, periodo 3000ms.

**Z scaling**: `z * abs(cameraWorld.scale.x)` → con zoom 0.8x, lo Z viene moltiplicato per 0.8.

### Parametri per entità

| Entità | z | radius | distanceSq | distanceSqWall | castsShadows | opacity | deltaForWall |
|--------|---|--------|------------|----------------|--------------|---------|--------------|
| **Player** | 90 | 1200 | 20600 | 20600 | false¹ | 1.0 | (0,-60) |
| **Skeleton** | 46 | 300 | 4950 | 4950 | false | 0.5 | (0,-60) |
| **Zombie** | 46 | 300 | 4950 | 4950 | false | 0.5 | (0,-60) |
| **Goblin** | 46 | 300 | 4950 | 4950 | false | 0.5 | (0,-60) |
| **Taurus** | 46 | 300 | 4950 | 4950 | false | 0.5 | (0,-60) |
| **Ghost** | 46 | 300 | 4950 | 4950 | false | 1.0 | **(0,-30)** |
| **Candle** | 67 | 150 | 7280 | 7280 | false | 1.0 | (0,-60) |
| **BrazierLit** | 67 | 150 | 7280 | **17000** | false | 1.0 | (0,-60) |

> ¹ **Bug noto**: Player.ts:84 imposta `castsShadows: false`. In C# il player era l'unica
> sorgente con shadow casting. Il default del costruttore `MarkLight` è `true`,
> ma viene esplicitamente disabilitato. Le ombre del player sono quindi assenti.
>
> ² **Candle opacity dinamica**: durante l'animazione di morte della candela,
> `this.surroundingLight.opacity` viene abbassato gradualmente fino a 0 (`Candle.ts:85`).
> Il valore 1.0 in tabella è il valore iniziale.

### Colori luci per entità

| Entità | color1 | color2 |
|--------|--------|--------|
| Player | 0xFFEDD6 (giallo caldo) | 0xFFFFFF (bianco) |
| Skeleton | 0xC476F0 (viola) | 0x9159B2 (viola scuro) |
| Zombie | 0x8BEF5A (verde) | 0x6DBC45 (verde scuro) |
| Goblin | 0xCAF076 (giallo-verde) | 0xC3E27C (giallo-verde scuro) |
| Taurus | 0xFF2600 (rosso) | 0xC9341A (rosso scuro) |
| Ghost | 0x9CEEC0 (verde menta) | 0xEFC0E3 (rosa) |
| Candle/Brazier | 0xFFC3A3 (bianco rosato) | 0xFFF7CF (bianco caldo) |

---

## Classe Hull (shadow caster)

**File**: `Hull.ts`

Rappresenta un occluder rettangolare in world-space (muri, pilastri).

**Vertici**: TL → BL → BR → TR (clockwise in screen coords, Y verso il basso).

**`computeShadowPolygons(lightX, lightY)`**:
- Per ogni edge del rettangolo (4 edge):
  - Calcola normale esterna CW: `normalX = -(p2.y - p1.y)`, `normalY = (p2.x - p1.x)`
  - Se back-facing (dot(normal, midpoint-light) > 0):
    - Proietta i vertici dell'edge di `FAR = 5000` pixel dalla luce
    - Restituisce trapezoid `[p1, p2, far2, far1]` come flat array
- Disegna shadowGfx + hullFillGfx neri sul `_perLightRT`

---

## Configurazione per-livello

**File**: `LevelCustomization.ts`

```typescript
penumbraAmbientColor:  number   // Hex — colore base lightmap (ombre)
penumbraAmbientAlpha:  number   // Opacità base lightmap
normalMapAmbienceColor: { r, g, b }  // Tint ambient sul layer NM
```

| Livello | penumbraAmbientColor | penumbraAmbientAlpha | normalMapAmbienceColor |
|---------|---------------------|----------------------|------------------------|
| 1-2 | 0x686868 (grigio) | 1.0 | { r:0.35, g:0.35, b:0.35 } |
| 3-8 | 0x000000 (nero) | 1.0 | { r:0.35, g:0.35, b:0.35 } |

> **Discrepanza con C#**: la spec C# indica `normalMapAmbienceColor`:
> - Livelli 1-2: `(0.11, 0.10, 0.22)` — grigio-blu
> - Livelli 3-8: `(0.0, 0.0, 0.0)` — nero assoluto
>
> L'implementazione TS usa `(0.35, 0.35, 0.35)` per tutti i livelli.
> Questo rende il pavimento sempre leggermente illuminato anche al buio (livelli 3-8).
> Vedi Task 1 in CLAUDE.md.

---

## Trasformazione world → screen per le luci

**Metodo**: `ShadowSystem._worldToScreen()` (`ShadowSystem.ts:423`)

```typescript
sx = (wx - cw.pivot.x) * cw.scale.x + cw.position.x
sy = (wy - cw.pivot.y) * cw.scale.y + cw.position.y
```

Dove `cw = camera.worldContainer`:
- `cw.pivot` = (0, worldHeight) — pivot bottom-left
- `cw.scale` = (0.8, 0.8) — zoom fisso
- `cw.position` = posizione camera

---

## Light texture (gradient radiale pre-renderizzato)

**Metodo**: `ShadowSystem._createLightTexture(256)` (`ShadowSystem.ts:440`)

```
RT 256×256
24 cerchi concentrici dal raggio massimo al minimo
Ogni cerchio: blendMode='add', alpha = 1/24
Risultato: centro alpha ≈ 1.0, bordo alpha ≈ 0 (degradé lineare)
```

Viene scalata per ogni luce: `lightSprite.scale = light.radius / 256`.

---

## Camera

**File**: `Camera.ts`

```
zoom:    0.8 fisso (setZoom(0.8) in costruttore)
x pivot: 0
y pivot: worldHeight (coordinate Y-up — bottom del mondo = origine)
position: aggiornato ogni frame dal GameBootstrap
shake:   delta additivo su position, attenuazione con decremento intensity 0.25/frame
```

---

## Blend mode summary

| Operazione | Blend Mode | Formula |
|-----------|------------|---------|
| Accumulo NM per-luce su _normalMappedGroundRT / _objectsRT | `add` | `dst + src` |
| Accumulo luce su _lightmap | `add` | `dst + src` |
| _lightmapSprite sullo stage | `multiply` | `dst × src` |
| Light texture pre-render (256×256) | `add` | `dst + src` |
| Tutti gli altri | `normal` | alpha blend |

---

## Sequenza completa per-frame (pseudocodice)

```
update()
├── Fase 0: animate lights
│
├── Fase 1: render _groundContainer → _groundRT  (black BG)
│
├── Fase 2: render _groundNormalContainer → _groundNormalRT  (black BG)
│
├── Fase 3: normal-map ground → _normalMappedGroundRT
│   ├── ambient tint(_groundRT) → _normalMappedGroundRT  (clear)
│   ├── setNormalMap(_groundNormalRT)  ← UNA VOLTA prima del loop
│   └── for each enabled light:
│       ├── setLight(screenX+deltaGround, screenY+deltaGround, z×zoom, color, distSqGround, opacity)
│       ├── filter(_groundRT) → _perLightRT  (clear)
│       └── additive Sprite(_perLightRT) → _normalMappedGroundRT  (no clear)
│
├── Fase 4: render _objectsContainer → _objectsRT  (clear)
│
├── Fase 5: render _objectsNormalContainer → _objectsNormalRT  (0x8080FF BG)
│
├── Fase 6: normal-map objects → _normalMappedObjectsRT
│   ├── ambient tint(_objectsRT) → _normalMappedObjectsRT  (clear)
│   ├── setNormalMap(_objectsNormalRT)  ← UNA VOLTA prima del loop
│   └── for each enabled light:
│       ├── setLight(screenX+deltaWall, screenY+deltaWall, z×zoom, color, distSqWall, opacity)
│       ├── filter(_objectsRT) → _perLightRT  (clear)
│       └── additive Sprite(_perLightRT) → _normalMappedObjectsRT  (no clear)
│
└── Fase 7: shadow lightmap → _lightmap
    ├── fill(ambientColor, ambientAlpha) → _lightmap  (clear)
    └── for each enabled light:
        ├── lightSprite(radius, color, opacity) in world-space
        ├── [if castsShadows] shadow trapezoids + hull fills
        ├── render perLightContainer → _perLightRT  (clear)
        ├── blur(sourceRadius × 0.15) → _blurredRT
        └── additive Sprite(_blurredRT) → _lightmap  (no clear)

Stage compositing (automatico PixiJS):
  [0] _normalMappedGroundSprite   (normal blend)
  [1] _lightmapSprite             (multiply blend) ← scurisce solo il ground
  [2] _normalMappedObjectsSprite  (normal blend)
  [3] camera._world               (scene live — non gestita da ShadowSystem)
```

---

## Dettagli non ovvi e critici

1. **Y-flip in normal map shader**: `n.y = -n.y` compensa la convenzione Y-down del sistema
   di coordinate schermo (e di come le normal map vengono create per MonoGame/DirectX).

2. **Z scalato per zoom camera**: `light.z * abs(cw.scale.x)` — a zoom 0.8x, lo Z viene
   ridotto a `z × 0.8`. Senza questo, le normali sul bordo degli oggetti sarebbero sbagliate
   rispetto alla posizione visiva della luce.

3. **Coseno al quadrato** (`nDotL * nDotL`): non è un'ottimizzazione, è scelta di design
   visiva eredit ata dal C#. Produce una caduta di luce più brusca all'aumentare
   dell'angolo di incidenza rispetto a Lambertian puro.

4. **Normal map UV ricalcolati**: il filter pool di PixiJS potrebbe usare atlas texture
   di dimensioni diverse. Il codice ricalcola l'UV dalla posizione pixel assoluta:
   `normalMapUv = pixelPos / normalMapSize`. Questo garantisce correttezza indipendente
   dal resize del filter pool.

5. **Delta offset differenziati**: stessa luce ha offset diversi per ground vs wall.
   Questo simula la parallasse 2.5D del gioco isometrico — la luce "vede" il pavimento
   da una direzione e i muri da un'altra.

6. **Hull fill anti-self-shadow**: i bounds neri di ogni hull impediscono che il muro
   riceva luce dalla propria proiezione shadow (auto-shadow). Critico per evitare
   artefatti visivi ai bordi dei muri.

7. **Normal map neutra 0x8080FF**: decode → (0, 0, 1) = vettore verso la camera.
   Garantisce che le sprite senza normal map ricevano solo una piccola contribuzione
   di luce (proporzionale al cosine con la direzione della luce), ma non zero.

8. **`_perLightRT` riusato**: stesso buffer usato sia per il NM pass che per la lightmap.
   Non sono mai usati contemporaneamente — safe.

9. **Container temporaneo per camera transform** (`renderWithCameraTransform`):
   PixiJS non permette di passare una transform matrix direttamente a `renderer.render()`.
   Si crea un Container temporaneo con `position/scale/pivot` clonati da `_camera.worldContainer`,
   si aggiunge il child, si renderizza, si rimuove il child, si distrugge il Container.
   Le fasi 1 e 2 usano l'helper `renderWithCameraTransform`; le fasi 4 e 5 replicano
   lo stesso pattern manualmente (aggiungendo `tmp.sortableChildren = true` sul wrapper,
   che però non ha effetto pratico poiché il wrapper ha un solo figlio).

10. **ShadowSystem non aggiunge figli al `camera._world`**: tutti gli sprite di output
    vengono aggiunti direttamente ad `app.stage` (posizioni [0],[1],[2]).
    `camera._world` è in posizione [3] e contiene solo la scena live (eventualmente vuoto).
