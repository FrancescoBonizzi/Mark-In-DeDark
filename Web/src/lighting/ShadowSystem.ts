import {
    Application,
    BlurFilter,
    Container,
    Graphics,
    RenderTexture,
    Sprite,
} from 'pixi.js';
import type Camera from '../world/Camera';
import { Hull } from './Hull';
import type { MarkLight } from './MarkLight';
import type { IWall } from '../grid/IWall';
import type { Pillar } from '../grid/Pillar';

const GAME_W = 1280;
const GAME_H = 720;
/** Size (px) of the pre-rendered radial gradient texture. */
const LIGHT_TEX_SIZE = 256;

/**
 * Two-layer shadow system:
 *
 *  Stage layout after construction:
 *    [0] _groundLitSprite  — fullscreen sprite showing the ground pass
 *    [1] _lightmapSprite   — lightmap composited with 'multiply' blend
 *    [2] camera._world     — walls, pillars, objects (always fully visible)
 *
 * Per-frame pipeline:
 *   1. Render groundContainer (with camera transform) → _groundRT.
 *   2. Fill _lightmap with ambient colour.
 *   3. For each light: build per-light container → _perLightRT, then
 *      blur-composite _perLightRT → _lightmap (additive).
 *      Shadows are black-filled polygons (normal blend) that overwrite
 *      light pixels with black; black adds 0 when composited → no light
 *      behind occluders.
 *   4. _lightmapSprite (multiply) composites lightmap over ground in the
 *      display list — PixiJS 8 advanced blend modes work correctly here.
 *
 * Walls/pillars live above the lightmap sprite ([2]), so they are always
 * fully visible regardless of lighting.
 */
export class ShadowSystem {
    lights: MarkLight[] = [];
    hulls:  Hull[]      = [];

    ambientColor = 0x000000;
    ambientAlpha = 1.0;

    private readonly _app:    Application;
    private readonly _camera: Camera;
    private readonly _groundContainer: Container;

    private readonly _groundRT:        RenderTexture;
    private readonly _lightmap:        RenderTexture;
    private readonly _perLightRT:      RenderTexture;
    private readonly _blurredRT:       RenderTexture;
    private readonly _lightTexture:    RenderTexture;
    private readonly _groundLitSprite: Sprite;
    private readonly _lightmapSprite:  Sprite;

    /** Cached blur filter — updated per-light, never recreated. */
    private readonly _blurFilter = new BlurFilter({ strength: 3, quality: 2 });

    constructor(app: Application, camera: Camera, groundContainer: Container) {
        this._app    = app;
        this._camera = camera;
        this._groundContainer = groundContainer;

        this._groundRT   = RenderTexture.create({ width: GAME_W, height: GAME_H });
        this._lightmap   = RenderTexture.create({ width: GAME_W, height: GAME_H });
        this._perLightRT = RenderTexture.create({ width: GAME_W, height: GAME_H });
        this._blurredRT  = RenderTexture.create({ width: GAME_W, height: GAME_H });

        this._lightTexture = this._createLightTexture(LIGHT_TEX_SIZE);

        // [0] Ground pass (lit by lightmap sprite above it)
        this._groundLitSprite = new Sprite(this._groundRT);
        app.stage.addChildAt(this._groundLitSprite, 0);

        // [1] Lightmap — multiply blend composites lighting over ground in
        //     the display list (PixiJS 8 advanced blends work on screen FB).
        this._lightmapSprite = new Sprite(this._lightmap);
        this._lightmapSprite.blendMode = 'multiply';
        app.stage.addChildAt(this._lightmapSprite, 1);
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

    /** Creates Hull objects from each wall's current hullRects (snapshot). */
    addHullsFromWalls(walls: IWall[]): void {
        for (const wall of walls) {
            for (const rect of wall.hullRects) {
                this.hulls.push(Hull.fromRectangle(rect));
            }
        }
    }

    /** Creates Hull objects from each pillar's current hullRects (snapshot). */
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
        for (const light of this.lights) {
            light.update(deltaMS);
        }

        // 1. Render groundContainer (with camera transform) → _groundRT
        {
            const tempContainer = new Container();
            tempContainer.position.copyFrom(cw.position);
            tempContainer.scale.copyFrom(cw.scale);
            tempContainer.pivot.copyFrom(cw.pivot);
            tempContainer.addChild(this._groundContainer);

            renderer.render({ container: tempContainer, target: this._groundRT, clear: true });

            // Remove without destroying (groundContainer must survive)
            tempContainer.removeChild(this._groundContainer);
            tempContainer.destroy();
        }

        // 2. Fill lightmap with ambient colour
        {
            const ambientGfx = new Graphics();
            ambientGfx
                .rect(0, 0, GAME_W, GAME_H)
                .fill({ color: this.ambientColor, alpha: this.ambientAlpha });
            renderer.render({ container: ambientGfx, target: this._lightmap, clear: true });
            ambientGfx.destroy();
        }

        // 3. Per-light pass
        for (const light of this.lights) {
            if (!light.enabled) continue;

            // Container that mirrors the camera world transform so we can
            // draw in world-space coordinates and get correct screen output.
            const perLightContainer = new Container();
            perLightContainer.position.copyFrom(cw.position);
            perLightContainer.scale.copyFrom(cw.scale);
            perLightContainer.pivot.copyFrom(cw.pivot);

            // 3a. Light circle sprite (world coords, tinted to current colour)
            const lightSprite = new Sprite(this._lightTexture);
            lightSprite.scale.set(light.radius / LIGHT_TEX_SIZE);
            lightSprite.anchor.set(0.5);
            lightSprite.x = light.position.x;
            lightSprite.y = light.position.y;
            lightSprite.tint  = light.currentColor;
            lightSprite.alpha = light.opacity;
            perLightContainer.addChild(lightSprite);

            // 3b. Shadow trapezoids — black normal-blend polygons overwrite
            //     lit pixels with black. When composited additively onto the
            //     lightmap, black adds 0 → no light contribution behind walls.
            if (light.castsShadows && this.hulls.length > 0) {
                const shadowGfx = new Graphics();

                for (const hull of this.hulls) {
                    if (!hull.enabled) continue;
                    const polys = hull.computeShadowPolygons(
                        light.position.x,
                        light.position.y,
                    );
                    for (const poly of polys) {
                        shadowGfx.poly(poly).fill(0x000000);
                    }
                }
                perLightContainer.addChild(shadowGfx);

                // Hull fill — black out hull surfaces (ShadowType.Solid).
                const hullFillGfx = new Graphics();
                for (const hull of this.hulls) {
                    if (!hull.enabled) continue;
                    const b = hull.bounds;
                    hullFillGfx.rect(b.x, b.y, b.width, b.height).fill(0x000000);
                }
                perLightContainer.addChild(hullFillGfx);
            }

            // 3c. Render per-light container → _perLightRT
            renderer.render({
                container: perLightContainer,
                target: this._perLightRT,
                clear: true,
            });
            perLightContainer.destroy({ children: true });

            // 3d-i. Blur pass: _perLightRT → _blurredRT (normal blend, filter applied)
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

            // 3d-ii. Composite pass: _blurredRT → _lightmap (additive, no filter)
            {
                const blurredSprite = new Sprite(this._blurredRT);
                blurredSprite.blendMode = 'add';
                renderer.render({
                    container: blurredSprite,
                    target: this._lightmap,
                    clear: false,
                });
                blurredSprite.destroy();
            }
        }

        // _lightmapSprite in the display list handles the final multiply composite.
    }

    destroy(): void {
        this._groundRT.destroy();
        this._lightmap.destroy();
        this._perLightRT.destroy();
        this._blurredRT.destroy();
        this._lightTexture.destroy();
        this._groundLitSprite.destroy();
        this._lightmapSprite.destroy();
        this._blurFilter.destroy();
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

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
