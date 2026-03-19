import { Container, Rectangle, Sprite, Texture } from 'pixi.js';
import { LinkDirection } from './enums';
import type { IWall } from './IWall';
import type { CollisionOffset } from './LevelCustomization';
import { zForWall } from './ZIndexCalculator';

export class WallFixed implements IWall {
    readonly direction: LinkDirection;
    readonly collisionRectangle: Rectangle;
    readonly position: { x: number; y: number };
    readonly centerPosition: { x: number; y: number };
    readonly hullRects: Rectangle[] = [];

    private readonly _sprite: Sprite;

    constructor(
        direction: LinkDirection,
        spriteX: number,
        spriteY: number,
        texture: Texture,
        offset: CollisionOffset,
    ) {
        this.direction = direction;
        this._sprite = new Sprite(texture);
        this._sprite.x = spriteX;
        this._sprite.y = spriteY;
        this._sprite.zIndex = zForWall(spriteY + texture.height / 2);

        this.position = { x: spriteX, y: spriteY };
        this.centerPosition = {
            x: spriteX + texture.width / 2,
            y: spriteY + texture.height / 2,
        };

        this.collisionRectangle = new Rectangle(
            spriteX + offset.x,
            spriteY + offset.y,
            offset.w,
            offset.h,
        );

        this.hullRects = [
            new Rectangle(
                spriteX + offset.x,
                spriteY + offset.y,
                offset.w,
                offset.h,
            ),
        ];
    }

    update(_deltaMS: number): void { /* static wall */ }

    addToContainer(container: Container): void {
        container.addChild(this._sprite);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._sprite);
    }
}
