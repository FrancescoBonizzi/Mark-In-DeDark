import { Container, Rectangle, Sprite, Texture } from 'pixi.js';
import type { PillarPositions } from './enums';
import type { CollisionOffset } from './LevelCustomization';
import { zForPillar } from './ZIndexCalculator';

export class Pillar {
    readonly position: PillarPositions;
    readonly x: number;
    readonly y: number;
    readonly hullRects: Rectangle[];

    private readonly _sprite: Sprite;

    constructor(
        position: PillarPositions,
        x: number,
        y: number,
        texture: Texture,
        collisionOffset: CollisionOffset,
    ) {
        this.position = position;
        this.x = x;
        this.y = y;
        this._sprite = new Sprite(texture);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForPillar(y);

        this.hullRects = [
            new Rectangle(
                x + collisionOffset.x,
                y + collisionOffset.y,
                collisionOffset.w,
                collisionOffset.h,
            ),
        ];
    }

    addToContainer(container: Container): void {
        container.addChild(this._sprite);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._sprite);
    }
}
