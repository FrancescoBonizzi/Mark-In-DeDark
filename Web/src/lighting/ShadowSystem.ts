import {
    Application,
    BlurFilter,
    Container,
    Graphics,
    RenderTexture,
    Sprite,
    Texture,
} from 'pixi.js';
import type Camera from '../world/Camera';
import { Hull } from './Hull';
import { NormalMapFilter } from './NormalMapFilter';
import type { MarkLight } from './MarkLight';
import type { IWall } from '../grid/IWall';
import type { Pillar } from '../grid/Pillar';

const GAME_W = 1280;
const GAME_H = 720;
/** Size (px) of the pre-rendered radial gradient texture. */
const LIGHT_TEX_SIZE = 256;

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
    hulls:  Hull[]      = [];

    ambientColor = 0x000000;
    ambientAlpha = 1.0;
    normalMapAmbienceColor: { r: number; g: number; b: number } = { r: 0.35, g: 0.35, b: 0.35 };
    /** Scale of glow sprites for specular lightmask and shadow lightmap (C# GlowScale). Set from LevelCustomization. */
    glowScale = 2.0;

    private readonly _app:    Application;
    private readonly _camera: Camera;

    private readonly _groundContainer:          Container;
    private readonly _groundNormalContainer:     Container;
    private readonly _groundSpecularContainer:   Container;
    private readonly _objectsContainer:          Container;
    private readonly _objectsNormalContainer:    Container;

    /** Glow sprite texture used for specular lightmask and shadow lightmap (C# Effects/glow). Null if not available. */
    private readonly _glowTexture: Texture | null;

    // RenderTextures
    private readonly _groundRT:              RenderTexture;
    private readonly _groundNormalRT:        RenderTexture;
    private readonly _groundSpecularRT:      RenderTexture;
    private readonly _specularLightmaskRT:   RenderTexture;
    private readonly _specularMaskedRT:      RenderTexture;
    private readonly _normalMappedGroundRT:  RenderTexture;
    private readonly _objectsRT:             RenderTexture;
    private readonly _objectsNormalRT:       RenderTexture;
    private readonly _normalMappedObjectsRT: RenderTexture;
    private readonly _lightmap:              RenderTexture;
    private readonly _perLightRT:            RenderTexture;
    private readonly _blurredRT:             RenderTexture;
    private readonly _lightTexture:          RenderTexture;

    // Display-list sprites
    private readonly _normalMappedGroundSprite:  Sprite;
    private readonly _lightmapSprite:            Sprite;
    private readonly _normalMappedObjectsSprite: Sprite;

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
        this._app    = app;
        this._camera = camera;
        this._groundContainer        = groundContainer;
        this._groundNormalContainer  = groundNormalContainer;
        this._groundSpecularContainer = groundSpecularContainer;
        this._objectsContainer       = objectsContainer;
        this._objectsNormalContainer = objectsNormalContainer;
        this._glowTexture            = glowTexture;

        const mkRT = () => RenderTexture.create({ width: GAME_W, height: GAME_H });

        this._groundRT              = mkRT();
        this._groundNormalRT        = mkRT();
        this._groundSpecularRT      = mkRT();
        this._specularLightmaskRT   = mkRT();
        this._specularMaskedRT      = mkRT();
        this._normalMappedGroundRT  = mkRT();
        this._objectsRT             = mkRT();
        this._objectsNormalRT       = mkRT();
        this._normalMappedObjectsRT = mkRT();
        this._lightmap              = mkRT();
        this._perLightRT            = mkRT();
        this._blurredRT             = mkRT();
        this._lightTexture          = this._createLightTexture(LIGHT_TEX_SIZE);

        // [0] Ground with normal map lighting
        this._normalMappedGroundSprite = new Sprite(this._normalMappedGroundRT);
        app.stage.addChildAt(this._normalMappedGroundSprite, 0);

        // [1] Shadow lightmap — multiply blend darkens the ground
        this._lightmapSprite = new Sprite(this._lightmap);
        this._lightmapSprite.blendMode = 'multiply';
        app.stage.addChildAt(this._lightmapSprite, 1);

        // [2] Walls/pillars with normal map lighting (above lightmap — no shadow multiply)
        this._normalMappedObjectsSprite = new Sprite(this._normalMappedObjectsRT);
        app.stage.addChildAt(this._normalMappedObjectsSprite, 2);
    }

    // -------------------------------------------------------------------------
    // Hull / light management
    // -------------------------------------------------------------------------

    addLight(light: MarkLight): void { this.lights.push(light); }

    removeLight(light: MarkLight): void {
        const i = this.lights.indexOf(light);
        if (i !== -1) this.lights.splice(i, 1);
    }

    addHull(hull: Hull): void { this.hulls.push(hull); }

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
            renderer.render({ container: blackFill, target: this._groundRT, clear: true });
            blackFill.destroy();

            renderWithCameraTransform(this._groundContainer, this._groundRT, false);
        }

        // ------------------------------------------------------------------
        // 2. Ground normals → _groundNormalRT (cleared to black = no normal)
        // ------------------------------------------------------------------
        {
            const blackFill = new Graphics();
            blackFill.rect(0, 0, GAME_W, GAME_H).fill(0x000000);
            renderer.render({ container: blackFill, target: this._groundNormalRT, clear: true });
            blackFill.destroy();

            renderWithCameraTransform(this._groundNormalContainer, this._groundNormalRT, false);
        }

        // ------------------------------------------------------------------
        // 2a. Ground specular → _groundSpecularRT (C#: specularMapRenderTarget)
        // ------------------------------------------------------------------
        {
            const blackFill = new Graphics();
            blackFill.rect(0, 0, GAME_W, GAME_H).fill(0x000000);
            renderer.render({ container: blackFill, target: this._groundSpecularRT, clear: true });
            blackFill.destroy();

            renderWithCameraTransform(this._groundSpecularContainer, this._groundSpecularRT, false);
        }

        // ------------------------------------------------------------------
        // 2b. Specular lightmask → _specularLightmaskRT
        //     C#: glow sprites from all lights with NonPremultiplied blend (specularMapLightmaskRenderTarget)
        // ------------------------------------------------------------------
        {
            const blackFill = new Graphics();
            blackFill.rect(0, 0, GAME_W, GAME_H).fill(0x000000);
            renderer.render({ container: blackFill, target: this._specularLightmaskRT, clear: true });
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

                renderer.render({ container: glowContainer, target: this._specularLightmaskRT, clear: false });
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
            renderer.render({ container: lightmaskSprite, target: this._specularMaskedRT, clear: true });
            lightmaskSprite.destroy();

            // Multiply specular onto the lightmask → result = specular * lightmask
            const specularSprite = new Sprite(this._groundSpecularRT);
            specularSprite.blendMode = 'multiply';
            renderer.render({ container: specularSprite, target: this._specularMaskedRT, clear: false });
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
                renderer.render({ container: clearFill, target: this._normalMappedGroundRT, clear: true });
                clearFill.destroy();
            }

            this._normalMapFilter.setNormalMap(this._groundNormalRT);
            this._normalMapFilter.setAmbientColor(this.normalMapAmbienceColor);

            for (const light of this.lights) {
                if (!light.enabled) continue;

                const { sx, sy } = this._worldToScreen(
                    light.position.x + light.deltaForGround.x,
                    light.position.y + light.deltaForGround.y,
                    cw,
                );
                this._normalMapFilter.setLight(
                    sx, sy,
                    // C#: Z non viene scalato per lo zoom (solo XY sono in screen-space)
                    light.z,
                    light.colorNormalVec3,
                    light.distanceSquaredForGround,
                    light.opacity,
                );

                // Phase 1: render filter output to temp RT
                const filterContainer = new Container();
                filterContainer.filters = [this._normalMapFilter];
                const diffuseSprite = new Sprite(this._groundRT);
                filterContainer.addChild(diffuseSprite);
                renderer.render({ container: filterContainer, target: this._perLightRT, clear: true });
                filterContainer.destroy({ children: true });

                // Phase 2: additive composite onto final RT
                const addSprite = new Sprite(this._perLightRT);
                addSprite.blendMode = 'add';
                renderer.render({ container: addSprite, target: this._normalMappedGroundRT, clear: false });
                addSprite.destroy();
            }

            // C#: composizione finale = appliedNormalMap + specularWithLightmask (BlendState.Additive)
            const specMaskedSprite = new Sprite(this._specularMaskedRT);
            specMaskedSprite.blendMode = 'add';
            renderer.render({ container: specMaskedSprite, target: this._normalMappedGroundRT, clear: false });
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
            renderer.render({ container: neutralFill, target: this._objectsNormalRT, clear: true });
            neutralFill.destroy();

            const tmp = new Container();
            tmp.position.copyFrom(cw.position);
            tmp.scale.copyFrom(cw.scale);
            tmp.pivot.copyFrom(cw.pivot);
            tmp.sortableChildren = true;
            tmp.addChild(this._objectsNormalContainer);
            renderer.render({ container: tmp, target: this._objectsNormalRT, clear: false });
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
                renderer.render({ container: clearFill, target: this._normalMappedObjectsRT, clear: true });
                clearFill.destroy();
            }

            this._normalMapFilter.setNormalMap(this._objectsNormalRT);
            this._normalMapFilter.setAmbientColor(this.normalMapAmbienceColor);

            for (const light of this.lights) {
                if (!light.enabled) continue;

                const { sx, sy } = this._worldToScreen(
                    light.position.x + light.deltaForWall.x,
                    light.position.y + light.deltaForWall.y,
                    cw,
                );
                this._normalMapFilter.setLight(
                    sx, sy,
                    // C#: Z non viene scalato per lo zoom (solo XY sono in screen-space)
                    light.z,
                    light.colorNormalVec3,
                    light.distanceSquaredForWall,
                    light.opacity,
                );

                // Phase 1: render filter output to temp RT
                const filterContainer = new Container();
                filterContainer.filters = [this._normalMapFilter];
                const diffuseSprite = new Sprite(this._objectsRT);
                filterContainer.addChild(diffuseSprite);
                renderer.render({ container: filterContainer, target: this._perLightRT, clear: true });
                filterContainer.destroy({ children: true });

                // Phase 2: additive composite onto final RT
                const addSprite = new Sprite(this._perLightRT);
                addSprite.blendMode = 'add';
                renderer.render({ container: addSprite, target: this._normalMappedObjectsRT, clear: false });
                addSprite.destroy();
            }
        }

        // ------------------------------------------------------------------
        // 7. Shadow lightmap (unchanged from Task 4)
        // ------------------------------------------------------------------
        {
            const ambientGfx = new Graphics();
            ambientGfx.rect(0, 0, GAME_W, GAME_H).fill({ color: this.ambientColor, alpha: this.ambientAlpha });
            renderer.render({ container: ambientGfx, target: this._lightmap, clear: true });
            ambientGfx.destroy();
        }

        for (const light of this.lights) {
            if (!light.enabled) continue;

            const perLightContainer = new Container();
            perLightContainer.position.copyFrom(cw.position);
            perLightContainer.scale.copyFrom(cw.scale);
            perLightContainer.pivot.copyFrom(cw.pivot);

            const lightSprite = new Sprite(this._lightTexture);
            lightSprite.scale.set(light.radius / LIGHT_TEX_SIZE);
            lightSprite.anchor.set(0.5);
            lightSprite.x = light.position.x;
            lightSprite.y = light.position.y;
            lightSprite.tint  = light.currentColor;
            lightSprite.alpha = light.opacity;
            perLightContainer.addChild(lightSprite);

            if (light.castsShadows && this.hulls.length > 0) {
                const shadowGfx = new Graphics();
                for (const hull of this.hulls) {
                    if (!hull.enabled) continue;
                    const polys = hull.computeShadowPolygons(light.position.x, light.position.y);
                    for (const poly of polys) shadowGfx.poly(poly).fill(0x000000);
                }
                perLightContainer.addChild(shadowGfx);

                const hullFillGfx = new Graphics();
                for (const hull of this.hulls) {
                    if (!hull.enabled) continue;
                    const b = hull.bounds;
                    hullFillGfx.rect(b.x, b.y, b.width, b.height).fill(0x000000);
                }
                perLightContainer.addChild(hullFillGfx);
            }

            renderer.render({ container: perLightContainer, target: this._perLightRT, clear: true });
            perLightContainer.destroy({ children: true });

            // Blur pass
            {
                this._blurFilter.strength = light.sourceRadius * 0.15;
                const filterContainer = new Container();
                filterContainer.filters = [this._blurFilter];
                const perLightSprite = new Sprite(this._perLightRT);
                filterContainer.addChild(perLightSprite);
                renderer.render({ container: filterContainer, target: this._blurredRT, clear: true });
                filterContainer.destroy({ children: true });
            }

            // Additive composite → lightmap
            {
                const blurredSprite = new Sprite(this._blurredRT);
                blurredSprite.blendMode = 'add';
                renderer.render({ container: blurredSprite, target: this._lightmap, clear: false });
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

            for (const light of this.lights) {
                if (!light.enabled || !light.isGlowEnabled) continue;
                const glow = new Sprite(this._glowTexture);
                glow.anchor.set(0.5);
                glow.x = light.position.x + light.deltaForGround.x;
                glow.y = light.position.y + light.deltaForGround.y;
                glow.scale.set(light.lightMaskScale);
                glow.blendMode = 'add';
                glowContainer.addChild(glow);
            }

            renderer.render({ container: glowContainer, target: this._lightmap, clear: false });
            glowContainer.destroy({ children: true });
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
    private _worldToScreen(wx: number, wy: number, cw: Container): { sx: number; sy: number } {
        return {
            sx: (wx - cw.pivot.x) * cw.scale.x + cw.position.x,
            sy: (wy - cw.pivot.y) * cw.scale.y + cw.position.y,
        };
    }


    /**
     * Pre-renders a white radial gradient circle into a RenderTexture.
     * Uses additive-blended concentric filled circles so the centre reaches
     * alpha=1 and the edge fades to ~0 linearly.
     */
    private _createLightTexture(size: number): RenderTexture {
        const rt   = RenderTexture.create({ width: size, height: size });
        const root = new Container();
        const steps = 24;
        const half  = size / 2;

        for (let i = steps; i >= 1; i--) {
            const r = (i / steps) * half;
            const gfx = new Graphics();
            gfx.blendMode = 'add';
            gfx.circle(half, half, r).fill({ color: 0xffffff, alpha: 1 / steps });
            root.addChild(gfx);
        }

        this._app.renderer.render({ container: root, target: rt, clear: true });
        root.destroy({ children: true });
        return rt;
    }
}
