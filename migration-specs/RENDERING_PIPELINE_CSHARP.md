# Rendering Pipeline — Mark In DeDark (C# / MonoGame originale)

Documento di riferimento per la migrazione a TypeScript/PixiJS.
Descrive meticolosamente ogni passo, ogni render target, ogni shader e ogni blend state
della pipeline originale XNA/MonoGame.

---

## Panoramica concettuale

Il gioco è un labirinto buio. La pipeline di rendering costruisce questo effetto attraverso
**quattro livelli concettuali** sovrapposti:

1. **Layer Pavimento** — La texture del pavimento, illuminata con normal mapping e specular
2. **Layer Ombre Software** — Il motore Penumbra genera ombre morbide con penombra reale
3. **Layer Scena** — Muri, personaggi, oggetti, illuminati con normal mapping
4. **Layer Effetti & HUD** — Particelle, neve, effetti nemici, interfaccia utente

Il tutto è governato da **16 RenderTarget2D** intermedi (14 nel codice principale + 2 interni
a Penumbra) e **6 shader HLSL** (2 custom + 4 Penumbra: Hull, Light, Shadow, Texture).

---

## Punto di ingresso: `Draw()` e `DrawPlayingScene()`

**File**: `Csharp/MarkInDeDark.Library/MarkInDeDarkGame.cs`

Il metodo `Draw()` (riga 861) distingue due scenari:

- **Game Over attivo**: chiama `DrawPlayingScene()` sul render target `_gameOverBackgroundRenderTarget`,
  poi disegna il player separatamente su `_gameOverForegroundRenderTarget`, e li compone
  sul `virtualDrawTarget` finale con fade alpha. Il player è disegnato sopra con
  `Color.White.WithAlpha(1 - _gameOverFadeObject.CurrentAlpha)`.
- **Gameplay normale**: chiama direttamente `DrawPlayingScene(spriteBatch, virtualDrawTarget, gameTime)`.

`DrawPlayingScene()` (riga 920) è il cuore dell'intera pipeline.

---

## Camera e zoom

**File**: `MarkInDeDarkGame.cs:241`

```csharp
_camera = new Camera2D(_matrixScaleProvider) { Zoom = 0.8f };
```

La camera ha un **zoom fisso di 0.8x** applicato alla viewMatrix. Questo valore scala tutte
le coordinate in `_camera.GetViewMatrix()`, che viene usata per:
- Il `transformMatrix` di SpriteBatch nei draw call con la camera
- Il `_penumbra.Transform` (aggiornato ogni frame in `Update()`)
- Il calcolo delle posizioni luce in screen-space (`WorldToScreenForLights`)

> **Per la migrazione**: la viewMatrix di PixiJS (`container.worldTransform`) deve replicare
> questo zoom 0.8x. Le posizioni luce screen-space devono essere calcolate post-zoom.

La risoluzione virtuale è **1920×1080** (`MarkInDeDarkBootstrap:116`).

---

## Matrice di proiezione per la Normal Map

**Metodo**: `GetProjectionMatrixForNormalMap()` (riga 905)

Crea una matrice ortografica per passare le coordinate mondo agli shader di normal mapping:

```csharp
// Codice reale: Matrix.Multiply(ref _worldMatrix, ref projection, out projection)
// = _worldMatrix * projection (con _worldMatrix = Identity, il risultato è identico)
Matrix.CreateOrthographicOffCenter(
    0, RealScreenWidth,
    RealScreenHeight, 0,   // Y invertito (asse Y verso il basso, tipico 2D)
    -1, 0
)
```

Viene calcolata una volta per livello durante l'inizializzazione.
Usata come parametro `ViewProjection` dello shader `circularnormalmap.fx`.

---

## FASE 0 — Preparazione shader normal map

**Riga 929–933 di `DrawPlayingScene()`**

Prima di qualsiasi draw call, si impostano i parametri globali del shader `_circularNormalMapShader`:

| Parametro        | Valore                  | Note                                        |
|------------------|-------------------------|---------------------------------------------|
| `AmbientColor`   | `_normalMapAmbienceColor` | Colore di base del livello (per-livello)  |
| `World`          | `Matrix.Identity`       | Nessuna trasformazione aggiuntiva           |
| `ViewProjection` | `_projectionMatrix`     | Matrice ortografica calcolata sopra         |

Dopo aver impostato i parametri, viene chiamato esplicitamente:
```csharp
_circularNormalMapShader.CurrentTechnique.Passes[0].Apply();
```

> **Critico per la migrazione**: questa chiamata committà lo stato GPU per `AmbientColor`,
> `World` e `ViewProjection`. Questi tre parametri **NON vengono mai re-impostati**
> nelle fasi successive (1E e 4): persistono per l'intera durata di `DrawPlayingScene()`.
> Solo `NormalTexture`, `LightPosition`, `LightColor`, `LightDistanceSquared` e `LightOpacity`
> vengono aggiornati per luce nel loop.

`_normalMapAmbienceColor` è configurato per livello tramite `LevelCustomization.AmbienceColor`.
Nei livelli iniziali è più chiaro (penombra), negli ultimi è nero puro (buio totale).

---

## FASE 1 — Pipeline del Pavimento con Normal Map

**Riga 936–986 di `DrawPlayingScene()`**
**Metodo**: `GameGrid.DrawGroundWithNormalMap()` (`GameGrid.cs` riga 803)

Questa è la sotto-pipeline più complessa. Produce `_groundLevelResultRenderTarget`, che
contiene il pavimento completamente illuminato con normal map e specular highlights,
prima ancora che vengano applicate le ombre Penumbra.

### Render Target coinvolti (in ordine di produzione)

| Nome RT                                                   | Contenuto                                     | Clear |
|-----------------------------------------------------------|-----------------------------------------------|-------|
| `_groundLevel0RenderTarget`                               | Texture base del pavimento                    | Nero  |
| `_groundLevelNormalMapRenderTarget`                       | Normal map del pavimento                      | Nero  |
| `_groundLevelSpecularMapRenderTarget`                     | Specular map del pavimento                    | Nero  |
| `_groundLevelSpecularMapLightMaskRenderTarget`            | Glow delle luci (maschera per specular)       | Nero  |
| `_groundLevelAppliedNormalMapRenderTarget`                | Pavimento + lighting calcolato dallo shader NM | Nero |
| `_groundLevelSpecularMapWithAppliedLightmaskRenderTarget` | Specular map modulata dal glow                | Nero  |
| `_groundLevelResultRenderTarget`                          | Composizione finale normal+specular+ombre     | Nero  |

---

> **Pattern `transformMatrix: viewMatrix`**: tutti i passi 1A–1D, la Fase 1-bis, la Fase 3A e la Fase 3B
> usano `SpriteBatch.Begin(transformMatrix: viewMatrix, ...)` — il contenuto di scena viene quindi
> disegnato in **spazio camera** (con zoom 0.8x e traslazione). Al contrario, i passi di
> lighting/compositing (1E, 1F, 1G composizione, Fase 4, Fase 5) **non** usano viewMatrix:
> disegnano full-screen quad su cui lo shader lavora in screen space direttamente.
> Questa asimmetria è fondamentale per la migrazione PixiJS.

### Passo 1A — Texture base pavimento

**Target**: `_groundLevel0RenderTarget`
**BlendState**: default (AlphaBlend)
**SamplerState**: PointClamp
**TransformMatrix**: `viewMatrix` (spazio camera)

Per ogni cella visibile nella viewport (`OnAllVisibleCells`):
- `c.DrawGround(spriteBatch, false)` — disegna la sprite del pavimento della cella
- Eventualmente disegna oggetti trigger sovrapposti al pavimento (`drawOtherGroundObjects`)

Il `false` nel parametro indica di disegnare il layer "base" (non i riempitivi decorativi).

---

### Passo 1B — Normal map del pavimento

**Target**: `_groundLevelNormalMapRenderTarget`
**BlendState**: default (AlphaBlend)
**SamplerState**: PointClamp
**TransformMatrix**: `viewMatrix` (spazio camera)

Per ogni cella visibile:
- `c.DrawGroundNormalMap(spriteBatch)` — disegna la texture di normal map della cella

Dopo il loop celle, viene chiamato anche `drawOtherGroundObjectsNormalMap(spriteBatch)`
che disegna le normal map dei **TriggerObject** presenti sul pavimento (stessa action passata
da `MarkInDeDarkGame` al metodo `DrawGroundWithNormalMap`). A differenza del Passo 1C
(specular), i trigger contribuiscono anche alla normal map.

La normal map codifica le normali superficiali nei canali RGB secondo la convenzione tangent-space standard:
- **R** → componente X della normale, range [-1,1] mappato in [0,255]
- **G** → componente Y della normale, range [-1,1] mappato in [0,255]
- **B** → componente Z della normale (≥ 128 per superfici rivolte verso la camera)

Colore neutro "piatta verso camera" = `(128, 128, 255)`.

---

### Passo 1C — Specular map del pavimento

**Target**: `_groundLevelSpecularMapRenderTarget`
**BlendState**: default (AlphaBlend)
**SamplerState**: PointClamp
**TransformMatrix**: `viewMatrix` (spazio camera)

Per ogni cella visibile:
- `c.DrawGroundSpecularMap(spriteBatch)` — disegna la texture di specular map della cella

Le celle con oggetti trigger **non** contribuiscono alla specular map.

La specular map definisce l'intensità dei riflessi punto per punto sulla superficie del pavimento.
Le zone bianche riflettono di più, le zone nere non riflettono.

---

### Passo 1D — Light mask per lo specular

**Target**: `_groundLevelSpecularMapLightMaskRenderTarget`
**BlendState**: `BlendState.NonPremultiplied`
**SamplerState**: PointClamp
**TransformMatrix**: `viewMatrix` (spazio camera)

Si disegnano i glow luminosi di tutte le sorgenti di luce attive:
- `_player.SurroundingLight?.DrawGlowForSpecular(s)`
- `e.SurroundingLight?.DrawGlowForSpecular(s)` per ogni nemico
- `l.SurroundingLight?.DrawGlowForSpecular(s)` per ogni braciere acceso
- `c.SurroundingLight?.DrawGlowForSpecular(s)` per ogni candela

`DrawGlowForSpecular()` (in `MarkLight.cs`) disegna la sprite `Effects/glow` centrata in
`Position + (0, 100)` (leggermente sotto la sorgente per simulare luce che cade sul pavimento).
La scala è `LevelCustomization.GlowScale`:
- **Livelli 1-2**: `GlowScale = 2f` (glow grande, livelli con penombra)
- **Livelli 3-8**: `GlowScale = 1f` (glow standard, livelli nel buio totale)

Il risultato è una **maschera luminosa** che indica dove la luce brilla sul pavimento.

---

### Passo 1E — Applicazione shader Normal Map al pavimento

**Target**: `_groundLevelAppliedNormalMapRenderTarget`
**BlendState**: `BlendState.Additive`
**SamplerState**: PointClamp
**Effect**: `_circularNormalMapShader` (`circularnormalmap.fx`)

Questo è il calcolo centrale del lighting. Per ogni luce nella lista `_normalMapLights`
(se `light.ShouldDraw == true`, cioè `IsEnabled && !IsOutOfScreen`):

```
normalMapEffect.Parameters["NormalTexture"]      = _groundLevelNormalMapRenderTarget
normalMapEffect.Parameters["LightPosition"]      = light.PositionForGround   (Vector3)
normalMapEffect.Parameters["LightColor"]         = light.ColorNormal          (Vector3)
normalMapEffect.Parameters["LightDistanceSquared"] = light.DistanceSquaredForGround
normalMapEffect.Parameters["LightOpacity"]       = light.Opacity

spriteBatch.Draw(_groundLevel0RenderTarget, Vector2.Zero, Color.White)  // 1 draw call per luce
```

**`PositionForGround`** = `Position + DeltaForGround` dove `DeltaForGround = (0, +100)` di default.
La luce è spostata in basso di 100px per simulare che illumina il pavimento "dall'alto" rispetto
alla sorgente visiva 2D (finto effetto di profondità Z).

Il risultato viene accumulato in **additive blending**: ogni luce aggiunge la propria contribuzione
all'illuminazione totale. Il target parte da nero (Clear) e si illumina a ogni draw call.

---

### Passo 1F — Applicazione light mask allo specular

**Target**: `_groundLevelSpecularMapWithAppliedLightmaskRenderTarget`
**Clear**: `Color.Black`
**BlendState**: default (`BlendState.AlphaBlend`) — non specificato esplicitamente nel codice
**Effect**: `_specularMapShader` (`specularmap.fx`)

```csharp
specularMapEffect.Parameters["SpecularTexture"].SetValue(specularMapLightmaskRenderTarget);
specularMapEffect.CurrentTechnique.Passes[0].Apply();   // chiamata esplicita prima del Begin
spriteBatch.Begin(effect: specularMapEffect);            // BlendState omesso → AlphaBlend di default
spriteBatch.Draw(specularMapRenderTarget, Vector2.Zero, Color.White);
```

Lo shader moltiplica pixel per pixel: `specularMapPixel * lightMaskPixel`.
Il risultato mostra i riflessi speculari **solo dove c'è luce**.

---

### Passo 1G — Composizione finale pavimento

**Target**: `_groundLevelResultRenderTarget`
**BlendState**: `BlendState.Additive`

Si sommano additivamente i due layer:
1. `_groundLevelAppliedNormalMapRenderTarget` — pavimento illuminato via normal map
2. `_groundLevelSpecularMapWithAppliedLightmaskRenderTarget` — riflessi speculari

Poi, in un **secondo SpriteBatch separato** (`BlendState` default AlphaBlend,
`transformMatrix: viewMatrix`, `samplerState: SamplerState.PointClamp`), si disegnano le ombre sprite
sempre sullo stesso `_groundLevelResultRenderTarget`:
- `_player.DrawShadows(s)` — silhouette d'ombra del player
- `en.DrawShadows(s)` per ogni nemico
- `brazierLit.DrawShadows(s)` per ogni braciere

**Nota**: queste ombre sono sprite 2D pre-renderizzate come silhouette, **non** ombre
calcolate geometricamente. Le ombre calcolate sono gestite da Penumbra (Fase 2).

---

## FASE 1-bis — Light mask addizionale

**Riga 988–1001 di `DrawPlayingScene()`**
**Target**: `_groundLevelAdditionalLightMaskRenderTarget`
**BlendState**: `BlendState.NonPremultiplied`
**Clear**: `Color.Transparent`
**TransformMatrix**: `viewMatrix` (spazio camera)

Separato dalla pipeline del pavimento, si disegna una seconda light mask con i glow di:
- Nemici: `e.SurroundingLight?.DrawLightMask(spriteBatch)`
- Bracieri: `l.SurroundingLight?.DrawLightMask(spriteBatch)`
- Candele: `c.SurroundingLight?.DrawLightMask(spriteBatch)`

`DrawLightMask()` usa la stessa sprite `Effects/glow` ma con scala fissa `2f`
(più grande del glow specular che usa `LevelCustomization.GlowScale`).
Posizione identica: `Position + (0, 100)`.

**Il player non è incluso qui.** La sua luce è gestita esclusivamente da Penumbra
(è l'unica luce con `castsShadows: true` e con `PenumbraLight.Enabled = true`).

**Importante**: nemici, candele e bracieri hanno `PenumbraLight.Enabled = false` (default nel
costruttore di `MarkLight`). Vengono aggiunti a `_penumbra.Lights` ma non contribuiscono al
rendering Penumbra diretto. La loro presenza visiva nella lightmap di Penumbra avviene
**esclusivamente** tramite questa light mask addizionale.

Questo render target viene poi additivamente blendato **dentro la lightmap di Penumbra** (Fase 2).

---

## FASE 2 — Penumbra: ombre soft con penombra reale

**Riga 1003–1020 di `DrawPlayingScene()`**

Usa il motore Penumbra, una libreria custom GPU-based per ombre 2D con transizione di penombra reale.

### Struttura dati

- **`Lights`**: `ObservableCollection<Light>` — ogni `MarkLight` espone un `PenumbraLight`
  che è un `PointLight` con `Scale = new Vector2(penumbraLightScale)`.
  Il player ha `Scale = Vector2(1200f)`, le candele e bracieri `Scale = Vector2(150f)`.
- **`Hulls`**: `HullList` — poligoni convessi che occludono la luce (muri, pilastri).
- **`AmbientColor`**: sovrascritta ogni livello da `LevelCustomization.PenumbraAmbientColor`:
  - **Livelli 1-2**: `new Color(104, 104, 104)` — grigio, ombre meno marcate (livelli con penombra)
  - **Livelli 3-8**: `Color.Black` — buio totale, massimo contrasto ombre
  (Il motore Penumbra ha default interno `Color.DarkSlateGray`, ma il gioco lo sovrascrive sempre.)
- **`Transform`**: impostato ogni frame in `Update()` con `viewMatrix` (`_penumbra.Transform = viewMatrix`),
  così Penumbra conosce la trasformazione camera corrente.

### Sequenza di rendering Penumbra

**Step 2.1 — `_penumbra.BeginDraw()`** → `PenumbraEngine.PreRender()`
- Salva i render target correnti (per ripristinarli dopo)
- Switcha il device su `Textures.DiffuseMap` (render target interno di Penumbra)
  tramite `Device.SetRenderTargets(Textures.DiffuseMapBindings)`

**Step 2.2 — Copia del pavimento nel DiffuseMap**
```csharp
_graphicsDevice.SetRenderTarget(_groundLevelRenderTarget);
_penumbra.BeginDraw();                         // PreRender → switch su DiffuseMap
_graphicsDevice.Clear(Color.Transparent);
spriteBatch.Begin();
spriteBatch.Draw(_groundLevelResultRenderTarget, Vector2.Zero, Color.White);
spriteBatch.End();
```
Il pavimento già illuminato (normal+specular) viene copiato nel DiffuseMap di Penumbra.
Nota: `SetRenderTarget(_groundLevelRenderTarget)` prima di `BeginDraw()` registra
`_groundLevelRenderTarget` come destinazione finale della composizione Penumbra.

**Step 2.3 — `_penumbra.Draw(gameTime)`** → `PenumbraEngine.RenderLightMap()`

```
// Init lightmap
Device.SetRenderTargets(Textures.LightmapBindings)
Device.Clear(AmbientColor | DepthBuffer | Stencil)  // il buio parte dall'ambient
Device.SamplerStates[0] = SamplerState.LinearClamp

// Per ogni luce abilitata che interseca la camera viewport:
for each light in Lights where (light.Enabled && !Hulls.Contains(light) && light.Intersects(Camera)):
    Camera.GetScissorRectangle(light) → Device.SetScissorRectangle
    ShadowRenderer.Render(light)   // passo ombre
    LightRenderer.Render(light)    // passo luce
```

**`ShadowRenderer.Render(light)`** — due sotto-passi:

*Sotto-passo ombre* — shader `PenumbraShadow.fx`, tecnica `Main`:
- BlendState `_bsShadow`:
  - `ColorWriteChannels = Alpha` (scrive **solo** il canale alpha)
  - `AlphaBlendFunction = ReverseSubtract`
  - `AlphaSourceBlend = One`, `AlphaDestinationBlend = One`
  - Effetto: sottrae dall'alpha corrente → zone d'ombra hanno alpha bassa
- Disegna i quad di proiezione ombra per ogni bordo degli hull visibili

*Sotto-passo hull fill* — shader `PenumbraHull.fx`, tecnica `Main`:
- BlendState `_bsHull`:
  - `ColorWriteChannels = Alpha`
  - `AlphaBlendFunction = Add`
  - `AlphaSourceBlend = One`, `AlphaDestinationBlend = Zero`
  - Effetto: ristabilisce alpha = 1 dentro gli hull stessi (evita auto-ombra)
- Disegna i poligoni degli hull come solidi

**`LightRenderer.Render(light)`** — shader `PenumbraLight.fx`, tecnica `PointLight`:
- BlendState `_bsLight`:
  - `ColorBlendFunction = Add`
  - `ColorSourceBlend = DestinationAlpha` → la luce viene modulata dall'alpha residua
    (aree in ombra hanno alpha bassa → pochissima luce passa)
  - `ColorDestinationBlend = One` → additive verso la lightmap
  - `AlphaBlendFunction = Add`
  - `AlphaSourceBlend = One`, `AlphaDestinationBlend = Zero` → azzera l'alpha per la prossima luce
- Disegna un quad leggermente più grande di `1/√2` (per gestire rotazioni camera)
- La luce ha falloff radiale lineare: `alpha = saturate(1 - distance * 2)`

La lightmap risultante contiene: `AmbientColor` nelle zone scure + colori luce nelle zone illuminate.

**Step 2.4 — Aggiunta della light mask addizionale alla lightmap**
```csharp
_graphicsDevice.SetRenderTarget(_penumbra.LightMap);   // lightmap interna Penumbra
spriteBatch.Begin(blendState: BlendState.Additive);
spriteBatch.Draw(_groundLevelAdditionalLightMaskRenderTarget, Vector2.Zero, Color.White);
spriteBatch.End();
```
Il glow di nemici, bracieri e candele viene **additivamente sommato** alla lightmap.
Questo aggiunge alone visivo oltre le ombre fisiche calcolate da Penumbra.

**Step 2.5 — `_penumbra.BlendAll()`** → `PenumbraEngine.BlendLightMap()`

Ripristina i render target originali.
Usa `LightMapRenderer` con shader `PenumbraTexture.fx`:
```
output = DiffuseMap.Sample(uv) * Lightmap.Sample(uv)
```
Il DiffuseMap (pavimento con normal+specular) viene moltiplicato per la lightmap
(buio + contribuzioni luci). Il risultato va nel render target corrente: `_groundLevelRenderTarget`.

---

## FASE 3A — Normal map di muri, pilastri e personaggi

**Riga 1022–1036 di `DrawPlayingScene()`**
**Target**: `_normalMapRenderTarget`
**Clear**: `_normalMapNeutralColor` = `new Color(128, 128, 255)` = normale neutra piatta
**SortMode**: `SpriteSortMode.FrontToBack`
**SamplerState**: PointClamp
**TransformMatrix**: `viewMatrix` (spazio camera)

In questo render target vengono composte le normal map di tutti gli elementi "verticali":

- `_gameGrid.DrawAllNormalMapsButGround(spriteBatch)` → `c.DrawNormalMapButGrounds(spriteBatch)`
  per ogni cella (muri, pilastri)
- `_player.DrawNormalMap(spriteBatch)` — normal map sprite del player
- `e.DrawNormalMap(spriteBatch)` per ogni nemico
- `littleLight.DrawNormalMap(spriteBatch)` per ogni braciere

Il clear con `(128, 128, 255)` garantisce che le aree senza normal map abbiano una normale
neutra (piatta, rivolta verso la camera) invece di zero/nero (che darebbe lighting nullo).

> **Oggetti senza normal map**: candele (`_candlesManager`), chiave (`_key`) e candle packs
> **non** vengono disegnati qui — appaiono solo nella Fase 3B (`_baseSceneRenderTarget`).
> Quando la Fase 4 applica il shader NM a questi pixel, usa la normale neutra `(128,128,255)`
> del clear: ricevono solo la luce ambientale di FASE 0 (`AmbientColor`), senza effetto 3D.
> Si tratta di un comportamento intenzionale: candele e chiave hanno un aspetto piatto.

---

## FASE 3B — Scena base (muri, oggetti, personaggi)

**Riga 1039–1073 di `DrawPlayingScene()`**
**Target**: `_baseSceneRenderTarget`
**Clear**: `Color.Transparent`
**SortMode**: `SpriteSortMode.FrontToBack`
**SamplerState**: PointClamp
**TransformMatrix**: `viewMatrix` (spazio camera)

Ordine di draw (Z-order dal più distante al più vicino, front-to-back):
1. `_gameGrid.DrawWalls(spriteBatch)` — muri e pilastri (in debug: `DrawForDebug`)
2. `_key.Draw(spriteBatch)` — chiave collezionabile
3. Candle packs — oggetti candela raccoglibili
4. `_gameGrid.DrawableObjects` — oggetti decorativi generici (`otherObject.Draw`)
5. Bracieri accesi (`brazierLit.Draw`)
6. `_candlesManager.Draw(spriteBatch)` — candele del player e nemici
7. `_player.Draw(spriteBatch)` — sprite player con animazione corrente
8. Nemici (`en.Draw`)
9. `_gameGrid.DrawRiempitiviGround(spriteBatch)` → `c.DrawGround(spriteBatch, true)` —
   decorazioni del pavimento che vanno **sopra** i personaggi (bordure, angoli, overlay)

---

## FASE 4 — Applicazione normal map a muri e personaggi

**Riga 1075–1107 di `DrawPlayingScene()`** (solo in modalità non-debug)
**Target**: `_baseSceneItemsWithNormalMapRenderTarget`
**BlendState**: `BlendState.Additive`
**SortMode**: `SpriteSortMode.Immediate`
**Effect**: `_circularNormalMapShader`
**Clear**: `Color.Transparent`

Stesso shader del pavimento, ma con parametri di posizione e distanza diversi per le superfici verticali:

```
normalMapEffect.Parameters["NormalTexture"] = _normalMapRenderTarget   // NM muri+chars
for each light in _normalMapLights (se light.ShouldDraw):
    normalMapEffect.Parameters["LightPosition"]       = light.PositionForWall
    normalMapEffect.Parameters["LightColor"]          = light.ColorNormal
    normalMapEffect.Parameters["LightOpacity"]        = light.Opacity
    normalMapEffect.Parameters["LightDistanceSquared"] = light.DistanceSquaredForWall
    spriteBatch.Draw(_baseSceneRenderTarget, Vector2.Zero, Color.White)  // 1 draw call per luce
```

**`PositionForWall`** = `Position + DeltaForWall` dove `DeltaForWall = (0, -60)` di default.
La luce è spostata di 60px **in su** rispetto alla posizione 2D della sorgente,
simulando che illumina le superfici verticali da un angolo più frontale.

**`DistanceSquaredForWall`**: per BrazierLit è `17_000f`, diverso da `DistanceSquaredForGround`
(`7_280f`). Questo regola il raggio di attenuation separatamente per le due superfici.

Il risultato in additive blending accumula tutte le contribuzioni luminose sulla scena.

---

## FASE 5 — Composizione finale

**Riga 1109–1134 di `DrawPlayingScene()`**
**Target**: `finalRenderTarget`
**Clear**: `Color.Black`

**Modalità normale** (non GodMode):
```csharp
spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, ...);
spriteBatch.Draw(_groundLevelRenderTarget, Vector2.Zero, Color.White);                   // 1° layer
spriteBatch.Draw(_baseSceneItemsWithNormalMapRenderTarget, Vector2.Zero, Color.White);   // 2° layer
spriteBatch.End();
```

**Modalità GodMode** (debug):
```csharp
spriteBatch.Begin();
spriteBatch.Draw(_groundLevelRenderTarget, Vector2.Zero, Color.White);
spriteBatch.Draw(_baseSceneRenderTarget, Vector2.Zero, Color.White.WithAlpha(0.2f));  // 20% alpha
spriteBatch.End();
```

---

## FASE 6 — Effetti sopra il buio

**Riga 1136–1142 di `DrawPlayingScene()`**
**Transform**: `_camera.GetViewMatrix()`
**SamplerState**: PointClamp

Disegnati sopra la composizione finale (visibili anche nel buio):
- `en.DrawEffectsOverDark(spriteBatch)` per ogni nemico (es. effetti di attacco, particelle)
- `_candlesManager.DrawEffectsOverDark(spriteBatch)` — effetti candele (flamme, particelle)
- `_snowingManager?.Draw(spriteBatch)` — neve (se abilitata per il livello)

---

## FASE 7 — HUD

**Riga 1144–1147 di `DrawPlayingScene()`**
Nessuna trasformazione camera (coordinate schermo assolute).

```csharp
spriteBatch.Begin();
_gameHud.Draw(spriteBatch);
spriteBatch.End();
```

---

## Tabella completa dei Render Target

| Nome                                                      | Dimensione  | Uso                                                |
|-----------------------------------------------------------|-------------|----------------------------------------------------|
| `_groundLevel0RenderTarget`                               | Back buffer | Texture base pavimento                             |
| `_groundLevelNormalMapRenderTarget`                       | Back buffer | Normal map pavimento                               |
| `_groundLevelSpecularMapRenderTarget`                     | Back buffer | Specular map pavimento                             |
| `_groundLevelSpecularMapLightMaskRenderTarget`            | Back buffer | Glow luci per specular highlight                   |
| `_groundLevelAppliedNormalMapRenderTarget`                | Back buffer | Pavimento illuminato via shader NM (additive)      |
| `_groundLevelSpecularMapWithAppliedLightmaskRenderTarget` | Back buffer | Specular modulato dalla light mask                 |
| `_groundLevelAdditionalLightMaskRenderTarget`             | Back buffer | Glow addizionale nemici/candele/bracieri           |
| `_groundLevelResultRenderTarget`                          | Back buffer | Pavimento finale (NM+specular+ombre sprite)        |
| `_groundLevelRenderTarget`                                | Back buffer | Pavimento + Penumbra blendato (output finale fase) |
| `_normalMapRenderTarget`                                  | Back buffer | NM muri+personaggi (clear neutro 128,128,255)      |
| `_baseSceneRenderTarget`                                  | Back buffer | Scena completa senza lighting (muri+chars+oggetti) |
| `_baseSceneItemsWithNormalMapRenderTarget`                | Back buffer | Scena con lighting NM applicato (additive)         |
| `_gameOverBackgroundRenderTarget`                         | Back buffer | Scena congelata per schermata game over            |
| `_gameOverForegroundRenderTarget`                         | Back buffer | Solo il player per schermata game over             |
| `Penumbra.DiffuseMap` (interno)                           | Viewport    | DiffuseMap interno Penumbra (riceve il pavimento)  |
| `Penumbra.LightMap` (interno)                             | Viewport    | LightMap calcolata da Penumbra                     |

---

## Sorgenti di Luce: la classe MarkLight

**File**: `Csharp/MarkInDeDark.Library/Lighting/MarkLight.cs`

Ogni sorgente luminosa è rappresentata da un `MarkLight` che incapsula sia la logica
per gli shader custom (normal map, specular) sia il `PointLight` per il motore Penumbra.

### Proprietà geometriche

| Proprietà           | Valore default  | Descrizione                                         |
|---------------------|-----------------|-----------------------------------------------------|
| `Position`          | —               | Posizione 2D nel mondo (aggiornata ogni frame)      |
| `DeltaForGround`    | `(0, +100)`     | Offset Z simulato verso il basso (pavimento)        |
| `DeltaForWall`      | `(0, -60)`      | Offset Z simulato verso l'alto (pareti) — **sovrascrivibile per entità** |
| `PositionForGround` | calcolata in `Update()` | **Screen space**: `Vector2.Transform(Position + DeltaForGround, viewMatrix)` + Z |
| `PositionForWall`   | calcolata in `Update()` | **Screen space**: `Vector2.Transform(Position + DeltaForWall, viewMatrix)` + Z |

> **Nota**: il componente **Z è identico** per `PositionForGround` e `PositionForWall` —
> entrambi usano `gameLight.Z` senza offset. Solo X e Y cambiano grazie ai delta.
> Codice di riferimento: `MarkInDeDarkGame.cs:794-808`.

> **Attenzione critica per la migrazione**: le posizioni luce passate allo shader
> sono in **screen space** (dopo applicazione della viewMatrix della camera), non in world space.
> Il calcolo avviene in `MarkInDeDarkGame.Update()` tramite `WorldToScreenForLights()` = `Vector2.Transform()`.
> Questo è coerente con il fatto che anche i vertici della sprite batch non usano viewMatrix
> nel passo NM (SpriteBatch senza transformMatrix), quindi entrambi sono nello stesso spazio.

**Override DeltaForWall per entità specifiche**:
- **Ghost**: `DeltaForWall = (0, -30)` — luce più vicina al centro (Ghost.cs:71)
- **BrazierLit**: `DeltaForWall = (0, +4)` — luce quasi a livello del pavimento (BrazierLit.cs:72)

### Proprietà luminose

| Proprietà                  | Descrizione                                              |
|----------------------------|----------------------------------------------------------|
| `ColorNormal`              | `Vector3` RGB, aggiornato ogni frame dall'animazione     |
| `Opacity`                  | Moltiplicatore intensità [0,1]                           |
| `DistanceSquaredForGround` | Raggio² attenuation per lo shader del pavimento          |
| `DistanceSquaredForWall`   | Raggio² attenuation per lo shader dei muri — **inizializzato uguale a `ForGround`**; solo BrazierLit lo sovrascrive a 17_000f |
| `PenumbraLight`            | `PointLight` di Penumbra, `Scale = Vector2(penumbraLightScale)`, **`Enabled = false` di default** |
| `IsEnabled`                | Abilita/disabilita la luce completamente                 |
| `IsGlowEnabled`            | Abilita/disabilita solo il disegno del glow sprite       |
| `IsOutOfScreen`            | Culling: non disegna se fuori dalla viewport             |
| `ShouldDraw`               | `IsEnabled && !IsOutOfScreen`                            |

> **Solo il Player** imposta `PenumbraLight.Enabled = true` (Player.cs:76) e `PenumbraLight.CastsShadows = true`.
> Tutti gli altri hanno il `PenumbraLight` disabilitato — partecipano alla lightmap Penumbra solo
> tramite la light mask addizionale (Fase 1-bis).

### Animazione colore

Il colore oscilla continuamente tra `firstAnimationColor` e `secondAnimationColor`
tramite `ColorChangingObject` con velocità `2.5`. Simula il flickering di torce/candele.
L'animazione aggiorna sia `ColorNormal` (per gli shader NM/specular) sia `PenumbraLight.Color`.

### Posizione del PenumbraLight

```csharp
PenumbraLight.Position = Position + new Vector2(-10f, 75f);
```

Il `PointLight` di Penumbra è posizionato 10px a sinistra e 75px sotto la posizione 2D della sorgente.

### Parametri per tipo di sorgente

| Entità           | `z`   | `penumbraLightScale` | `distanceSquared` (Ground=Wall salvo note) | `opacity` | `castsShadows` | `PenumbraLight.Enabled` |
|------------------|-------|----------------------|-------------------------------------------|-----------|----------------|-------------------------|
| Player           | 90f   | 1200f                | 20_600f                                   | 1f        | `true`         | `true`                  |
| Goblin           | 46f   | 300f                 | 4_950f                                    | 0.5f      | `false`        | `false`                 |
| Zombie           | 46f   | 300f                 | 4_950f                                    | 0.5f      | `false`        | `false`                 |
| Ghost            | 46f   | 300f                 | 4_950f                                    | 1f        | `false`        | `false`                 |
| Skeletron        | 46f   | 300f                 | 4_950f                                    | 0.5f      | `false`        | `false`                 |
| Taurus           | 46f   | 300f                 | 4_950f                                    | 0.5f      | `false`        | `false`                 |
| **Bubu**         | —     | —                    | —                                         | —         | —              | — (**nessuna luce**)    |
| Candele          | 67f   | 150f                 | 7_280f                                    | 1f        | `false`        | `false`                 |
| BrazierLit       | 67f   | 150f                 | Ground: 7_280f / **Wall: 17_000f**        | 1f        | `false`        | `false`                 |

> **Bubu** ha `characterTorchConfiguration: null` (`Bubu.cs:32`): nessun `SurroundingLight`,
> non contribuisce alla pipeline NM né alla lightmap Penumbra. È l'unico nemico senza luce.

La colonna `z` rappresenta la componente Z passata come `LightPosition.z` allo shader.
Simula la profondità della sorgente: valori più alti = luce più "in aria" = angolo di incidenza
più frontale sulle superfici verticali, meno differenziato sull'asse X/Y.

Note override per entità specifiche:
- **Ghost**: `DeltaForWall = (0, -30)` invece del default `(0, -60)` — Ghost.cs:71
- **BrazierLit**: `DeltaForWall = (0, +4)` invece del default `(0, -60)` — BrazierLit.cs:72
- **Candle**: la posizione iniziale è `WorldCenter + (0, -55)` (Candle.cs:56).
  La posizione del glow per specular è `Position + (0, +100)` = `WorldCenter + (0, +45)`.
  Diversamente dal player e dai nemici (il cui glow arriva a `Position + (0, +100)` direttamente),
  le candele emettono il glow **45px sotto** il loro centro mondo, non 100px.
  Il `DeltaForGround = (0, +100)` si applica a `Position` (già -55) in screen-space transform.
- Le **candele** sono aggiunte a `_normalMapLights` e a `_penumbra.Lights` dinamicamente al
  momento del piazzamento (CandlesManager.cs:145), non durante l'inizializzazione del livello.

---

## Shader 1: circularnormalmap.fx — Analisi completa

**File**: `Csharp/MarkInDeDark.Game/Assets/ContentBuilder/Effects/circularnormalmap.fx`
**Shader model**: `vs_3_0` / `ps_3_0` (DirectX 9 Shader Model 3, HLSL)
**Tecnica**: `PointLightNormalMap`

### Parametri (uniforms)

```hlsl
float3 LightPosition;           // commento originale dice "World Space" ma è ERRATO:
                                // in pratica viene passata la posizione in SCREEN space (post viewMatrix) + Z
float3 LightColor = 1.0;        // colore RGB della luce (default bianco puro)
float3 AmbientColor = 0.35;     // luce ambientale (default shader, sovrascritta da LevelCustomization)
float  LightDistanceSquared;    // raggio² per il calcolo di attenuation
float  LightOpacity = 1.0;      // moltiplicatore globale intensità

float4x4 World;                 // matrice World (in uso: Identity)
float4x4 ViewProjection;        // matrice ortografica viewport

sampler2D ScreenTexture : register(s0);  // texture base (pavimento o scena)
texture   NormalTexture;
sampler2D NormalSampler = sampler_state { Texture = <NormalTexture>; };
```

### Vertex Shader

Input:
- `float4 Position : POSITION0` — posizione locale vertex
- `float2 TexCoords : TEXCOORD0`
- `float4 Color : COLOR0`

```hlsl
VertexShaderOutput VS(VertexShaderInput input) {
    float4 pos = mul(input.Position, World);
    output.PosWorld = pos;                      // world space → interpolato al PS
    output.Position = mul(pos, ViewProjection); // clip space
    output.TexCoords = input.TexCoords;
    output.Color = input.Color;
    return output;
}
```

La posizione world space viene passata al pixel shader tramite `TEXCOORD1`.

### Pixel Shader

```hlsl
float4 PS(VertexShaderOutput input) : COLOR0 {
    float3 posWorld = input.PosWorld.xyz;

    // Vettore direzione luce: dal pixel verso la sorgente (normalizzato)
    float3 lightdir = normalize(posWorld - LightPosition);

    // Campionamento texture base
    float4 tex = tex2D(ScreenTexture, input.TexCoords);

    // Decodifica normal map tangent-space: [0,1] → [-1,1]
    float3 normal = normalize((2 * tex2D(NormalSampler, input.TexCoords).xyz) - 1);
    normal.y *= -1;   // flip Y: MonoGame ha Y verso il basso, normal map assume Y verso l'alto

    // Diffuse Lambertian: coseno tra normale e direzione luce invertita
    float diffuseLighting = saturate(dot(normal, -lightdir));

    // Attenuation inverse-square: LightDistanceSquared / |lightVector|²
    // → quando distance = sqrt(LightDistanceSquared), attenuation = 1.0
    float3 lightVector = LightPosition - posWorld;
    diffuseLighting *= (LightDistanceSquared / dot(lightVector, lightVector));

    // Calcolo lightAmount separato (double-saturate per sicurezza)
    float lightAmount = saturate(dot(normal, -lightdir));

    // Output: colore sprite modulato per (ambient + contribuzione luce)
    input.Color.rgb *= AmbientColor + (lightAmount * LightColor * diffuseLighting * LightOpacity);
    return input.Color * tex;
}
```

**Dettaglio flip Y**: `normal.y *= -1` compensa l'inversione dell'asse Y in MonoGame.
In XNA/MonoGame l'asse Y cresce verso il basso, mentre le normal map standard
(e la matematica di lighting) assumono Y verso l'alto. Senza flip, le zone
illuminate e quelle in ombra sarebbero specchiate verticalmente.

**Dettaglio attenuation**: il fattore `LightDistanceSquared / dot(lightVector, lightVector)`
è una legge di attenuazione inversa del quadrato scalata. Non scende a zero in modo netto:
a distanza maggiore di `sqrt(LightDistanceSquared)` il valore è < 1.0 (attenuazione),
ma `diffuseLighting *= ...` lo lascia agire moltiplicativamente sul termine diffuse.

---

## Shader 2: specularmap.fx — Analisi completa

**File**: `Csharp/MarkInDeDark.Game/Assets/ContentBuilder/Effects/specularmap.fx`
**Shader model**: `ps_3_0` (solo pixel shader, nessun vertex shader esplicito)
**Tecnica**: `Technique1`

```hlsl
sampler s0;                      // binding automatico SpriteBatch → registro s0

texture SpecularTexture;
sampler lightSampler : register(s1) = sampler_state { Texture = <SpecularTexture>; };

float4 PixelShaderFunction(
    float4 pos    : SV_Position,
    float4 color1 : COLOR0,
    float2 coords : TEXCOORD0) : COLOR0
{
    float4 color      = tex2D(s0, coords);          // testo specular map
    float4 lightColor = tex2D(lightSampler, coords); // testo light mask glow
    return color * lightColor;                      // moltiplicazione diretta
}
```

**Come viene usato in pratica**:
- `s0` riceve `_groundLevelSpecularMapRenderTarget` (la specular map del pavimento)
- `SpecularTexture` è impostato a `_groundLevelSpecularMapLightMaskRenderTarget` (il glow)
- La moltiplicazione `specular * glow` produce i riflessi visibili solo dove c'è luce

---

## Shader 3: PenumbraShadow.fx — Analisi completa

**File**: `Csharp/MarkInDeDark.Shadows/Content/PenumbraShadow.fx`
**Shader model**: `vs_4_0_level_9_1` / `ps_4_0_level_9_1` (via macro `TECHNIQUE`)
**Tecniche**: `Main`, `Debug`

### Parametri

```hlsl
cbuffer cbConstant  { float4 Color; }                                    // usato solo in debug
cbuffer cbPerFrame  { float4x4 ViewProjection; }
cbuffer cbPerObject { float2 LightPosition; float LightRadius; }
```

### Vertex Input — una struttura per ogni bordo di hull

```hlsl
struct VertexIn {
    float2 SegmentA : TEXCOORD0;    // vertice A del bordo dell'hull occludente
    float2 SegmentB : TEXCOORD1;    // vertice B del bordo dell'hull occludente
    float2 Stencil  : SV_POSITION0; // X ∈ {0,1}: seleziona seg A o B
                                    // Y ∈ {0,1}: 0=posizione sull'edge, 1=proiettato
};
```

Ogni bordo genera 4 vertici (un quad) con combinazioni `(Stencil.x, Stencil.y)` = (0,0),(1,0),(0,1),(1,1).

### Vertex Shader — Costruzione penombra

```hlsl
VertexOut VS(VertexIn vin) {
    float2 toSegmentA = vin.SegmentA - LightPosition;
    float2 toSegmentB = vin.SegmentB - LightPosition;

    // Offset perpendicolari, scalati per LightRadius (ampiezza penombra)
    // CCW 90°: (x,y) → (-y,x)
    float2 toLightOffsetA = float2(-LightRadius, LightRadius) * normalize(toSegmentA).yx;
    float2 toLightOffsetB = float2( LightRadius,-LightRadius) * normalize(toSegmentB).yx;
    float2 lightOffsetA = LightPosition + toLightOffsetA;  // sorgente spostata CCW
    float2 lightOffsetB = LightPosition + toLightOffsetB;  // sorgente spostata CW

    // Selezione vertice del quad tramite Stencil.x
    float2 position         = lerp(vin.SegmentA, vin.SegmentB, vin.Stencil.x);
    float2 projectionOffset = lerp(lightOffsetA, lightOffsetB, vin.Stencil.x);

    // projected.w = 1 - Stencil.y:
    //   Stencil.y=0 → w=1, vertice sull'edge (non proiettato)
    //   Stencil.y=1 → w=0, divisione prospettica porta all'infinito (volume d'ombra)
    float4 projected = float4(position - projectionOffset * vin.Stencil.y, 0.0, 1.0 - vin.Stencil.y);

    float4 clipPosition = mul(projected, ViewProjection);

    // Coordinate penombra: interpolazione nello spazio del triangolo di penombra
    float2 penumbraA = mul(projected.xy - vin.SegmentA * projected.w,
                           Invert(float2x2(toLightOffsetA, toSegmentA)));
    float2 penumbraB = mul(projected.xy - vin.SegmentB * projected.w,
                           Invert(float2x2(toLightOffsetB, toSegmentB)));

    // Backface clipping: normale CW 90° del bordo
    float2 clipNormal = (vin.SegmentB - vin.SegmentA).yx * float2(1.0, -1.0);
    float  clipValue  = dot(clipNormal, projected.xy - projected.w * position);
    // clipValue < 0 → bordo rivolto verso la luce, non deve generare ombra

    vout.Position  = clipPosition;
    vout.Penumbra  = float4(penumbraA, penumbraB);
    vout.ClipValue = clipValue;
}
```

### Pixel Shader — Interpolazione penombra Hermite

```hlsl
float4 PS(VertexOut pin) : SV_TARGET {
    clip(pin.ClipValue);   // scarta pixel dei bordi rivolti verso la luce

    // Normalizza in [-1,1]: dentro l'ombra dura → -1, fuori → +1
    float2 p = clamp(pin.Penumbra.xz / pin.Penumbra.yw, -1.0, 1.0);

    // Hermite cubico smooth: f(p) = p*(3 - p²)*0.25 + 0.5
    // Va da 0.0 (luce piena) a 1.0 (ombra piena) in modo continuo
    // Se Penumbra.yw <= 0 (proiezione non valida) → forza a 1.0 (piena occlusione)
    float2 value = lerp(p * (3.0 - p * p) * 0.25 + 0.5, 1.0, step(pin.Penumbra.yw, 0.0));

    float occlusion = value.x + value.y - 1.0;   // [0,1]: 0=luce, 1=ombra totale
    return float4(0.0, 0.0, 0.0, occlusion);     // solo canale alpha
}
```

Il valore di occlusione finisce nell'**alpha channel** della lightmap.
Il `ShadowRenderer` usa `_bsShadow` con `AlphaBlendFunction = ReverseSubtract`:
l'alpha della lightmap viene ridotta nelle zone d'ombra, e il successivo `LightRenderer`
usa `ColorSourceBlend = DestinationAlpha` per disegnare la luce solo dove alpha è alta.

---

## Shader 4: PenumbraLight.fx — Analisi completa

**File**: `Csharp/MarkInDeDark.Shadows/Content/PenumbraLight.fx`
**Shader model**: `vs_4_0_level_9_1` / `ps_4_0_level_9_1`
**Tecniche**: `PointLight`, `Spotlight`, `TexturedLight`, `DebugLight`

### Parametri

```hlsl
Texture2D    Texture;
SamplerState TextureSampler;

cbuffer cbConstant    { float4 Color; }                         // debug
cbuffer cbPerLight    {
    float4x4 WorldViewProjection;
    float3   LightColor;
    float    LightIntensity;
}
cbuffer cbPerSpotlight {
    float ConeHalfAngle;
    float ConeDecay;
}
```

### Vertex Shader (condiviso)

```hlsl
VertexOut VS(VertexIn vin) {
    vout.Position = mul(float4(vin.Position, 0.0, 1.0), WorldViewProjection);
    vout.TexCoord = vin.TexCoord;
}
```

Il quad viene costruito dal `LightRenderer` con vertici leggermente più grandi di `1/√2 ≈ 0.707`
per coprire la rotazione della camera senza tagliare il cono di luce.

### Helper colore

```hlsl
float4 GetComputedColor(float alpha) {
    alpha = abs(alpha);
    float3 lightColor = LightColor * alpha;
    lightColor = pow(abs(lightColor), LightIntensity);  // curva gamma-like per LightIntensity
    return float4(lightColor, 1.0);
}
```

### Tecnica PointLight (usata nel gioco per tutte le luci)

```hlsl
float4 PSPointLight(VertexOut pin) : SV_TARGET {
    // TexCoord in [0,1]: il centro è (0.5, 0.5)
    float halfMagnitude = length(pin.TexCoord - float2(0.5, 0.5));
    float alpha = saturate(1.0 - halfMagnitude * 2.0);  // falloff lineare: 1 al centro, 0 al bordo
    return GetComputedColor(alpha);
}
```

### Tecnica Spotlight (non usata nel gioco ma disponibile)

```hlsl
float4 PSSpotLight(VertexOut pin) : SV_TARGET {
    float2 lightVector = pin.TexCoord - float2(0.0, 0.5);
    float  magnitude   = length(lightVector);
    float2 lightDir    = lightVector / magnitude;

    float halfAngle = acos(dot(lightDir, float2(1.0, 0.0)));  // angolo dal centro cono
    float occlusion          = step(halfAngle, ConeHalfAngle); // fuori dal cono: 0
    float distanceAttenuation = saturate(1.0 - magnitude);
    float coneAttenuation     = 1.0 - pow(abs(halfAngle / ConeHalfAngle), ConeDecay);
    float alpha = distanceAttenuation * coneAttenuation;
    return GetComputedColor(alpha * occlusion);
}
```

---

## Shader 5: PenumbraTexture.fx — Analisi completa

**File**: `Csharp/MarkInDeDark.Shadows/Content/PenumbraTexture.fx`
**Shader model**: `vs_4_0_level_9_1` / `ps_4_0_level_9_1`
**Tecnica**: `Main`

```hlsl
Texture2D    DiffuseMap;
Texture2D    Lightmap;
SamplerState TextureSampler;

VertexOut VS(VertexIn vin) {
    // Pass-through: clip space diretto senza trasformazione
    vout.Position = float4(vin.Position, 0.0, 1.0);
    vout.TexCoord = vin.TexCoord;
}

float4 PS(VertexOut pin) : SV_TARGET {
    float4 diffuse = DiffuseMap.Sample(TextureSampler, pin.TexCoord);
    float4 light   = Lightmap.Sample(TextureSampler, pin.TexCoord);
    return diffuse * light;  // moltiplicazione diretta: luce scura → scurisce il diffuse
}
```

Composizione finale di Penumbra:
- **DiffuseMap** = il pavimento con normal+specular (copiato in Step 2.2)
- **Lightmap** = distribuzione delle luci calcolata dal motore Penumbra
- Output: il pavimento correttamente illuminato con ombre

---

## Shader 6: PenumbraHull.fx — Analisi

**File**: `Csharp/MarkInDeDark.Shadows/Content/PenumbraHull.fx`
**Shader model**: `vs_4_0_level_9_1` / `ps_4_0_level_9_1`
**Tecnica**: `Main`, `Debug`

Usato nel sotto-passo "hull fill" di `ShadowRenderer.Render()`.
Non calcola shading, serve solo a **ristabilire alpha = 1** all'interno dei poligoni hull
(muri, pilastri) dopo che `PenumbraShadow.fx` ha sottratto alpha nelle zone d'ombra.
Senza questo passo, gli hull stessi sembrerebbero in ombra (auto-ombra errata).

### BlendState (`_bsHull`)
```
ColorWriteChannels = Alpha          // scrive SOLO il canale alpha
AlphaBlendFunction = Add
AlphaSourceBlend   = One
AlphaDestinationBlend = Zero        // sovrascrive l'alpha con 1 (il PS restituisce 1.0)
```

### Vertex/Pixel Shader
- VS: trasforma i vertici degli hull in clip space con `ViewProjection`
- PS: restituisce `float4(0.0, 0.0, 0.0, 1.0)` — nessun colore, solo alpha piena

---

## Macros.fxh — Astrazione shader model

**File**: `Csharp/MarkInDeDark.Shadows/Content/Macros.fxh`

Fornisce astrazione tra SM4 (DX11/XNA4) e SM2 (DX9/XNA3.1):

```hlsl
#ifdef SM4
    // DX11: cbuffer, Texture2D, register(t#/s#/b#)
    #define TECHNIQUE(name, vsname, psname) \
        technique name { pass { \
            VertexShader = compile vs_4_0_level_9_1 vsname(); \
            PixelShader  = compile ps_4_0_level_9_1 psname(); } }
    #define DECLARE_TEXTURE(Name, index) \
        Texture2D<float4> Name : register(t##index); \
        sampler Name##Sampler : register(s##index)
    #define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#else
    // DX9: globali, sampler2D, register(s#)
    #define TECHNIQUE(name, vsname, psname) \
        technique name { pass { \
            VertexShader = compile vs_2_0 vsname(); \
            PixelShader  = compile ps_2_0 psname(); } }
    #define DECLARE_TEXTURE(Name, index)  sampler2D Name : register(s##index)
    #define SAMPLE_TEXTURE(Name, texCoord)  tex2D(Name, texCoord)
#endif
```

**Nota**: `circularnormalmap.fx` e `specularmap.fx` **non usano queste macro** e compilano
direttamente per `vs_3_0`/`ps_3_0`. Sono shader indipendenti dal sistema Penumbra.

---

## Tabella riassuntiva Blend State

| Contesto                                    | ColorSrc      | ColorDst     | AlphaSrc | AlphaDst | ColorFunc | AlphaFunc       |
|---------------------------------------------|---------------|--------------|----------|----------|-----------|-----------------|
| Normal map pav. (additive accumulo)         | One           | One          | One      | One      | Add       | Add             |
| Specular light mask (NonPremultiplied)      | SrcAlpha      | InvSrcAlpha  | SrcAlpha | InvSrcAlpha | Add    | Add             |
| Composizione NM+specular (additive)         | One           | One          | One      | One      | Add       | Add             |
| Penumbra `_bsShadow` (solo alpha, ombra)    | —             | —            | One      | One      | —         | ReverseSubtract |
| Penumbra `_bsHull`  (solo alpha, fill hull) | —             | —            | One      | Zero     | —         | Add             |
| Penumbra `_bsLight` (colore luce + alpha)   | DestAlpha     | One          | One      | Zero     | Add       | Add             |
| Glow additivo su lightmap Penumbra          | One           | One          | One      | One      | Add       | Add             |
| Composizione finale (AlphaBlend normale)    | SrcAlpha      | InvSrcAlpha  | One      | InvSrcAlpha | Add    | Add             |

---

## Flusso dati — Diagramma

```
══════════════════════════════════════════════════════════════
 PIPELINE PAVIMENTO (DrawGroundWithNormalMap)
══════════════════════════════════════════════════════════════

[Celle visibili]
  DrawGround(false)       → _groundLevel0RT           (clear nero)
  DrawGroundNormalMap()   → _groundLevelNormalMapRT    (clear nero)
  DrawGroundSpecularMap() → _groundLevelSpecularMapRT  (clear nero)

[Tutte le luci: player, nemici, bracieri, candele]
  DrawGlowForSpecular()   → _groundLevelSpecularMapLightMaskRT (NonPremultiplied)

[Shader circularnormalmap.fx, NormalTexture=NormalMapRT]
  per ogni luce (ShouldDraw):
    LightPosition=PositionForGround, LightColor, LightDistanceSquared(Ground), Opacity
    Draw(groundLevel0RT) → accumula in _groundLevelAppliedNormalMapRT (Additive)

[Shader specularmap.fx, SpecularTexture=LightMaskRT]
  Draw(specularMapRT) → _groundLevelSpecularMapWithAppliedLightmaskRT

[Additive blend]
  groundLevelAppliedNormalMapRT
  + groundLevelSpecularMapWithAppliedLightmaskRT
  → _groundLevelResultRT (clear nero)

[Sprite ombre 2D: player, nemici, bracieri]
  DrawShadows() sopra _groundLevelResultRT (default blend)

══════════════════════════════════════════════════════════════
 PIPELINE LIGHT MASK ADDIZIONALE
══════════════════════════════════════════════════════════════

[Nemici + bracieri + candele]
  DrawLightMask() → _groundLevelAdditionalLightMaskRT (NonPremultiplied, clear transparent)

══════════════════════════════════════════════════════════════
 PIPELINE PENUMBRA
══════════════════════════════════════════════════════════════

penumbra.BeginDraw()
  → salva RT corrente (_groundLevelRenderTarget)
  → switch GraphicsDevice su Penumbra.DiffuseMap

Draw(groundLevelResultRT) → Penumbra.DiffuseMap  (copia pavimento illuminato)

penumbra.Draw() → RenderLightMap()
  lightmap.Clear(AmbientColor)  ← il "buio base"
  per ogni luce:
    Scissor = bounding rect luce
    ShadowRenderer (PenumbraShadow.fx):
      bsShadow → sottrae alpha nelle zone d'ombra (ReverseSubtract)
      bsHull   → ristabilisce alpha dentro gli hull (Add, One, Zero)
    LightRenderer (PenumbraLight.fx, PointLight):
      bsLight  → aggiunge luce * DestAlpha (solo aree con alpha alta)
               → azzera alpha per prossimo giro (AlphaDst=Zero)
  → Penumbra.LightMap

[Additive blend]
  Draw(additionalLightMaskRT) → Penumbra.LightMap  (glow nemici/candele/bracieri)

penumbra.BlendAll() → LightMapRenderer (PenumbraTexture.fx)
  Penumbra.DiffuseMap * Penumbra.LightMap → _groundLevelRenderTarget

══════════════════════════════════════════════════════════════
 PIPELINE SCENA (muri, personaggi, oggetti)
══════════════════════════════════════════════════════════════

[Celle + player + nemici + bracieri]
  DrawNormalMapButGrounds(), DrawNormalMap()
  → _normalMapRT  (clear Color(128,128,255))

[Muri + oggetti + candele + player + nemici + riempitivi]
  → _baseSceneRT  (clear transparent, FrontToBack sort)

[Shader circularnormalmap.fx, NormalTexture=normalMapRT]
  per ogni luce (ShouldDraw):
    LightPosition=PositionForWall, LightDistanceSquared(Wall)
    Draw(baseSceneRT) → accumula in _baseSceneItemsWithNormalMapRT (Additive)

══════════════════════════════════════════════════════════════
 COMPOSIZIONE FINALE
══════════════════════════════════════════════════════════════

finalRenderTarget.Clear(Black)
Draw(_groundLevelRenderTarget)                  → pavimento + Penumbra
Draw(_baseSceneItemsWithNormalMapRenderTarget)  → scena + lighting NM
                                                  (AlphaBlend)

[Effects over dark: camera transform]
  enemy effects + candles + snow → finalRT

[HUD: nessuna camera transform]
  GameHUD → finalRT
```

---

## Note implementative per la migrazione PixiJS

1. **Il flip Y della normal map** (`normal.y *= -1`) è necessario anche in WebGL/WebGPU
   se le coordinate texture Y partono dall'alto (come in PixiJS di default).

2. **Additive blending multi-luce**: il ciclo "una draw call per luce" con blending additivo
   è il pattern core. In PixiJS va riprodotto con un render texture e un filtro custom
   per ogni luce, accumulando con `BLEND_MODES.ADD`.

3. **Il parametro `LightDistanceSquared`**: non è il quadrato della distanza della luce,
   è un parametro di scala per l'attenuazione. Corrisponde al quadrato del raggio in cui
   l'attenuazione vale esattamente 1.0.

4. **Le due posizioni** (`PositionForGround` e `PositionForWall`) simulano la profondità Z:
   la stessa luce 2D viene passata con offset Y diverso per pavimento e pareti.
   **Entrambe sono in screen space** (post viewMatrix), non world space — vedi sezione MarkLight.

5. **Il motore Penumbra** gestisce solo la luce del player (l'unica con `castsShadows=true`
   e `PenumbraLight.Enabled=true`). Le luci di nemici, candele e bracieri hanno il PenumbraLight
   disabilitato: la loro presenza nella lightmap viene dalla light mask addizionale (Fase 1-bis).

6. **Penumbra AmbientColor per livello**: non è fisso ma dipende da `LevelCustomization`:
   - Livelli 1-2: `Color(104, 104, 104)` — grigio, penombra visibile anche lontano dalle luci
   - Livelli 3-8: `Color.Black` — buio assoluto fuori dai coni di luce

7. **Candele: aggiunta dinamica**: le candele vengono aggiunte alla lista `_normalMapLights`
   e a `_penumbra.Lights` al momento del piazzamento dal player, non all'inizializzazione.
   Questo significa che il numero di luci nella pipeline cresce durante il gioco.

8. **AmbientColor nello shader NM**: il parametro `AmbientColor` di `circularnormalmap.fx`
   viene moltiplicato per il colore della sprite e sommato alla contribuzione luminosa.
   Con blending additivo e N luci, il termine ambient si accumula N volte.
   - Livelli 1-2: `AmbientColor = (0.11, 0.10, 0.22)` → contribuisce N volte (accettabile)
   - Livelli 3-8: `AmbientColor = Vector3.Zero` → termine ambient = 0, nessun accumulo
   L'implementazione PixiJS attuale (`NormalMapFilter.ts`) **non include AmbientColor**,
   il che è corretto per i livelli 3-8 ma differisce per i livelli 1-2.

9. **Camera zoom 0.8x**: la `Camera2D` ha `Zoom = 0.8f` fisso. Le posizioni luce
   in screen-space vengono calcolate post-zoom (`Vector2.Transform` con viewMatrix scalata).
   La risoluzione virtuale è 1920×1080 ma il contenuto è visualizzato all'80% di quella.

10. **Bubu non ha luce**: è l'unico nemico con `characterTorchConfiguration: null`.
    Non partecipa a nessun passo della pipeline NM né alla lightmap Penumbra.

11. **Formula lighting effettiva: coseno AL QUADRATO, non Lambertian standard** — questo è
    il dettaglio più critico per riprodurre il look visivo esatto.

    Nel pixel shader (`circularnormalmap.fx` righe 63-69), il dot product viene calcolato DUE VOLTE:
    ```hlsl
    float diffuseLighting = saturate(dot(normal, -lightdir));            // [1] Lambertian
    diffuseLighting *= (LightDistanceSquared / dot(lightVector, lightVector)); // × attenuation
    float lightAmount  = saturate(dot(normal, -lightdir));               // [2] SECONDO Lambertian
    color.rgb *= AmbientColor + (lightAmount * LightColor * diffuseLighting * LightOpacity);
    ```

    La formula effettiva è:
    ```
    output_factor = AmbientColor + dot(N, -L)² × (D² / |r|²) × LightColor × opacity
    ```

    Ovvero: **coseno al quadrato** (squared cosine) moltiplicato per l'attenuazione.
    Non è Lambertian puro (`dot(N,-L)`) ma `dot(N,-L)²`.

    **Conseguenza visiva**: la luce cade più bruscamente al variare dell'angolo di incidenza
    rispetto a un diffuse standard. Superfici quasi-parallele alla direzione della luce
    ricevono molto meno luce di quanto farebbe un Lambertian classico.

    **Per la migrazione PixiJS**: lo shader WGSL/GLSL deve replicare esattamente
    `lightAmount * diffuseLighting` = `dot² × attenuation`, non solo `dot × attenuation`.
    Usare solo il termine Lambertian standard produce un'illuminazione troppo "piatta" e
    mancanza di contrasto — probabilmente una delle cause dei problemi visivi attuali.

12. **Culling luci tramite BoundingRectangle**: in `MarkInDeDarkGame.Update()` (righe 794-807),
    prima di aggiornare `PositionForGround`/`PositionForWall`, il gioco verifica se la luce
    è fuori viewport:
    ```csharp
    gameLight.IsOutOfScreen =
        _camera.Contains(gameLight.BoundingRectangle) == ContainmentType.Disjoint;
    ```
    `ShouldDraw = IsEnabled && !IsOutOfScreen` — le luci fuori viewport non eseguono
    nessuna draw call. Questo è un ottimizzazione importante da replicare in PixiJS per
    evitare pass shader inutili quando la sorgente è lontana dalla camera.
