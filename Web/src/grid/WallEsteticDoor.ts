import { Container, Rectangle, Sprite, Texture } from 'pixi.js';
import { LinkDirection } from './enums';
import type { IWall } from './IWall';
import { Z_OVERALL, zForWall } from './ZIndexCalculator';

/** Entrance or exit door — purely visual, no open/close animation */
export class WallEsteticDoor implements IWall {
    readonly direction: LinkDirection;
    readonly collisionRectangle: Rectangle;
    readonly position: { x: number; y: number };
    readonly centerPosition: { x: number; y: number };
    readonly hullRects: Rectangle[] = [];

    private readonly _sprite: Sprite;
    private readonly _arcSprite: Sprite | null;

    constructor(
        direction: LinkDirection,
        x: number,
        y: number,
        textureClosed: Texture,
        arcTexture?: Texture,
    ) {
        this.direction = direction;

        this._sprite = new Sprite(textureClosed);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForWall(y + textureClosed.height / 2);

        if (arcTexture) {
            this._arcSprite = new Sprite(arcTexture);
            this._arcSprite.x = x;
            this._arcSprite.y = y - 40;
            this._arcSprite.zIndex = Z_OVERALL;
        } else {
            this._arcSprite = null;
        }

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
        if (this._arcSprite) container.addChild(this._arcSprite);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._sprite);
        if (this._arcSprite) container.removeChild(this._arcSprite);
    }
}
