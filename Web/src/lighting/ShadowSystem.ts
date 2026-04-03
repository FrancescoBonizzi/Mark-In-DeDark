import {
  Application,
  BlurFilter,
  Container,
  Graphics,
  RenderTexture,
  Sprite,
  Texture,
} from "pixi.js";
import type { IWall } from "../grid/IWall";
import type { Pillar } from "../grid/Pillar";
import type Camera from "../world/Camera";
import { Hull } from "./Hull";
import type { MarkLight } from "./MarkLight";
import { NormalMapFilter } from "./NormalMapFilter";

// Task 1 parity: keep PixiJS render passes aligned with C# while handling web asset gaps.
// C# had full ground normal maps; web spritesheets have partial ground normal coverage,
// so the ground-normal RT must clear to neutral normal (0x8080FF) instead of black.

const GAME_W = 1280;
const GAME_H = 720;
/** Size (px) of the pre-rendered radial gradient texture. */
const LIGHT_TEX_SIZE = 256;
/** Limit per-light shadow work to nearby hulls (reduces black crush and frame spikes). */
const MAX_SHADOW_HULLS_PER_LIGHT = 180;
const SHADOW_HULL_CULL_RADIUS_MULTIPLIER = 1.75;

interface RenderDebugConfig {
  enabled: boolean;
  logEveryNFrames: number;
  singleLightIndex: number | undefined;
  forceAmbient: { r: number; g: number; b: number } | undefined;
  disableMultiply: boolean;
  sampleLuma: boolean;
  /** Debug: mostra il pavimento grezzo (_groundRT) senza NM filter, per diagnostica colori tile. */
  showGroundRT: boolean;
  /** Debug: nasconde _lightmapSprite per vedere _normalMappedGroundRT direttamente senza il multiply. */
  hideLightmap: boolean;
  /** Debug: salta il composite della specular masked RT su _normalMappedGroundRT. */
  disableSpecular: boolean;
  /** Debug: mostra _specularLightmaskRT al posto di _normalMappedGroundRT. */
  showSpecLightmask: boolean;
  /** Debug: mostra _specularMaskedRT al posto di _normalMappedGroundRT. */
  showSpecMasked: boolean;
  /** Debug: mostra _lightmap RT grezzo (prima del multiply) al posto di _normalMappedGroundRT. */
  showLightmap: boolean;
}

const DEFAULT_RENDER_DEBUG: RenderDebugConfig = {
  enabled: false,
  logEveryNFrames: 120,
  singleLightIndex: undefined,
  forceAmbient: undefined,
  disableMultiply: false,
  sampleLuma: false,
  showGroundRT: false,
  hideLightmap: false,
  disableSpecular: false,
  showSpecLightmask: false,
  showSpecMasked: false,
  showLightmap: false,
};

/**
 * Shadow + Normal-Map system (Task 5).
 *
 * Stage layout after construction:
 *   [0] _normalMappedGroundSprite  — ground with normal map lighting applied
 *   [1] _lightmapSprite            — shadow lightmap (multiply, ground only)
 *   [2] _normalMappedObjectsSprite — walls/pillars with normal map lighting
 *   [3] camera._world              — empty (no children added by bootstrap)
 *
 * Per-frame pipeline:
 *   1. Ground base      → _groundRT
 *   2. Ground normals   → _groundNormalRT
 *   2a. Ground specular → _groundSpecularRT          (C#: specularMapRenderTarget)
 *   2b. Specular lightmask → _specularLightmaskRT    (C#: specularMapLightmaskRenderTarget, glow sprites)
 *   2c. Specular masked → _specularMaskedRT           (C#: specular × lightmask via specularmap.fx)
 *   3. Normal-map ground: per-light Lambertian (additive) + additive composite of specularMasked → _normalMappedGroundRT
 *   4. Objects base     → _objectsRT
 *   5. Objects normals  → _objectsNormalRT
 *   6. Normal-map objs: per-light Lambertian (additive)  → _normalMappedObjectsRT
 *   7. Shadow lightmap  → _lightmap (ambient fill + per-light circle - shadows + glow mask additively)
 */
export class ShadowSystem {
  lights: MarkLight[] = [];
  hulls: Hull[] = [];

  /** Wall→hull pairs for dynamic enabled-state sync (e.g. interactive doors). */
  private _wallHullPairs: Array<{ wall: IWall; hull: Hull }> = [];

  ambientColor = 0x000000;
  ambientAlpha = 1.0;
  normalMapAmbienceColor: { r: number; g: number; b: number } = {
    r: 0,
    g: 0,
    b: 0,
  };
  /** Scale of glow sprites for specular lightmask and shadow lightmap (C# GlowScale). Set from LevelCustomization. */
  glowScale = 2.0;

  private readonly _camera: Camera;

  private readonly _groundContainer: Container;
  private readonly _groundNormalContainer: Container;
  private readonly _groundSpecularContainer: Container;
  private readonly _objectsContainer: Container;
  private readonly _objectsNormalContainer: Container;

  /** Glow sprite texture used for specular lightmask and shadow lightmap (C# Effects/glow). Null if not available. */
  private readonly _glowTexture: Texture | null;

  // RenderTextures
  private readonly _groundRT: RenderTexture;
  private readonly _groundNormalRT: RenderTexture;
  private readonly _groundSpecularRT: RenderTexture;
  private readonly _specularLightmaskRT: RenderTexture;
  private readonly _specularMaskedRT: RenderTexture;
  private readonly _normalMappedGroundRT: RenderTexture;
  private readonly _objectsRT: RenderTexture;
  private readonly _objectsNormalRT: RenderTexture;
  private readonly _normalMappedObjectsRT: RenderTexture;
  private readonly _lightmap: RenderTexture;
  private readonly _perLightRT: RenderTexture;
  private readonly _blurredRT: RenderTexture;
  private readonly _lightTexture: Texture;

  // Display-list sprites
  private readonly _normalMappedGroundSprite: Sprite;
  private readonly _lightmapSprite: Sprite;
  private readonly _normalMappedObjectsSprite: Sprite;
  private _warnedNoLights = false;

  // --- Player death fade (Task 2) ---
  /** Container on top of stage (world-space coords, camera-synced) for the player death overlay sprite. */
  readonly playerDeathContainer: Container;
  private _deathFading = false;
  private _deathFadeElapsedMs = 0;
  private _deathFadeDurationMs = 1500;

  /**
   * Container for character shadow sprites (C# drawShadows pass).
   * Rendered onto _normalMappedGroundRT after the normal-map pass, in AlphaBlend,
   * with camera transform applied — same as C# Passo 1G second SpriteBatch.
   */
  readonly shadowContainer = new Container();

  /** Reused blur filter for the shadow lightmap pass. */
  private readonly _blurFilter = new BlurFilter({ strength: 3, quality: 2 });
  /** Reused normal-map filter (blendMode = 'add' always). */
  private readonly _normalMapFilter = new NormalMapFilter();

  // --- Pre-allocated objects to reduce per-frame garbage (Task Bonus: memory) ---
  /** Empty container used for clearColor renders instead of allocating Graphics each frame. */
  private readonly _emptyContainer!: Container;
  /** Reusable camera-transform container for borrowed-child renders. */
  private readonly _camContainer!: Container;
  /** Reusable sortable camera-transform container for objects pass. */
  private readonly _camSortableContainer!: Container;
  /** Sprite wrapping _groundRT; child of _groundNMFilterContainer. */
  private readonly _groundNMSprite!: Sprite;
  /** Container with NM filter; reused for per-light NM ground pass. */
  private readonly _groundNMFilterContainer!: Container;
  /** Sprite wrapping _objectsRT; child of _objectsNMFilterContainer. */
  private readonly _objectsNMSprite!: Sprite;
  /** Container with NM filter; reused for per-light NM objects pass. */
  private readonly _objectsNMFilterContainer!: Container;
  /** Sprite wrapping _perLightRT; child of _blurPassContainer. */
  private readonly _perLightReuseSprite!: Sprite;
  /** Container with blur filter; reused each frame for blur pass. */
  private readonly _blurPassContainer!: Container;
  /** Sprite wrapping _blurredRT with additive blend; reused for lightmap composite. */
  private readonly _blurredCompositeSprite!: Sprite;
  /** Sprite wrapping _specularMaskedRT with additive blend; reused for NM ground composite. */
  private readonly _specMaskedCompositeSprite!: Sprite;
  /** Sprite wrapping _specularLightmaskRT; reused for specular masked step. */
  private readonly _specLightmaskCopySprite!: Sprite;
  /** Sprite wrapping _groundSpecularRT with multiply blend; reused for specular masked step. */
  private readonly _specMultiplySprite!: Sprite;
  /** Wrapper container for _specMultiplySprite; required so blend mode applies against existing RT content. */
  private readonly _specMultiplyContainer!: Container;

  constructor(
    app: Application,
    camera: Camera,
    groundContainer: Container,
    groundNormalContainer: Container,
    groundSpecularContainer: Container,
    objectsContainer: Container,
    objectsNormalContainer: Container,
    glowTexture: Texture | null,
  ) {
    this._camera = camera;
    this._groundContainer = groundContainer;
    this._groundNormalContainer = groundNormalContainer;
    this._groundSpecularContainer = groundSpecularContainer;
    this._objectsContainer = objectsContainer;
    this._objectsNormalContainer = objectsNormalContainer;
    this._glowTexture = glowTexture;

    if (!this._glowTexture) {
      console.warn(
        "[ShadowSystem] glow texture is missing; specular/lightmap glow passes will be skipped.",
      );
    }

    const mkRT = () => RenderTexture.create({ width: GAME_W, height: GAME_H });

    this._groundRT = mkRT();
    this._groundNormalRT = mkRT();
    this._groundSpecularRT = mkRT();
    this._specularLightmaskRT = mkRT();
    this._specularMaskedRT = mkRT();
    this._normalMappedGroundRT = mkRT();
    this._objectsRT = mkRT();
    this._objectsNormalRT = mkRT();
    this._normalMappedObjectsRT = mkRT();
    this._lightmap = mkRT();
    this._perLightRT = mkRT();
    this._blurredRT = mkRT();
    this._lightTexture = this._createLightTexture(LIGHT_TEX_SIZE);

    // Pre-allocate reusable objects (avoids per-frame allocation — Task Bonus: memory)
    this._emptyContainer = new Container();
    this._camContainer = new Container();
    this._camSortableContainer = new Container();
    this._camSortableContainer.sortableChildren = true;

    this._groundNMSprite = new Sprite(this._groundRT);
    this._groundNMFilterContainer = new Container();
    this._groundNMFilterContainer.filters = [this._normalMapFilter];
    this._groundNMFilterContainer.addChild(this._groundNMSprite);

    this._objectsNMSprite = new Sprite(this._objectsRT);
    this._objectsNMFilterContainer = new Container();
    this._objectsNMFilterContainer.filters = [this._normalMapFilter];
    this._objectsNMFilterContainer.addChild(this._objectsNMSprite);

    this._perLightReuseSprite = new Sprite(this._perLightRT);
    this._blurPassContainer = new Container();
    this._blurPassContainer.filters = [this._blurFilter];
    this._blurPassContainer.addChild(this._perLightReuseSprite);

    this._blurredCompositeSprite = new Sprite(this._blurredRT);
    this._blurredCompositeSprite.blendMode = 'add';

    this._specMaskedCompositeSprite = new Sprite(this._specularMaskedRT);
    this._specMaskedCompositeSprite.blendMode = 'add';

    this._specLightmaskCopySprite = new Sprite(this._specularLightmaskRT);

    this._specMultiplySprite = new Sprite(this._groundSpecularRT);
    this._specMultiplySprite.blendMode = 'multiply';
    this._specMultiplyContainer = new Container();
    this._specMultiplyContainer.addChild(this._specMultiplySprite);

    // [0] Ground with normal map lighting
    this._normalMappedGroundSprite = new Sprite(this._normalMappedGroundRT);
    app.stage.addChildAt(this._normalMappedGroundSprite, 0);

    // [1] Shadow lightmap — multiply blend darkens the ground
    this._lightmapSprite = new Sprite(this._lightmap);
    this._lightmapSprite.blendMode = "multiply";
    app.stage.addChildAt(this._lightmapSprite, 1);

    // [2] Walls/pillars with normal map lighting (above lightmap — no shadow multiply)
    this._normalMappedObjectsSprite = new Sprite(this._normalMappedObjectsRT);
    app.stage.addChildAt(this._normalMappedObjectsSprite, 2);

    // [4] Player death overlay — world-space container above everything (camera-synced each frame)
    this.playerDeathContainer = new Container();
    app.stage.addChild(this.playerDeathContainer);
  }

  // -------------------------------------------------------------------------
  // Hull / light management
  // -------------------------------------------------------------------------

  addLight(light: MarkLight): void {
    this.lights.push(light);
  }

  removeLight(light: MarkLight): void {
    const i = this.lights.indexOf(light);
    if (i !== -1) this.lights.splice(i, 1);
  }

  addHull(hull: Hull): void {
    this.hulls.push(hull);
  }

  removeHull(hull: Hull): void {
    const i = this.hulls.indexOf(hull);
    if (i !== -1) this.hulls.splice(i, 1);
  }

  addHullsFromWalls(walls: IWall[]): void {
    for (const wall of walls) {
      for (const rect of wall.hullRects) {
        const hull = Hull.fromRectangle(rect);
        this.hulls.push(hull);
        this._wallHullPairs.push({ wall, hull });
      }
    }
  }

  addHullsFromPillars(pillars: Pillar[]): void {
    for (const pillar of pillars) {
      for (const rect of pillar.hullRects) {
        this.hulls.push(Hull.fromRectangle(rect));
      }
    }
  }

  /**
   * Starts fading out the world rendering (ground + lightmap + objects sprites) over `durationMs`.
   * `playerDeathContainer` (above the fading layer) is NOT faded — it stays fully visible.
   * Call once when the player death sequence begins.
   */
  startDeathFade(durationMs: number = 1500): void {
    this._deathFadeDurationMs = durationMs;
    this._deathFadeElapsedMs = 0;
    this._deathFading = true;
  }

  /** Resets death fade state (called on level restart). */
  resetDeathFade(): void {
    this._deathFading = false;
    this._deathFadeElapsedMs = 0;
    this._normalMappedGroundSprite.alpha = 1;
    this._lightmapSprite.alpha = 1;
    this._normalMappedObjectsSprite.alpha = 1;
    this.playerDeathContainer.removeChildren();
  }

  // -------------------------------------------------------------------------
  // Per-frame update
  // -------------------------------------------------------------------------

  update(app: Application, deltaMS: number): void {
    const renderer = app.renderer;
    const cw = this._camera.worldContainer;
    const debug = this._readRenderDebugConfig();

    this._lightmapSprite.blendMode = debug.disableMultiply
      ? "normal"
      : "multiply";
    this._lightmapSprite.visible = !debug.hideLightmap;

    if (this.lights.length === 0 && !this._warnedNoLights) {
      this._warnedNoLights = true;
      console.warn(
        "[ShadowSystem] no lights registered; normal-map passes will render black.",
      );
    }

    // Sync hull enabled state from walls (handles interactive doors opening/closing)
    for (const { wall, hull } of this._wallHullPairs) {
      hull.enabled = wall.hullRects.length > 0;
    }

    // Animate lights
    for (const light of this.lights) light.update(deltaMS);

    // Sync playerDeathContainer with camera transform every frame (children use world coords)
    this.playerDeathContainer.position.copyFrom(cw.position);
    this.playerDeathContainer.scale.copyFrom(cw.scale);
    this.playerDeathContainer.pivot.copyFrom(cw.pivot);

    // Death fade: fade out world sprites, playerDeathContainer stays at alpha=1
    if (this._deathFading) {
      this._deathFadeElapsedMs += deltaMS;
      const t = Math.min(1, this._deathFadeElapsedMs / this._deathFadeDurationMs);
      const alpha = 1 - t;
      this._normalMappedGroundSprite.alpha = alpha;
      this._lightmapSprite.alpha = alpha;
      this._normalMappedObjectsSprite.alpha = alpha;
    }

    // Reusable helper: borrow a child into the pre-created camera container, render, then return it.
    // Avoids allocating a new Container every call (Task Bonus: memory).
    const renderWithCam = (child: Container, target: RenderTexture, clear: boolean) => {
      this._camContainer.position.copyFrom(cw.position);
      this._camContainer.scale.copyFrom(cw.scale);
      this._camContainer.pivot.copyFrom(cw.pivot);
      this._camContainer.addChild(child);
      renderer.render({ container: this._camContainer, target, clear });
      this._camContainer.removeChild(child);
    };

    // Same but with sortableChildren=true (for objects passes).
    const renderWithCamSortable = (child: Container, target: RenderTexture, clear: boolean) => {
      this._camSortableContainer.position.copyFrom(cw.position);
      this._camSortableContainer.scale.copyFrom(cw.scale);
      this._camSortableContainer.pivot.copyFrom(cw.pivot);
      this._camSortableContainer.addChild(child);
      renderer.render({ container: this._camSortableContainer, target, clear });
      this._camSortableContainer.removeChild(child);
    };

    // ------------------------------------------------------------------
    // 1. Ground base → _groundRT (opaque black background)
    // ------------------------------------------------------------------
    renderer.render({ container: this._emptyContainer, target: this._groundRT, clear: true, clearColor: [0, 0, 0, 1] });
    renderWithCam(this._groundContainer, this._groundRT, false);

    // ------------------------------------------------------------------
    // DEBUG: showGroundRT — mostra il pavimento grezzo senza NM filter
    // ------------------------------------------------------------------
    if (debug.showGroundRT) {
      const groundCopySprite = new Sprite(this._groundRT);
      renderer.render({ container: groundCopySprite, target: this._normalMappedGroundRT, clear: true });
      groundCopySprite.destroy();
      return;
    }

    // ------------------------------------------------------------------
    // 2. Ground normals → _groundNormalRT (neutral normal 0x8080FF fill)
    // C# could clear black because every ground tile had a normal map.
    // Web has missing ground normal textures on several packs, so use neutral normal fill.
    // ------------------------------------------------------------------
    renderer.render({ container: this._emptyContainer, target: this._groundNormalRT, clear: true, clearColor: [0x80 / 255, 0x80 / 255, 1, 1] });
    renderWithCam(this._groundNormalContainer, this._groundNormalRT, false);

    // ------------------------------------------------------------------
    // 2a. Ground specular → _groundSpecularRT (C#: specularMapRenderTarget)
    // ------------------------------------------------------------------
    renderer.render({ container: this._emptyContainer, target: this._groundSpecularRT, clear: true, clearColor: [0, 0, 0, 1] });
    renderWithCam(this._groundSpecularContainer, this._groundSpecularRT, false);

    // ------------------------------------------------------------------
    // 2b. Specular lightmask → _specularLightmaskRT
    //     C#: glow sprites from all lights with NonPremultiplied blend (specularMapLightmaskRenderTarget)
    // ------------------------------------------------------------------
    renderer.render({ container: this._emptyContainer, target: this._specularLightmaskRT, clear: true, clearColor: [0, 0, 0, 1] });
    if (this._glowTexture && this.lights.length > 0) {
      const glowContainer = new Container();
      glowContainer.position.copyFrom(cw.position);
      glowContainer.scale.copyFrom(cw.scale);
      glowContainer.pivot.copyFrom(cw.pivot);

      if (debug.enabled) {
        console.log(`[spec-lightmask] glowScale=${this.glowScale} glowTexSize=${this._glowTexture.source.width}x${this._glowTexture.source.height}`);
      }

      for (const light of this.lights) {
        if (!light.enabled || !light.isGlowEnabled) continue;
        const glow = new Sprite(this._glowTexture);
        glow.anchor.set(0.5);
        // C#: _specularGlowDrawingInfos.Position = Position + (0, 100), Scale = GlowScale
        glow.x = light.position.x + light.deltaForGround.x;
        glow.y = light.position.y + light.deltaForGround.y;
        glow.scale.set(this.glowScale);
        if (debug.enabled) {
          const { sx, sy } = this._worldToScreen(glow.x, glow.y, cw);
          const screenR = (this._glowTexture.source.width / 2) * this.glowScale * cw.scale.x;
          console.log(`[spec-lightmask] glow world=(${glow.x.toFixed(0)},${glow.y.toFixed(0)}) screen=(${sx.toFixed(1)},${sy.toFixed(1)}) screenRadius=${screenR.toFixed(1)}`);
        }
        glowContainer.addChild(glow);
      }

      renderer.render({ container: glowContainer, target: this._specularLightmaskRT, clear: false });
      glowContainer.destroy({ children: true });
    }

    // ------------------------------------------------------------------
    // 2c. Specular masked → _specularMaskedRT
    //     C#: specularmap.fx → return specular * lightmask (pixel multiply)
    // ------------------------------------------------------------------
    // Start with the lightmask as base (pre-created sprite)
    renderer.render({ container: this._specLightmaskCopySprite, target: this._specularMaskedRT, clear: true });
    // Multiply specular onto the lightmask → result = specular * lightmask
    // Use container wrapper: blend mode on a root sprite does not apply against existing RT content in PixiJS 8 WebGL.
    renderer.render({ container: this._specMultiplyContainer, target: this._specularMaskedRT, clear: false });

    // ------------------------------------------------------------------
    // DEBUG: showSpecLightmask / showSpecMasked
    // ------------------------------------------------------------------
    if (debug.showSpecLightmask) {
      const s = new Sprite(this._specularLightmaskRT);
      renderer.render({ container: s, target: this._normalMappedGroundRT, clear: true });
      s.destroy();
      return;
    }
    if (debug.showSpecMasked) {
      const s = new Sprite(this._specularMaskedRT);
      renderer.render({ container: s, target: this._normalMappedGroundRT, clear: true });
      s.destroy();
      return;
    }

    // ------------------------------------------------------------------
    // 3. Normal-map ground pass → _normalMappedGroundRT
    //    C#: AmbientColor è dentro lo shader, accumulato N volte via additive blend
    //    Per-light: additive Lambertian + ambient per-pass
    //    Al termine: composizione additiva di _specularMaskedRT (C#: additive blend specular + NM)
    // ------------------------------------------------------------------
    // Clear RT to black — l'ambient viene ora accumulato dallo shader per ogni luce
    renderer.render({ container: this._emptyContainer, target: this._normalMappedGroundRT, clear: true, clearColor: [0, 0, 0, 1] });

    // C#: composizione specular+lightmask (BlendState.Additive) eseguita PRIMA del loop NM.
    // Additivo è commutativo (A+B = B+A) e renderlo qui (su RT pulita da black fill) evita il
    // problema PixiJS 8 per cui i render diretti su una RT già usata come filter-target si comportano
    // come overwrite invece di accumulo.
    // Skip specular composite when no specular geometry exists (groundSpecularContainer empty).
    // multiply blend on a black RT via root-Sprite does not zero the lightmask in PixiJS 8 WebGL;
    // compositing the raw lightmask additively produces incorrect glow artifacts on the floor.
    if (!debug.disableSpecular) {
      renderer.render({ container: this._specMaskedCompositeSprite, target: this._normalMappedGroundRT, clear: false });
    }

    this._normalMapFilter.setNormalMap(this._groundNormalRT);
    this._normalMapFilter.setAmbientColor(debug.forceAmbient ?? this.normalMapAmbienceColor);
    // Single-pass additive accumulation: filter writes directly onto _normalMappedGroundRT
    // with blendMode="add" instead of the two-pass (filter→_perLightRT, sprite→finalRT) approach.
    // This matches how PixiJS 8 FilterSystem applies the filter blend state: filter._state.blendMode
    // controls the blend when the filter quad is drawn to the outputRenderSurface.
    this._normalMapFilter.blendMode = "add";

    if (debug.enabled) {
      console.log(`[NM-ground] cw.pos=(${cw.position.x.toFixed(1)},${cw.position.y.toFixed(1)}) scale=(${cw.scale.x.toFixed(3)}) pivot=(${cw.pivot.x.toFixed(1)},${cw.pivot.y.toFixed(1)})`);
    }

    for (let i = 0; i < this.lights.length; i++) {
      const light = this.lights[i]!;
      if (!light.enabled) continue;
      if (debug.singleLightIndex !== undefined && i !== debug.singleLightIndex) continue;

      const { sx, sy } = this._worldToScreen(
        light.position.x + light.deltaForGround.x,
        light.position.y + light.deltaForGround.y,
        cw,
      );
      if (debug.enabled) {
        const c = light.colorNormalVec3;
        console.log(`[NM-ground] light[${i}] world=(${(light.position.x+light.deltaForGround.x).toFixed(0)},${(light.position.y+light.deltaForGround.y).toFixed(0)}) screen=(${sx.toFixed(1)},${sy.toFixed(1)}) z=${light.z} distSq=${light.distanceSquaredForGround} opacity=${light.opacity} rgb=(${c.r.toFixed(2)},${c.g.toFixed(2)},${c.b.toFixed(2)})`);
      }
      this._normalMapFilter.setLight(sx, sy, light.z, light.colorNormalVec3, light.distanceSquaredForGround, light.opacity);
      // Use pre-created filter container (no allocation)
      renderer.render({ container: this._groundNMFilterContainer, target: this._normalMappedGroundRT, clear: false });
    }

    this._normalMapFilter.blendMode = "normal";

    // ------------------------------------------------------------------
    // 3b. Shadow sprites → _normalMappedGroundRT (C#: Passo 1G second SpriteBatch)
    //     Character shadow textures drawn on top of lit ground, AlphaBlend, camera transform.
    // ------------------------------------------------------------------
    if (this.shadowContainer.children.length > 0) {
      renderWithCam(this.shadowContainer, this._normalMappedGroundRT, false);
    }

    // ------------------------------------------------------------------
    // 4. Objects base → _objectsRT
    // ------------------------------------------------------------------
    renderWithCamSortable(this._objectsContainer, this._objectsRT, true);

    // ------------------------------------------------------------------
    // 5. Objects normals → _objectsNormalRT (neutral normal = 0x8080FF)
    // ------------------------------------------------------------------
    renderer.render({ container: this._emptyContainer, target: this._objectsNormalRT, clear: true, clearColor: [0x80 / 255, 0x80 / 255, 1, 1] });
    renderWithCamSortable(this._objectsNormalContainer, this._objectsNormalRT, false);

    // ------------------------------------------------------------------
    // 6. Normal-map objects pass → _normalMappedObjectsRT
    //    C#: clear to Color.Transparent (NOT opaque black) so that non-object pixels
    //    remain transparent and don't cover the ground layer below.
    //    Opaque black fill was the root cause of the black-floor bug (Task 1).
    // ------------------------------------------------------------------
    // Clear to transparent — non-object pixels stay alpha=0, showing ground through.
    renderer.render({ container: this._emptyContainer, target: this._normalMappedObjectsRT, clear: true });

    this._normalMapFilter.setNormalMap(this._objectsNormalRT);
    this._normalMapFilter.setAmbientColor(debug.forceAmbient ?? this.normalMapAmbienceColor);
    this._normalMapFilter.blendMode = "add";

    for (let i = 0; i < this.lights.length; i++) {
      const light = this.lights[i]!;
      if (!light.enabled) continue;
      if (debug.singleLightIndex !== undefined && i !== debug.singleLightIndex) continue;

      const { sx, sy } = this._worldToScreen(
        light.position.x + light.deltaForWall.x,
        light.position.y + light.deltaForWall.y,
        cw,
      );
      this._normalMapFilter.setLight(sx, sy, light.z, light.colorNormalVec3, light.distanceSquaredForWall, light.opacity);
      // Use pre-created filter container (no allocation)
      renderer.render({ container: this._objectsNMFilterContainer, target: this._normalMappedObjectsRT, clear: false });
    }

    this._normalMapFilter.blendMode = "normal";

    // ------------------------------------------------------------------
    // 7. Shadow lightmap
    // ------------------------------------------------------------------
    {
      const r = ((this.ambientColor >> 16) & 0xFF) / 255;
      const g = ((this.ambientColor >> 8) & 0xFF) / 255;
      const b = (this.ambientColor & 0xFF) / 255;
      renderer.render({ container: this._emptyContainer, target: this._lightmap, clear: true, clearColor: [r, g, b, this.ambientAlpha] });
    }

    for (let i = 0; i < this.lights.length; i++) {
      const light = this.lights[i]!;
      if (!light.enabled) continue;
      // C#: solo le luci con castsShadows=true (player) entrano nel lightmap Penumbra.
      // Brazier/candles non hanno Penumbra.Enabled e appaiono solo via NM-pass e glow sprites (Phase 7b).
      if (!light.castsShadows) continue;
      if (debug.singleLightIndex !== undefined && i !== debug.singleLightIndex) continue;

      const perLightContainer = new Container();
      perLightContainer.position.copyFrom(cw.position);
      perLightContainer.scale.copyFrom(cw.scale);
      perLightContainer.pivot.copyFrom(cw.pivot);

      const penumbraPos = light.getPenumbraPosition();

      const lightSprite = new Sprite(this._lightTexture);
      lightSprite.scale.set(light.radius / LIGHT_TEX_SIZE);
      lightSprite.anchor.set(0.5);
      // C#: PenumbraLight uses Position + (-10, +75), not the raw torch point.
      lightSprite.x = penumbraPos.x;
      lightSprite.y = penumbraPos.y;
      lightSprite.tint = light.currentColor;
      lightSprite.alpha = light.opacity;
      perLightContainer.addChild(lightSprite);

      if (light.castsShadows && this.hulls.length > 0) {
        const hullsForLight = this._collectRelevantHulls(
          penumbraPos.x,
          penumbraPos.y,
          light.radius * SHADOW_HULL_CULL_RADIUS_MULTIPLIER,
          MAX_SHADOW_HULLS_PER_LIGHT,
        );

        const shadowGfx = new Graphics();
        for (const hull of hullsForLight) {
          if (!hull.enabled) continue;
          const polys = hull.computeShadowPolygons(penumbraPos.x, penumbraPos.y);
          for (const poly of polys) shadowGfx.poly(poly).fill(0x000000);
        }
        perLightContainer.addChild(shadowGfx);

        const hullFillGfx = new Graphics();
        for (const hull of hullsForLight) {
          if (!hull.enabled) continue;
          const b = hull.bounds;
          hullFillGfx.rect(b.x, b.y, b.width, b.height).fill(0x000000);
        }
        perLightContainer.addChild(hullFillGfx);
      }

      renderer.render({ container: perLightContainer, target: this._perLightRT, clear: true });
      perLightContainer.destroy({ children: true });

      // Blur pass — use pre-created container (no allocation)
      this._blurFilter.strength = light.sourceRadius * 0.15;
      renderer.render({ container: this._blurPassContainer, target: this._blurredRT, clear: true });

      // Additive composite → lightmap (pre-created sprite)
      renderer.render({ container: this._blurredCompositeSprite, target: this._lightmap, clear: false });
    }

    // ------------------------------------------------------------------
    // 7b. Additional glow mask on lightmap (RC1)
    //     C#: enemy/brazier/candle DrawLightMask() drawn additively onto Penumbra.LightMap
    //     Effects/glow at Position+(0,100), scale lightMaskScale, additive blend
    // ------------------------------------------------------------------
    if (this._glowTexture && this.lights.length > 0) {
      const glowContainer = new Container();
      glowContainer.position.copyFrom(cw.position);
      glowContainer.scale.copyFrom(cw.scale);
      glowContainer.pivot.copyFrom(cw.pivot);

      for (let i = 0; i < this.lights.length; i++) {
        const light = this.lights[i]!;
        if (
          !light.enabled ||
          !light.isGlowEnabled ||
          !light.drawInAdditionalLightMask
        )
          continue;
        if (
          debug.singleLightIndex !== undefined &&
          i !== debug.singleLightIndex
        )
          continue;
        const glow = new Sprite(this._glowTexture);
        glow.anchor.set(0.5);
        glow.x = light.position.x + light.deltaForGround.x;
        glow.y = light.position.y + light.deltaForGround.y;
        glow.scale.set(light.lightMaskScale);
        glow.blendMode = "add";
        glowContainer.addChild(glow);
      }

      renderer.render({
        container: glowContainer,
        target: this._lightmap,
        clear: false,
      });
      glowContainer.destroy({ children: true });
    }

    // ------------------------------------------------------------------
    // DEBUG: readback diretto del _lightmap RT ai punti di test
    // ------------------------------------------------------------------
    if (debug.enabled) {
      type ExtractAPI = { extract?: { pixels?: (opts: {target: RenderTexture; frame?: {x:number;y:number;width:number;height:number}}) => Promise<{pixels:Uint8ClampedArray}> | {pixels:Uint8ClampedArray} } };
      const ext = (renderer as unknown as ExtractAPI).extract;
      if (ext?.pixels) {
        for (const [name, px, py] of [["below_640_460",640,460],["player_640_360",640,360],["left_530_360",530,360]] as [string,number,number][]) {
          Promise.resolve(ext.pixels({ target: this._lightmap, frame: {x:px,y:py,width:1,height:1} })).then(r => {
            console.log(`[lightmap-gpu] ${name} (${px},${py}): rgba=(${r.pixels[0]},${r.pixels[1]},${r.pixels[2]},${r.pixels[3]})`);
          }).catch(() => {});
        }
      }
    }

    // ------------------------------------------------------------------
    // DEBUG: showLightmap — mostra il lightmap grezzo senza multiply
    // ------------------------------------------------------------------
    if (debug.showLightmap) {
      const s = new Sprite(this._lightmap);
      renderer.render({ container: s, target: this._normalMappedGroundRT, clear: true });
      s.destroy();
      return;
    }

  }

  destroy(): void {
    this._wallHullPairs = [];

    // Destroy pre-allocated reusable objects (detach children before destroying containers
    // to avoid double-destroy of the sprites they contain)
    this._emptyContainer.destroy();
    this._camContainer.destroy();
    this._camSortableContainer.destroy();

    this._groundNMFilterContainer.filters = [];
    this._groundNMFilterContainer.removeChildren();
    this._groundNMFilterContainer.destroy();
    this._groundNMSprite.destroy();

    this._objectsNMFilterContainer.filters = [];
    this._objectsNMFilterContainer.removeChildren();
    this._objectsNMFilterContainer.destroy();
    this._objectsNMSprite.destroy();

    this._blurPassContainer.filters = [];
    this._blurPassContainer.removeChildren();
    this._blurPassContainer.destroy();
    this._perLightReuseSprite.destroy();

    this._blurredCompositeSprite.destroy();
    this._specMaskedCompositeSprite.destroy();
    this._specLightmaskCopySprite.destroy();
    this._specMultiplySprite.destroy();

    this._groundRT.destroy();
    this._groundNormalRT.destroy();
    this._groundSpecularRT.destroy();
    this._specularLightmaskRT.destroy();
    this._specularMaskedRT.destroy();
    this._normalMappedGroundRT.destroy();
    this._objectsRT.destroy();
    this._objectsNormalRT.destroy();
    this._normalMappedObjectsRT.destroy();
    this._lightmap.destroy();
    this._perLightRT.destroy();
    this._blurredRT.destroy();
    this._lightTexture.destroy();
    this._normalMappedGroundSprite.destroy();
    this._lightmapSprite.destroy();
    this._normalMappedObjectsSprite.destroy();
    this._blurFilter.destroy();
    this._normalMapFilter.destroy();
    this.playerDeathContainer.destroy({ children: true });
  }

  // -------------------------------------------------------------------------
  // Internal helpers
  // -------------------------------------------------------------------------

  /** Convert a world-space point to screen-space pixel coordinates. */
  private _worldToScreen(
    wx: number,
    wy: number,
    cw: Container,
  ): { sx: number; sy: number } {
    return {
      sx: (wx - cw.pivot.x) * cw.scale.x + cw.position.x,
      sy: (wy - cw.pivot.y) * cw.scale.y + cw.position.y,
    };
  }

  private _readRenderDebugConfig(): RenderDebugConfig {
    const source = (
      globalThis as unknown as {
        __MIDD_RENDER_DEBUG__: Partial<RenderDebugConfig> | undefined;
      }
    ).__MIDD_RENDER_DEBUG__;

    if (!source) return DEFAULT_RENDER_DEBUG;

    const logEveryNFrames =
      typeof source.logEveryNFrames === "number" && source.logEveryNFrames > 0
        ? Math.floor(source.logEveryNFrames)
        : DEFAULT_RENDER_DEBUG.logEveryNFrames;
    const singleLightIndex =
      typeof source.singleLightIndex === "number"
        ? Math.floor(source.singleLightIndex)
        : undefined;
    const forceAmbient =
      source.forceAmbient &&
      typeof source.forceAmbient.r === "number" &&
      typeof source.forceAmbient.g === "number" &&
      typeof source.forceAmbient.b === "number"
        ? source.forceAmbient
        : undefined;

    return {
      enabled: source.enabled ?? DEFAULT_RENDER_DEBUG.enabled,
      logEveryNFrames,
      singleLightIndex,
      forceAmbient,
      disableMultiply:
        source.disableMultiply ?? DEFAULT_RENDER_DEBUG.disableMultiply,
      sampleLuma: source.sampleLuma ?? DEFAULT_RENDER_DEBUG.sampleLuma,
      showGroundRT: source.showGroundRT ?? DEFAULT_RENDER_DEBUG.showGroundRT,
      hideLightmap: source.hideLightmap ?? DEFAULT_RENDER_DEBUG.hideLightmap,
      disableSpecular: source.disableSpecular ?? DEFAULT_RENDER_DEBUG.disableSpecular,
      showSpecLightmask: source.showSpecLightmask ?? DEFAULT_RENDER_DEBUG.showSpecLightmask,
      showSpecMasked: source.showSpecMasked ?? DEFAULT_RENDER_DEBUG.showSpecMasked,
      showLightmap: source.showLightmap ?? DEFAULT_RENDER_DEBUG.showLightmap,
    };
  }

  private _collectRelevantHulls(
    x: number,
    y: number,
    radius: number,
    maxCount: number,
  ): Hull[] {
    const radiusSq = radius * radius;
    const scored: Array<{ hull: Hull; distSq: number }> = [];

    for (const hull of this.hulls) {
      if (!hull.enabled) continue;
      const d2 = this._pointToRectDistanceSq(x, y, hull.bounds);
      if (d2 <= radiusSq) scored.push({ hull, distSq: d2 });
    }

    scored.sort((a, b) => a.distSq - b.distSq);
    return scored.slice(0, maxCount).map((item) => item.hull);
  }

  private _pointToRectDistanceSq(
    x: number,
    y: number,
    rect: { x: number; y: number; width: number; height: number },
  ): number {
    const rx2 = rect.x + rect.width;
    const ry2 = rect.y + rect.height;
    const dx = x < rect.x ? rect.x - x : x > rx2 ? x - rx2 : 0;
    const dy = y < rect.y ? rect.y - y : y > ry2 ? y - ry2 : 0;
    return dx * dx + dy * dy;
  }

  /**
   * Pre-renders a white flat-top radial gradient circle into a Texture.
   * Mimics C# Penumbra behaviour: white (1.0) from center to ~70% of radius,
   * then smooth fade to transparent at the edge.
   */
  private _createLightTexture(size: number): Texture {
    const canvas = document.createElement("canvas");
    canvas.width = size;
    canvas.height = size;
    const ctx = canvas.getContext("2d")!;
    const cx = size / 2;
    const cy = size / 2;
    const r = size / 2;
    // Inner circle radius where the gradient starts fading (flat-top region)
    const flatEdge = r * 0.70;
    const gradient = ctx.createRadialGradient(cx, cy, flatEdge, cx, cy, r);
    gradient.addColorStop(0, "rgba(255,255,255,1.0)");
    gradient.addColorStop(1, "rgba(255,255,255,0.0)");
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.arc(cx, cy, r, 0, Math.PI * 2);
    ctx.fill();
    return Texture.from(canvas);
  }
}
