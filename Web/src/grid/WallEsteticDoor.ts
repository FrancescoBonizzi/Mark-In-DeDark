import { Container, Rectangle, Sprite, Texture } from 'pixi.js';
import { LinkDirection } from './enums';
import type { IWall } from './IWall';
import { zForWall } from './ZIndexCalculator';

/** Entrance or exit door — purely visual, no open/close animation */
export class WallEsteticDoor implements IWall {
    readonly direction: LinkDirection;
    readonly collisionRectangle: Rectangle;
    readonly position: { x: number; y: number };
    readonly centerPosition: { x: number; y: number };
    readonly hullRects: Rectangle[] = [];

    private readonly _sprite: Sprite;

    constructor(
        direction: LinkDirection,
        x: number,
        y: number,
        textureClosed: Texture,
    ) {
        this.direction = direction;

        this._sprite = new Sprite(textureClosed);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForWall(y + textureClosed.height / 2);

        this.position = { x, y };
        this.centerPosition = {
            x: x + textureClosed.width / 2,
            y: y + textureClosed.height / 2,
        };

        this.collisionRectangle = new Rectangle(x, y + 100, textureClosed.width, 40);
    }

    update(_deltaMS: number): void { /* static */ }

    addToContainer(container: Container): void {
        container.addChild(this._sprite);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._sprite);
    }
}
