import { Container, Rectangle, Sprite, Texture } from 'pixi.js';
import { LinkDirection } from './enums';
import type { IWall } from './IWall';
import { Z_OVERALL, zForWall } from './ZIndexCalculator';

export class WallBreakable implements IWall {
    readonly direction: LinkDirection;
    readonly collisionRectangle: Rectangle;
    readonly position: { x: number; y: number };
    readonly centerPosition: { x: number; y: number };
    hullRects: Rectangle[] = [];

    private readonly _spriteIntact: Sprite;
    private readonly _spriteBrokenA: Sprite;
    private readonly _spriteBrokenB: Sprite | null;
    private _broken = false;

    constructor(
        direction: LinkDirection,
        x: number,
        y: number,
        textureIntact: Texture,
        textureBrokenA: Texture,
        textureBrokenB: Texture | null,
        isVertical: boolean,
    ) {
        this.direction = direction;

        const intactZ = zForWall(y + textureIntact.height / 2);

        this._spriteIntact = new Sprite(textureIntact);
        this._spriteIntact.x = x;
        this._spriteIntact.y = y;
        this._spriteIntact.zIndex = intactZ;

        if (isVertical && textureBrokenB) {
            // Piece B: lower section at original position
            this._spriteBrokenB = new Sprite(textureBrokenB);
            this._spriteBrokenB.x = x;
            this._spriteBrokenB.y = y;
            this._spriteBrokenB.zIndex = intactZ;
            this._spriteBrokenB.visible = false;

            // Piece A: upper section floats above (offset so bottom aligns with intact bottom)
            this._spriteBrokenA = new Sprite(textureBrokenA);
            this._spriteBrokenA.x = x;
            this._spriteBrokenA.y = y + textureIntact.height - textureBrokenA.height;
            this._spriteBrokenA.zIndex = Z_OVERALL;
            this._spriteBrokenA.visible = false;
        } else {
            // Horizontal wall: single broken piece
            this._spriteBrokenA = new Sprite(textureBrokenA);
            this._spriteBrokenA.x = x;
            this._spriteBrokenA.y = y;
            this._spriteBrokenA.zIndex = intactZ;
            this._spriteBrokenA.visible = false;
            this._spriteBrokenB = null;
        }

        this.position = { x, y };
        this.centerPosition = {
            x: x + textureIntact.width / 2,
            y: y + textureIntact.height / 2,
        };

        if (isVertical) {
            this.collisionRectangle = new Rectangle(x + 20, y, textureIntact.width - 40, textureIntact.height + 55);
        } else {
            this.collisionRectangle = new Rectangle(x, y + 100, textureIntact.width, 40);
        }

        // Bug fix: usa collisionRectangle invece del bbox del sprite (stesso problema di WallFixed)
        this.hullRects = [this.collisionRectangle];
    }

    break(): void {
        if (this._broken) return;
        this._broken = true;
        this._spriteIntact.visible = false;
        this._spriteBrokenA.visible = true;
        if (this._spriteBrokenB) this._spriteBrokenB.visible = true;
        this.hullRects = [];
    }

    update(_deltaMS: number): void { /* no tick logic */ }

    addNormalToContainer(_container: Container): void { /* no normal map for breakable walls */ }

    addToContainer(container: Container): void {
        container.addChild(this._spriteIntact);
        container.addChild(this._spriteBrokenA);
        if (this._spriteBrokenB) container.addChild(this._spriteBrokenB);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._spriteIntact);
        container.removeChild(this._spriteBrokenA);
        if (this._spriteBrokenB) container.removeChild(this._spriteBrokenB);
    }
}
