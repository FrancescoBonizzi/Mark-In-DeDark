import { Container, Rectangle } from 'pixi.js';
import type { LinkDirection } from './enums';

export interface IWall {
    direction: LinkDirection;
    collisionRectangle: Rectangle;
    position: { x: number; y: number };
    centerPosition: { x: number; y: number };
    /** Shadow hull rectangles — populated in Task 4 */
    hullRects: Rectangle[];
    update(deltaMS: number): void;
    addToContainer(container: Container): void;
    removeFromContainer(container: Container): void;
}
