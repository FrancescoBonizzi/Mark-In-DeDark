import {
    Application,
    Container,
    Graphics,
    Rectangle,
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
 * Renders a per-frame lightmap (GAME_W×GAME_H RenderTexture) and overlays it
 * on the scene with blendMode='multiply', producing the "dark dungeon with
 * coloured lights" effect.
 *
 * Algorithm per frame:
 *   1. Fill _lightmap with ambientColor (black/grey).
 *   2. For each enabled light:
 *      a. Render light circle + shadow trapezoids → _perLightRT (erase blend
 *         cuts shadows from the circle).
 *      b. Composite _perLightRT → _lightmap (add blend).
 *   3. _lightmapSprite (multiply blend) sits atop the camera world.
 */
export class ShadowSystem {
    lights: MarkLight[] = [];
    hulls:  Hull[]      = [];

    ambientColor = 0x000000;
    ambientAlpha = 1.0;

    private readonly _app:    Application;
    private readonly _camera: Camera;

    private readonly _lightmap:      RenderTexture;
    private readonly _perLightRT:    RenderTexture;
    private readonly _lightmapSprite: Sprite;
    private readonly _lightTexture:  RenderTexture;

    constructor(app: Application, camera: Camera) {
        this._app    = app;
        this._camera = camera;

        this._lightmap   = RenderTexture.create({ width: GAME_W, height: GAME_H });
        this._perLightRT = RenderTexture.create({ width: GAME_W, height: GAME_H });

        this._lightTexture = this._createLightTexture(LIGHT_TEX_SIZE);

        this._lightmapSprite = new Sprite(this._lightmap);
        this._lightmapSprite.blendMode = 'multiply';
        // Add after the camera world so it composites on top
        app.stage.addChild(this._lightmapSprite);
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

    /** Creates Hull objects from each wall's current hullRects (snapshot).
     *  Static walls (WallFixed) have their rects merged to eliminate light
     *  leaks between adjacent blocks. Dynamic walls are added individually
     *  so they can update their hull at runtime. */
    addHullsFromWalls(walls: IWall[]): void {
        const staticRects: Rectangle[] = [];

        for (const wall of walls) {
            const isDynamic = 'isOpen' in wall || 'break' in wall;
            if (isDynamic) {
                for (const rect of wall.hullRects) {
                    this.hulls.push(Hull.fromRectangle(rect));
                }
            } else {
                for (const rect of wall.hullRects) {
                    staticRects.push(rect);
                }
            }
        }

        const merged = ShadowSystem._mergeRects(staticRects);
        for (const rect of merged) {
            this.hulls.push(Hull.fromRectangle(rect));
        }
    }

    /**
     * Iteratively merges axis-aligned rectangles that share an edge.
     * Merge rules (within 2px epsilon):
     *  - Horizontal: same y and h, adjacent x edges.
     *  - Vertical:   same x and w, adjacent y edges.
     */
    private static _mergeRects(rects: Rectangle[]): Rectangle[] {
        const EPS = 2;
        let list = rects.map(r => new Rectangle(r.x, r.y, r.width, r.height));
        let merged = true;

        while (merged) {
            merged = false;
            const next: Rectangle[] = [];
            const used = new Uint8Array(list.length);

            for (let i = 0; i < list.length; i++) {
                if (used[i]) continue;
                let a = list[i]!;

                for (let j = i + 1; j < list.length; j++) {
                    if (used[j]) continue;
                    const b = list[j]!;

                    // Horizontal merge: same y/h, b starts where a ends
                    if (Math.abs(a.y - b.y) < EPS && Math.abs(a.height - b.height) < EPS) {
                        if (Math.abs((a.x + a.width) - b.x) < EPS) {
                            a = new Rectangle(a.x, a.y, a.width + b.width, a.height);
                            used[j] = 1;
                            merged = true;
                            continue;
                        }
                        if (Math.abs((b.x + b.width) - a.x) < EPS) {
                            a = new Rectangle(b.x, a.y, b.width + a.width, a.height);
                            used[j] = 1;
                            merged = true;
                            continue;
                        }
                    }

                    // Vertical merge: same x/w, b starts where a ends
                    if (Math.abs(a.x - b.x) < EPS && Math.abs(a.width - b.width) < EPS) {
                        if (Math.abs((a.y + a.height) - b.y) < EPS) {
                            a = new Rectangle(a.x, a.y, a.width, a.height + b.height);
                            used[j] = 1;
                            merged = true;
                            continue;
                        }
                        if (Math.abs((b.y + b.height) - a.y) < EPS) {
                            a = new Rectangle(a.x, b.y, a.width, b.height + a.height);
                            used[j] = 1;
                            merged = true;
                            continue;
                        }
                    }
                }

                next.push(a);
            }

            list = next;
        }

        return list;
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

        // 1. Fill lightmap with ambient colour
        const ambientGfx = new Graphics();
        ambientGfx
            .rect(0, 0, GAME_W, GAME_H)
            .fill({ color: this.ambientColor, alpha: this.ambientAlpha });
        renderer.render({ container: ambientGfx, target: this._lightmap, clear: true });
        ambientGfx.destroy();

        // 2. Per-light pass
        for (const light of this.lights) {
            if (!light.enabled) continue;

            // Container that mirrors the camera world transform so we can
            // draw in world-space coordinates and get correct screen output.
            const perLightContainer = new Container();
            perLightContainer.position.copyFrom(cw.position);
            perLightContainer.scale.copyFrom(cw.scale);
            perLightContainer.pivot.copyFrom(cw.pivot);

            // 2a. Light circle sprite (world coords, tinted to current colour)
            const lightSprite = new Sprite(this._lightTexture);
            lightSprite.scale.set(light.radius / LIGHT_TEX_SIZE);
            lightSprite.anchor.set(0.5);
            lightSprite.x = light.position.x;
            lightSprite.y = light.position.y;
            lightSprite.tint  = light.currentColor;
            lightSprite.alpha = light.opacity;
            perLightContainer.addChild(lightSprite);

            // 2b. Shadow trapezoids (erase blend removes light behind occluders)
            if (light.castsShadows && this.hulls.length > 0) {
                const shadowGfx = new Graphics();
                shadowGfx.blendMode = 'erase';

                for (const hull of this.hulls) {
                    if (!hull.enabled) continue;
                    const polys = hull.computeShadowPolygons(
                        light.position.x,
                        light.position.y,
                    );
                    for (const poly of polys) {
                        shadowGfx.poly(poly).fill(0xffffff);
                    }
                }
                perLightContainer.addChild(shadowGfx);
            }

            // 2c. Render per-light container → perLightRT
            renderer.render({
                container: perLightContainer,
                target: this._perLightRT,
                clear: true,
            });
            perLightContainer.destroy({ children: true });

            // 2d. Composite perLightRT → lightmap (additive)
            const perLightSprite = new Sprite(this._perLightRT);
            perLightSprite.blendMode = 'add';
            renderer.render({
                container: perLightSprite,
                target: this._lightmap,
                clear: false,
            });
            perLightSprite.destroy();
        }
    }

    destroy(): void {
        this._lightmap.destroy();
        this._perLightRT.destroy();
        this._lightTexture.destroy();
        this._lightmapSprite.destroy();
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
