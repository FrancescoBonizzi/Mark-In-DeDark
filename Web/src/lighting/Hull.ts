import { Rectangle } from 'pixi.js';

const FAR = 5000;

/**
 * A rectangular occluder that casts shadows away from a light source.
 * Vertices are in world-space pixel coordinates (y increases downward).
 * Order: TL → BL → BR → TR (clockwise in screen coords).
 */
export class Hull {
    enabled = true;

    readonly vertices: [
        { x: number; y: number },
        { x: number; y: number },
        { x: number; y: number },
        { x: number; y: number },
    ];

    constructor(rect: Rectangle) {
        // Clockwise in screen coords (y-down): TL, BL, BR, TR
        this.vertices = [
            { x: rect.x,              y: rect.y              },
            { x: rect.x,              y: rect.y + rect.height },
            { x: rect.x + rect.width, y: rect.y + rect.height },
            { x: rect.x + rect.width, y: rect.y              },
        ];
    }

    static fromRectangle(rect: Rectangle): Hull {
        return new Hull(rect);
    }

    /**
     * Returns shadow trapezoid polygons (as flat [x,y,...] arrays) for all
     * back-facing edges relative to the given light position.
     *
     * For a CW polygon in screen coords, outward normal of edge p1→p2 is:
     *   normalX = -(p2.y - p1.y)
     *   normalY =  (p2.x - p1.x)
     * A back-facing edge has dot(normal, midpoint - light) > 0.
     */
    computeShadowPolygons(lightX: number, lightY: number): number[][] {
        const verts = this.vertices;
        const n = verts.length;
        const result: number[][] = [];

        for (let i = 0; i < n; i++) {
            const p1 = verts[i]!;
            const p2 = verts[(i + 1) % n]!;

            const edgeDX = p2.x - p1.x;
            const edgeDY = p2.y - p1.y;

            // Outward normal for CW polygon (screen coords, y-down)
            const normalX = -edgeDY;
            const normalY =  edgeDX;

            // Vector from light to edge midpoint
            const midX = (p1.x + p2.x) / 2 - lightX;
            const midY = (p1.y + p2.y) / 2 - lightY;

            // Back-facing: normal points away from light → dot > 0
            if (normalX * midX + normalY * midY <= 0) continue;

            // Project p1 and p2 away from the light by FAR units
            const dir1X = p1.x - lightX;
            const dir1Y = p1.y - lightY;
            const len1 = Math.sqrt(dir1X * dir1X + dir1Y * dir1Y) || 1;
            const far1X = p1.x + (dir1X / len1) * FAR;
            const far1Y = p1.y + (dir1Y / len1) * FAR;

            const dir2X = p2.x - lightX;
            const dir2Y = p2.y - lightY;
            const len2 = Math.sqrt(dir2X * dir2X + dir2Y * dir2Y) || 1;
            const far2X = p2.x + (dir2X / len2) * FAR;
            const far2Y = p2.y + (dir2Y / len2) * FAR;

            // Trapezoid: p1, p2, far2, far1
            result.push([p1.x, p1.y, p2.x, p2.y, far2X, far2Y, far1X, far1Y]);
        }

        return result;
    }
}
