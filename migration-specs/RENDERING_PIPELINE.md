# RENDERING_PIPELINE.md — Analisi Normal Map & Lighting

Creato come prerequisito al Task 8 (normal map rotta).
Confronto tra il rendering C# originale e l'implementazione TypeScript attuale.

---

## 1. Core Logic (C#) — Pipeline originale

La pipeline si trova in `MarkInDeDarkGame.DrawPlayingScene()` e in
`GameGrid.DrawGroundWithNormalMap()`.
Entrambe usano lo shader `circularnormalmap.fx` e (per il pavimento) anche
`specularmap.fx`.

### 1.1 Passo-per-passo in `DrawPlayingScene`

```
[0] Preparazione shader
    _circularNormalMapShader["AmbientColor"]    = _normalMapAmbienceColor  (Vector3)
    _circularNormalMapShader["World"]           = Matrix.Identity
    _circularNormalMapShader["ViewProjection"]  = orthographic(0, W, H, 0)

[1] DrawGroundWithNormalMap → _groundLevelResultRenderTarget
    a. baseRT                  = clear Black  + draw ground sprites
    b. normalMapRT             = clear Black  + draw ground _normal_ sprites
    c. specularMapRT           = clear Black  + draw ground _specular_ sprites
    d. specularLightmaskRT     = clear Black  + draw glow sprites per light (additive)
    e. appliedNormalMapRT      = clear Black
         per light (additive, BlendState.Additive):
           set LightPosition = light.PositionForGround   (screen-space Vector3, z=Z)
           set LightColor    = light.ColorNormal         (Vector3 [0-1])
           set LightDistSq   = light.DistSquaredForGround
           set LightOpacity  = light.Opacity
           spriteBatch.Draw(baseRT)  ← shader scrive ambient+lighting
    f. specularWithLightmaskRT = specularRT * specularLightmaskRT  (multiply shader)
    g. resultRT                = additive(appliedNormalMapRT + specularWithLightmaskRT)
    h. Draw shadows (Character.DrawShadows — sprite-based, NON hull-based)

[2] Penumbra (lightmap moltiplicativa)
    _penumbra.BeginDraw()
    draw(groundLevelResultRT)
    _penumbra.Draw(gameTime)           ← shadow lightmap con PenumbraLight per sorgente
    draw(additionalLightmaskRT) additive sul penumbra.LightMap
    _penumbra.BlendAll()               ← MULTIPLY finale sul frame

[3] Normal map per muri / pilastri / player / nemici
    normalMapRT = clear (128, 128, 255)    ← neutral pointing-toward-viewer
    draw all walls, pillars, player, enemies con le loro normal map (FrontToBack sort)

[4] Base scene (muri, oggetti, candele, player, nemici)
    baseSceneRT = clear Transparent
    draw in FrontToBack order

[5] Applica normal map alla base scene
    baseSceneItemsRT = clear Transparent
    per light (additive, BlendState.Additive):
      set LightPosition = light.PositionForWall   (screen-space, z=Z)
      ...
      spriteBatch.Draw(baseSceneRT)

[6] Composizione finale
    draw(groundLevelRT)           (pavimento con ombre Penumbra)
    draw(baseSceneItemsRT)        (oggetti/personaggi con lighting)

[7] Effetti "over dark" (particelle candela, neve, etc.)

[8] HUD
```

### 1.2 Formula esatta dello shader HLSL (`circularnormalmap.fx`)

```hlsl
// VERTEX: World = Identity → posWorld = screen-pixel coordinate (SpriteBatch coords)
float3 posWorld = input.PosWorld.xyz;  // ← screen pixels, NON world units

float3 lightdir = normalize(posWorld - LightPosition);   // da luce → pixel
float3 normal   = normalize(2 * normalMap.rgb - 1);
normal.y *= -1;

float nDotL           = saturate(dot(normal, -lightdir));       // da pixel → luce
float3 lightVector    = LightPosition - posWorld;
float attenuation     = LightDistanceSquared / dot(lightVector, lightVector);
float diffuseLighting = nDotL * attenuation;

float lightAmount = nDotL;   // = nDotL di prima (stessa variabile)

// Contributo per pixel per questa luce:
float3 contribution = AmbientColor + lightAmount * LightColor * diffuseLighting * LightOpacity;
//                  = AmbientColor + nDotL²        * LightColor * attenuation   * Opacity

output = input.Color * tex * contribution;
// Con BlendState.Additive e N luci:
// pixel_final = tex * ( N * AmbientColor + Σᵢ nDotLᵢ² * Colorᵢ * attᵢ * opᵢ )
```

---

## 2. Texture Specs

### 2.1 Normal map: encoding e canali

| Canale | Significato         | Valore neutro | Note                                 |
|--------|---------------------|---------------|--------------------------------------|
| R      | Normale X (+destra) | 0.5 = 128     |                                      |
| G      | Normale Y (+su)     | 0.5 = 128     | flip Y in shader: `n.y *= -1`        |
| B      | Normale Z (+fuori)  | 1.0 = 255     | B=255 → punta direttamente allo schermo |
| A      | (non usato)         | —             |                                      |

**Valore neutro per pixel senza normale**: `(128, 128, 255)` = RGB. Decodificato: `(0, 0, 1)` → nDotL è alto per luci frontali → pixel riceve illuminazione normale.

**Clear color usato per ground normal RT**: `Color.Black` = `(0, 0, 0)`. Decodificato: `(-1, -1, -1)` normalizzato = `(-0.577, -0.577, -0.577)`. Per luci dall'alto, `nDotL ≈ 0` → nessuna illuminazione. **Questo è intenzionale in C#**: le tile senza normal map sono quasi nere.

### 2.2 Parametri luci — valori empirici C#

| Parametro              | Esempio giocatore / candela  | Note                                     |
|------------------------|------------------------------|------------------------------------------|
| `Z`                    | ~90 world units              | Altezza della sorgente sopra il pavimento |
| `DistanceSquaredForGround` | ~20 600               | Unità² schermo (calibrate per zoom=0.8)  |
| `DistanceSquaredForWall`   | ~20 600               | Può differire dal ground               |
| `DeltaForGround`       | (0, +100)                    | Offset applicato prima di WorldToScreen  |
| `DeltaForWall`         | (0, -60)                     | Offset verso l'alto per le pareti        |
| `Opacity`              | 1.0                          | Scalare finale                           |
| `ColorNormal`          | vettore RGB animato tra C1/C2|                                          |

### 2.3 AmbientColor per livello

| Livelli | `AmbientColor` (C#)            | `PenumbraAmbientColor` (C#) |
|---------|--------------------------------|------------------------------|
| 1–2     | `(0.11, 0.10, 0.22)` blu-viola | `Color(104, 104, 104)` grigio|
| 3+      | `(0, 0, 0)` → nessun ambient   | `Color.Black`                |

### 2.4 Presence normale map nelle spritesheets

Le normal map **esistono** negli asset. Grep su `cell1_normal` trova corrispondenze in
`Spritesheet-0.json`, `Spritesheet-1.json`, `Spritesheet-2.json`. Anche pareti, pilastri
e `riempimento` hanno varianti `_normal`.

---

## 3. Discrepancy Report

### D1 — `normalMapAmbienceColor` errato per tutti i livelli (**CRITICO**)

| | Valore |
|-|--------|
| **TS attuale** | `{r: 0.35, g: 0.35, b: 0.35}` — **per tutti i livelli** |
| **C# corretto** | Livelli 1–2: `{r: 0.11, g: 0.10, b: 0.22}` · Livelli 3+: `{r: 0, g: 0, b: 0}` |

**Effetto osservato:**
- Livelli 3+ (la maggior parte del gioco): ambient 35% → le tile *senza* normal texture
  ricevono `tex * 0.35` invece di 0. Il contrasto buio/illuminato crolla.
- La differenza tra una tile illuminata da luce direzionale e una in ombra è ridotta,
  rendendo l'effetto 3D quasi impercettibile.

**Root cause**: task 5c menzionava la correzione nei commenti, ma il file
`LevelCustomization.ts` non è mai stato aggiornato; la riga è rimasta `0.35`.

**File da modificare**: `Web/src/grid/LevelCustomization.ts` riga 30.
**Fix**: `normalMapAmbienceColor: levelId <= 2 ? { r: 0.11, g: 0.10, b: 0.22 } : { r: 0, g: 0, b: 0 }`

---

### D2 — Ambient applicato una volta vs. N volte per luce (**MEDIO**)

| | Comportamento |
|-|---------------|
| **C#** | `AmbientColor` è nel corpo dello shader, blend additivo per N luci → ambient finale = `N × AmbientColor × tex` |
| **TS** | Ambient applicato come *pre-pass* separato (una sola volta), poi per-light additive senza ambient |

**Effetto**: TS produce ambient `1×` vs. C# che produce `N×`. Con 3 luci attive e ambient `(0.11,0.10,0.22)`, C# arriva a `(0.33,0.30,0.66)` mentre TS resta a `(0.11,0.10,0.22)`. Per livelli 3+ (ambient=0) non fa differenza.

**Fix (opzionale)**: aggiungere `uAmbientColor` come uniform nel NormalMapFilter e includerlo nella formula: `output = diffuse * (ambient + nDotL² * lightColor * att * op)`. Rimuovere la pre-pass ambient da ShadowSystem.

---

### D3 — Mancante: specular map + glow pass (**MEDIO**)

In C#, il rendering del pavimento include:
1. **Specular map texture** (canale speculare separato per ogni tile `cell1_spec`)
2. **Glow lightmask**: ogni sorgente di luce disegna lo sprite `Effects/glow` sopra la specular, poi il risultato (specular × glow) viene addato alla scena.

Questo produce riflessi brillanti sul pavimento vicino alle candele e ai bracieri.

**TS**: il passaggio specular non è implementato. Le spritesheets hanno già i frame
`cell1_spec`, ma non vengono usati.

**Fix (futura iterazione)**: aggiungere `_groundSpecRT`, render glow sprites per luce
(`Effects/glow` con deltaForGround), `specularResult = specularRT × glowRT`,
additive blend su `_normalMappedGroundRT`.

---

### D4 — Culling luci fuori schermo: mancante (**PERFORMANCE**)

C#: `light.IsOutOfScreen = camera.Contains(light.BoundingRectangle) == Disjoint` →
le luci fuori dal frustum vengono saltate.

TS: tutte le luci vengono processate ogni frame, anche quando fuori schermo.

Non causa difetti visivi ma impatta le performance con molte sorgenti di luce.

---

### D5 — Coordinate system: CORRETTO in TS (**nessun bug**)

SpriteBatch in MonoGame passa posizioni in screen-pixel (con `World=Identity`).
Lo shader TS opera anch'esso in screen-pixel tramite `_worldToScreen`. Entrambi
usano le stesse unità: consistente.

Zoom camera = **0.8 in entrambi** (`setZoom(0.8)` in Camera.ts, `Zoom=0.8f` in C#).
Il `distanceSquared` è calibrato per zoom 0.8 → funziona uguale in TS.

---

### D6 — Formula nDotL²: CORRETTA in TS (**nessun bug**)

C# usa `nDotL * diffuseLighting = nDotL * (nDotL * att) = nDotL² * att`.
TS usa `nDotL * nDotL * attenuation`. Identici.

---

### D7 — Y-flip normal map: CORRETTO in TS (**nessun bug**)

C#: `normal.y *= -1`. TS: `n.y = -n.y`. Identici.

---

### D8 — Ground normal RT clear color: INTENZIONALE, uguale in C# e TS

C#: `graphicsDevice.Clear(Color.Black)` prima di disegnare le ground normal map.
TS: `fill(0x000000)`. Stesso comportamento.
Le tile senza normal map sono scure → effetto atmosferico voluto.

---

## 4. WGSL/GLSL Blueprint — Shader corretto

Il NormalMapFilter attuale è matematicamente corretto per la parte nDotL².
Le modifiche necessarie sono **due**:

### 4.1 Aggiungere `uAmbientColor` allo shader (opzionale, per parità con C#)

```wgsl
struct NormalMapUniforms {
    uLightPosition: vec4<f32>,     // xy = screen pos, z = height
    uLightColor:    vec4<f32>,     // rgb = [0-1]
    uLightParams:   vec4<f32>,     // x = distanceSq, y = opacity
    uAmbientColor:  vec4<f32>,     // rgb = [0-1] ambient (aggiungi questo)
};

// Fragment:
let lit = diffuse.rgb * (
    normalMapUniforms.uAmbientColor.rgb +
    normalMapUniforms.uLightColor.rgb * nDotL * nDotL * attenuation * normalMapUniforms.uLightParams.y
);
return vec4<f32>(lit, diffuse.a);
```

Se si aggiunge questo, eliminare il pre-pass ambient tint in `ShadowSystem.ts`
(righe che creano `ambientSprite` con `ambientTint`).

### 4.2 Cambiamenti minimi per D1 (senza modificare il shader)

Con la fix di `normalMapAmbienceColor` in `LevelCustomization.ts` e la formula
attuale (ambient come pre-pass), il comportamento per i livelli 3+ diventa:
- Ambient = 0 → pre-pass produce una RT nera
- Per-light additive: tile lit = `tex * nDotL² * color * att * op`, tile dark = 0
- Contrasto forte → effetto 3D visibile

Per i livelli 1-2:
- Ambient = `(0.11, 0.10, 0.22)` → leggero blue-viola sulle tile non illuminate
- Per-light additive: aggiunge la componente direzionale
- C# avrebbe `N × (0.11,...)` — se il risultato è troppo buio, si può aumentare
  leggermente il valore o adottare la soluzione D2.

### 4.3 Uniform layout (invariato) — nessuna modifica al binding

```
group(0) binding(0) → GlobalFilterUniforms (PixiJS)
group(0) binding(1) → uTexture    (diffuse RT)
group(0) binding(2) → uSampler
group(1) binding(0) → normalMapUniforms
group(1) binding(1) → uNormalMap  (normal RT)
group(1) binding(2) → uNormalMapSampler
```

---

## 5. Piano di bugfix (ordinato per impatto)

| Priorità | Fix | File | Righe |
|----------|-----|------|-------|
| 🔴 CRITICO | D1: correggere `normalMapAmbienceColor` per livello | `Web/src/grid/LevelCustomization.ts` | 30 |
| 🟡 MEDIO | D2: spostare ambient DENTRO il filter per parità C# | `NormalMapFilter.ts` + `ShadowSystem.ts` | ~10 |
| 🟢 BASSO | D3: aggiungere specular + glow pass | `ShadowSystem.ts` | ~30 |
| 🟢 BASSO | D4: culling luci fuori schermo | `ShadowSystem.ts` | ~5 |

**Il fix D1 da solo dovrebbe risolvere sia "luci fioche" che "nessun effetto 3D".**
