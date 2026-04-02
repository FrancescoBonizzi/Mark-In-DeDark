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
}

const DEFAULT_RENDER_DEBUG: RenderDebugConfig = {
  enabled: false,
  logEveryNFrames: 120,
  singleLightIndex: undefined,
  forceAmbient: undefined,
  disableMultiply: false,
  sampleLuma: false,
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

  ambientColor = 0x000000;
  ambientAlpha = 1.0;
  normalMapAmbienceColor: { r: number; g: number; b: number } = {
    r: 0,
    g: 0,
    b: 0,
  };
  /** Scale of glow sprites for specular lightmask and shadow lightmap (C# GlowScale). Set from LevelCustomization. */
  glowScale = 2.0;

  private readonly _app: Application;
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
  private readonly _lightTexture: RenderTexture;

  // Display-list sprites
  private readonly _normalMappedGroundSprite: Sprite;
  private readonly _lightmapSprite: Sprite;
  private readonly _normalMappedObjectsSprite: Sprite;
  private _frameIndex = 0;
  private _warnedNoLights = false;

  /** Reused blur filter for the shadow lightmap pass. */
  private readonly _blurFilter = new BlurFilter({ strength: 3, quality: 2 });
  /** Reused normal-map filter (blendMode = 'add' always). */
  private readonly _normalMapFilter = new NormalMapFilter();
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
    this._app = app;
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
        this.hulls.push(Hull.fromRectangle(rect));
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

    if (this.lights.length === 0 && !this._warnedNoLights) {
      this._warnedNoLights = true;
      console.warn(
        "[ShadowSystem] no lights registered; normal-map passes will render black.",
      );
    }

    // Animate lights
    for (const light of this.lights) light.update(deltaMS);

    // Helper: create a temp container that mirrors the camera world transform.
    // The given child is borrowed (removed after render, not destroyed).
    const renderWithCameraTransform = (
      child: Container,
      target: RenderTexture,
      clear: boolean,
      extraSetup?: (c: Container) => void,
    ) => {
      const tmp = new Container();
      tmp.position.copyFrom(cw.position);
      tmp.scale.copyFrom(cw.scale);
      tmp.pivot.copyFrom(cw.pivot);
      if (extraSetup) extraSetup(tmp);
      tmp.addChild(child);
      renderer.render({ container: tmp, target, clear });
      tmp.removeChild(child);
      tmp.destroy();
    };

    // ------------------------------------------------------------------
    // 1. Ground base → _groundRT (opaque black background)
    // ------------------------------------------------------------------
    {
      const blackFill = new Graphics();
      blackFill.rect(0, 0, GAME_W, GAME_H).fill(0x000000);
      renderer.render({
        container: blackFill,
        target: this._groundRT,
        clear: true,
      });
      blackFill.destroy();

      renderWithCameraTransform(this._groundContainer, this._groundRT, false);
    }

    // ------------------------------------------------------------------
    // 2. Ground normals → _groundNormalRT
    // C# could clear black because every ground tile had a normal map.
    // Web has missing ground normal textures on several packs, so use neutral normal fill.
    // ------------------------------------------------------------------
    {
      const normalFill = new Graphics();
      normalFill.rect(0, 0, GAME_W, GAME_H).fill(0x8080ff);
      renderer.render({
        container: normalFill,
        target: this._groundNormalRT,
        clear: true,
      });
      normalFill.destroy();

      renderWithCameraTransform(
        this._groundNormalContainer,
        this._groundNormalRT,
        false,
      );
    }

    // ------------------------------------------------------------------
    // 2a. Ground specular → _groundSpecularRT (C#: specularMapRenderTarget)
    // ------------------------------------------------------------------
    {
      const blackFill = new Graphics();
      blackFill.rect(0, 0, GAME_W, GAME_H).fill(0x000000);
      renderer.render({
        container: blackFill,
        target: this._groundSpecularRT,
        clear: true,
      });
      blackFill.destroy();

      renderWithCameraTransform(
        this._groundSpecularContainer,
        this._groundSpecularRT,
        false,
      );
    }

    // ------------------------------------------------------------------
    // 2b. Specular lightmask → _specularLightmaskRT
    //     C#: glow sprites from all lights with NonPremultiplied blend (specularMapLightmaskRenderTarget)
    // ------------------------------------------------------------------
    {
      const blackFill = new Graphics();
      blackFill.rect(0, 0, GAME_W, GAME_H).fill(0x000000);
      renderer.render({
        container: blackFill,
        target: this._specularLightmaskRT,
        clear: true,
      });
      blackFill.destroy();

      if (this._glowTexture && this.lights.length > 0) {
        const glowContainer = new Container();
        glowContainer.position.copyFrom(cw.position);
        glowContainer.scale.copyFrom(cw.scale);
        glowContainer.pivot.copyFrom(cw.pivot);

        for (const light of this.lights) {
          if (!light.enabled || !light.isGlowEnabled) continue;
          const glow = new Sprite(this._glowTexture);
          glow.anchor.set(0.5);
          // C#: _specularGlowDrawingInfos.Position = Position + (0, 100), Scale = GlowScale
          glow.x = light.position.x + light.deltaForGround.x;
          glow.y = light.position.y + light.deltaForGround.y;
          glow.scale.set(this.glowScale);
          glowContainer.addChild(glow);
        }

        renderer.render({
          container: glowContainer,
          target: this._specularLightmaskRT,
          clear: false,
        });
        glowContainer.destroy({ children: true });
      }
    }

    // ------------------------------------------------------------------
    // 2c. Specular masked → _specularMaskedRT
    //     C#: specularmap.fx → return specular * lightmask (pixel multiply)
    // ------------------------------------------------------------------
    {
      // Start with the lightmask as base
      const lightmaskSprite = new Sprite(this._specularLightmaskRT);
      renderer.render({
        container: lightmaskSprite,
        target: this._specularMaskedRT,
        clear: true,
      });
      lightmaskSprite.destroy();

      // Multiply specular onto the lightmask → result = specular * lightmask
      const specularSprite = new Sprite(this._groundSpecularRT);
      specularSprite.blendMode = "multiply";
      renderer.render({
        container: specularSprite,
        target: this._specularMaskedRT,
        clear: false,
      });
      specularSprite.destroy();
    }

    // ------------------------------------------------------------------
    // 3. Normal-map ground pass → _normalMappedGroundRT
    //    C#: AmbientColor è dentro lo shader, accumulato N volte via additive blend
    //    Per-light: additive Lambertian + ambient per-pass
    //    Al termine: composizione additiva di _specularMaskedRT (C#: additive blend specular + NM)
    // ------------------------------------------------------------------
    {
      // Clear RT to black — l'ambient viene ora accumulato dallo shader per ogni luce
      {
        const clearFill = new Graphics();
        clearFill.rect(0, 0, GAME_W, GAME_H).fill(0x000000);
        renderer.render({
          container: clearFill,
          target: this._normalMappedGroundRT,
          clear: true,
        });
        clearFill.destroy();
      }

      this._normalMapFilter.setNormalMap(this._groundNormalRT);
      this._normalMapFilter.setAmbientColor(
        debug.forceAmbient ?? this.normalMapAmbienceColor,
      );
      // Single-pass additive accumulation: filter writes directly onto _normalMappedGroundRT
      // with blendMode="add" instead of the two-pass (filter→_perLightRT, sprite→finalRT) approach.
      // This matches how PixiJS 8 FilterSystem applies the filter blend state: filter._state.blendMode
      // controls the blend when the filter quad is drawn to the outputRenderSurface.
      this._normalMapFilter.blendMode = "add";

      for (let i = 0; i < this.lights.length; i++) {
        const light = this.lights[i]!;
        if (!light.enabled) continue;
        if (
          debug.singleLightIndex !== undefined &&
          i !== debug.singleLightIndex
        )
          continue;

        const { sx, sy } = this._worldToScreen(
          light.position.x + light.deltaForGround.x,
          light.position.y + light.deltaForGround.y,
          cw,
        );
        this._normalMapFilter.setLight(
          sx,
          sy,
          // C#: Z non viene scalato per lo zoom (solo XY sono in screen-space)
          light.z,
          light.colorNormalVec3,
          light.distanceSquaredForGround,
          light.opacity,
        );

        const filterContainer = new Container();
        filterContainer.filters = [this._normalMapFilter];
        const diffuseSprite = new Sprite(this._groundRT);
        filterContainer.addChild(diffuseSprite);
        renderer.render({
          container: filterContainer,
          target: this._normalMappedGroundRT,
          clear: false,
        });
        filterContainer.destroy({ children: true });
      }

      this._normalMapFilter.blendMode = "normal";

      // C#: composizione finale = appliedNormalMap + specularWithLightmask (BlendState.Additive)
      const specMaskedSprite = new Sprite(this._specularMaskedRT);
      specMaskedSprite.blendMode = "add";
      renderer.render({
        container: specMaskedSprite,
        target: this._normalMappedGroundRT,
        clear: false,
      });
      specMaskedSprite.destroy();
    }

    // ------------------------------------------------------------------
    // 4. Objects base → _objectsRT
    // ------------------------------------------------------------------
    {
      const tmp = new Container();
      tmp.position.copyFrom(cw.position);
      tmp.scale.copyFrom(cw.scale);
      tmp.pivot.copyFrom(cw.pivot);
      tmp.sortableChildren = true;
      tmp.addChild(this._objectsContainer);
      renderer.render({ container: tmp, target: this._objectsRT, clear: true });
      tmp.removeChild(this._objectsContainer);
      tmp.destroy();
    }

    // ------------------------------------------------------------------
    // 5. Objects normals → _objectsNormalRT (neutral normal = 0x8080FF)
    // ------------------------------------------------------------------
    {
      const neutralFill = new Graphics();
      neutralFill.rect(0, 0, GAME_W, GAME_H).fill(0x8080ff);
      renderer.render({
        container: neutralFill,
        target: this._objectsNormalRT,
        clear: true,
      });
      neutralFill.destroy();

      const tmp = new Container();
      tmp.position.copyFrom(cw.position);
      tmp.scale.copyFrom(cw.scale);
      tmp.pivot.copyFrom(cw.pivot);
      tmp.sortableChildren = true;
      tmp.addChild(this._objectsNormalContainer);
      renderer.render({
        container: tmp,
        target: this._objectsNormalRT,
        clear: false,
      });
      tmp.removeChild(this._objectsNormalContainer);
      tmp.destroy();
    }

    // ------------------------------------------------------------------
    // 6. Normal-map objects pass → _normalMappedObjectsRT
    //    C#: AmbientColor è dentro lo shader, accumulato N volte via additive blend
    // ------------------------------------------------------------------
    {
      // Clear RT to black — l'ambient viene ora accumulato dallo shader per ogni luce
      {
        const clearFill = new Graphics();
        clearFill.rect(0, 0, GAME_W, GAME_H).fill(0x000000);
        renderer.render({
          container: clearFill,
          target: this._normalMappedObjectsRT,
          clear: true,
        });
        clearFill.destroy();
      }

      this._normalMapFilter.setNormalMap(this._objectsNormalRT);
      this._normalMapFilter.setAmbientColor(
        debug.forceAmbient ?? this.normalMapAmbienceColor,
      );
      this._normalMapFilter.blendMode = "add";

      for (let i = 0; i < this.lights.length; i++) {
        const light = this.lights[i]!;
        if (!light.enabled) continue;
        if (
          debug.singleLightIndex !== undefined &&
          i !== debug.singleLightIndex
        )
          continue;

        const { sx, sy } = this._worldToScreen(
          light.position.x + light.deltaForWall.x,
          light.position.y + light.deltaForWall.y,
          cw,
        );
        this._normalMapFilter.setLight(
          sx,
          sy,
          // C#: Z non viene scalato per lo zoom (solo XY sono in screen-space)
          light.z,
          light.colorNormalVec3,
          light.distanceSquaredForWall,
          light.opacity,
        );

        const filterContainer = new Container();
        filterContainer.filters = [this._normalMapFilter];
        const diffuseSprite = new Sprite(this._objectsRT);
        filterContainer.addChild(diffuseSprite);
        renderer.render({
          container: filterContainer,
          target: this._normalMappedObjectsRT,
          clear: false,
        });
        filterContainer.destroy({ children: true });
      }

      this._normalMapFilter.blendMode = "normal";
    }

    // ------------------------------------------------------------------
    // 7. Shadow lightmap (unchanged from Task 4)
    // ------------------------------------------------------------------
    {
      const ambientGfx = new Graphics();
      ambientGfx
        .rect(0, 0, GAME_W, GAME_H)
        .fill({ color: this.ambientColor, alpha: this.ambientAlpha });
      renderer.render({
        container: ambientGfx,
        target: this._lightmap,
        clear: true,
      });
      ambientGfx.destroy();
    }

    for (let i = 0; i < this.lights.length; i++) {
      const light = this.lights[i]!;
      if (!light.enabled) continue;
      if (debug.singleLightIndex !== undefined && i !== debug.singleLightIndex)
        continue;

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
          const polys = hull.computeShadowPolygons(
            penumbraPos.x,
            penumbraPos.y,
          );
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

      renderer.render({
        container: perLightContainer,
        target: this._perLightRT,
        clear: true,
      });
      perLightContainer.destroy({ children: true });

      // Blur pass
      {
        this._blurFilter.strength = light.sourceRadius * 0.15;
        const filterContainer = new Container();
        filterContainer.filters = [this._blurFilter];
        const perLightSprite = new Sprite(this._perLightRT);
        filterContainer.addChild(perLightSprite);
        renderer.render({
          container: filterContainer,
          target: this._blurredRT,
          clear: true,
        });
        filterContainer.destroy({ children: true });
      }

      // Additive composite → lightmap
      {
        const blurredSprite = new Sprite(this._blurredRT);
        blurredSprite.blendMode = "add";
        renderer.render({
          container: blurredSprite,
          target: this._lightmap,
          clear: false,
        });
        blurredSprite.destroy();
      }
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

    this._frameIndex += 1;
    if (
      debug.enabled &&
      this._frameIndex % Math.max(1, debug.logEveryNFrames) === 0
    ) {
      const enabledLights = this.lights.filter((l) => l.enabled).length;
      const firstLight = this.lights.find((l) => l.enabled);
      const firstLightScreenPos = firstLight
        ? this._worldToScreen(
            firstLight.position.x + firstLight.deltaForGround.x,
            firstLight.position.y + firstLight.deltaForGround.y,
            cw,
          )
        : null;

      const groundLuma = debug.sampleLuma
        ? this._estimateLuma(renderer, this._normalMappedGroundRT)
        : null;
      const objectsLuma = debug.sampleLuma
        ? this._estimateLuma(renderer, this._normalMappedObjectsRT)
        : null;
      const lightmapLuma = debug.sampleLuma
        ? this._estimateLuma(renderer, this._lightmap)
        : null;
      const groundRTLuma = debug.sampleLuma
        ? this._estimateLuma(renderer, this._groundRT)
        : null;
      const groundNMLuma = debug.sampleLuma
        ? this._estimateLuma(renderer, this._groundNormalRT)
        : null;
      const objectsRTLuma = debug.sampleLuma
        ? this._estimateLuma(renderer, this._objectsRT)
        : null;
      const objectsNMLuma = debug.sampleLuma
        ? this._estimateLuma(renderer, this._objectsNormalRT)
        : null;
      const perLightLuma = debug.sampleLuma
        ? this._estimateLuma(renderer, this._perLightRT)
        : null;

      console.log("[ShadowSystem:debug] " + JSON.stringify({
        lightsTotal: this.lights.length,
        lightsEnabled: enabledLights,
        singleLightIndex: debug.singleLightIndex,
        ambient: debug.forceAmbient ?? this.normalMapAmbienceColor,
        disableMultiply: debug.disableMultiply,
        rendererResolution: renderer.resolution,
        camera: {
          position: { x: cw.position.x, y: cw.position.y },
          scale: { x: cw.scale.x, y: cw.scale.y },
          pivot: { x: cw.pivot.x, y: cw.pivot.y },
        },
        firstLightScreenPos,
        firstLightZ: firstLight?.z,
        firstLightDistSq: firstLight?.distanceSquaredForGround,
        luma: {
          groundRT: groundRTLuma,
          groundNM: groundNMLuma,
          objectsRT: objectsRTLuma,
          objectsNM: objectsNMLuma,
          normalMappedGround: groundLuma,
          normalMappedObjects: objectsLuma,
          lightmap: lightmapLuma,
          perLightRT: perLightLuma,
        },
      }));
    }
  }

  destroy(): void {
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
    };
  }

  private _estimateLuma(
    renderer: Application["renderer"],
    target: RenderTexture,
  ): number | null {
    // PixiJS 8: renderer.extract.pixels() returns { pixels: Uint8Array, width, height }
    const extract = (
      renderer as unknown as {
        extract: {
          pixels: (value: RenderTexture) => { pixels: Uint8Array } | Uint8Array;
        };
      }
    ).extract;
    console.log("[luma-debug] extract=" + typeof extract + " keys=" + (renderer ? Object.keys(renderer).filter(k => k.toLowerCase().includes("extract") || k.toLowerCase().includes("pixel")).join(",") : "no-renderer"));
    if (!extract) return null;

    try {
      const result = extract.pixels(target);
      const pixels =
        result instanceof Uint8Array
          ? result
          : (result as { pixels: Uint8Array }).pixels;
      console.log("[luma-debug] pixels type=" + (pixels ? pixels.constructor.name : "null") + " len=" + (pixels ? pixels.length : 0) + " isUint8=" + (pixels instanceof Uint8Array));
      if (!(pixels instanceof Uint8Array) || pixels.length < 4) return null;

      const pixelCount = Math.floor(pixels.length / 4);
      const stride = Math.max(1, Math.floor(pixelCount / 256));
      let sum = 0;
      let count = 0;

      for (let p = 0; p < pixelCount; p += stride) {
        const idx = p * 4;
        const r = pixels[idx] ?? 0;
        const g = pixels[idx + 1] ?? 0;
        const b = pixels[idx + 2] ?? 0;
        sum += 0.2126 * r + 0.7152 * g + 0.0722 * b;
        count += 1;
      }

      if (count === 0) return null;
      return sum / (count * 255);
    } catch (e) {
      console.log("[luma-error] " + String(e));
      return null;
    }
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
   * Pre-renders a white radial gradient circle into a RenderTexture.
   * Uses additive-blended concentric filled circles so the centre reaches
   * alpha=1 and the edge fades to ~0 linearly.
   */
  private _createLightTexture(size: number): RenderTexture {
    const rt = RenderTexture.create({ width: size, height: size });
    const root = new Container();
    const steps = 24;
    const half = size / 2;

    for (let i = steps; i >= 1; i--) {
      const r = (i / steps) * half;
      const gfx = new Graphics();
      gfx.blendMode = "add";
      gfx.circle(half, half, r).fill({ color: 0xffffff, alpha: 1 / steps });
      root.addChild(gfx);
    }

    this._app.renderer.render({ container: root, target: rt, clear: true });
    root.destroy({ children: true });
    return rt;
  }
}
