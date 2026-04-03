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
    hullRects: Rectangle[];

    private readonly _sprite: Sprite;
    private readonly _arcSprite: Sprite | null;
    private readonly _textureOpen: Texture | undefined;
    private _normalSprite: Sprite | null = null;
    private readonly _normalTextureOpen: Texture | undefined;
    private _isOpen = false;

    constructor(
        direction: LinkDirection,
        x: number,
        y: number,
        textureClosed: Texture,
        openTexture?: Texture,
        arcTexture?: Texture,
        arcX?: number,
        normalTextureClosed?: Texture,
        normalTextureOpen?: Texture,
    ) {
        this.direction = direction;
        this._textureOpen = openTexture;
        this._normalTextureOpen = normalTextureOpen;

        this._sprite = new Sprite(textureClosed);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForWall(y + textureClosed.height / 2);

        if (normalTextureClosed) {
            this._normalSprite = new Sprite(normalTextureClosed);
            this._normalSprite.x = x;
            this._normalSprite.y = y;
            this._normalSprite.zIndex = zForWall(y + textureClosed.height / 2);
        }

        if (arcTexture) {
            this._arcSprite = new Sprite(arcTexture);
            this._arcSprite.x = arcX ?? x;
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
        this.hullRects = [this.collisionRectangle];
    }

    get isOpen(): boolean { return this._isOpen; }

    setOpen(): void {
        if (this._isOpen) return;
        this._isOpen = true;
        if (this._textureOpen) this._sprite.texture = this._textureOpen;
        if (this._normalSprite && this._normalTextureOpen) {
            this._normalSprite.texture = this._normalTextureOpen;
        }
        this.hullRects = [];
    }

    update(_deltaMS: number): void { /* static */ }

    addNormalToContainer(container: Container): void {
        if (this._normalSprite) container.addChild(this._normalSprite);
    }

    addToContainer(container: Container): void {
        container.addChild(this._sprite);
        if (this._arcSprite) container.addChild(this._arcSprite);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._sprite);
        if (this._arcSprite) container.removeChild(this._arcSprite);
    }
}
