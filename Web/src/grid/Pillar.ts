import { Container, Rectangle, Sprite, Texture } from 'pixi.js';
import type { PillarPositions } from './enums';
import { zForPillar } from './ZIndexCalculator';

export class Pillar {
    readonly position: PillarPositions;
    readonly x: number;
    readonly y: number;
    /** Shadow hull rects — stub for Task 4 */
    readonly hullRects: Rectangle[] = [];

    private readonly _sprite: Sprite;

    constructor(position: PillarPositions, x: number, y: number, texture: Texture) {
        this.position = position;
        this.x = x;
        this.y = y;
        this._sprite = new Sprite(texture);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForPillar(y);
    }

    addToContainer(container: Container): void {
        container.addChild(this._sprite);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._sprite);
    }
}
