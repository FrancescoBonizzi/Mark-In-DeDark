import { Container, Rectangle, Sprite, Texture } from 'pixi.js';
import { LinkDirection } from './enums';
import type { IWall } from './IWall';
import { zForWall } from './ZIndexCalculator';

export class WallBreakable implements IWall {
    readonly direction: LinkDirection;
    readonly collisionRectangle: Rectangle;
    readonly position: { x: number; y: number };
    readonly centerPosition: { x: number; y: number };
    readonly hullRects: Rectangle[] = [];

    private readonly _spriteIntact: Sprite;
    private readonly _textureBrokenA: Texture;
    private readonly _textureBrokenB: Texture | null;
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
        this._textureBrokenA = textureBrokenA;
        this._textureBrokenB = textureBrokenB;

        this._spriteIntact = new Sprite(textureIntact);
        this._spriteIntact.x = x;
        this._spriteIntact.y = y;
        this._spriteIntact.zIndex = zForWall(y + textureIntact.height / 2);

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

        this.hullRects = [new Rectangle(
            this.collisionRectangle.x,
            this.collisionRectangle.y,
            this.collisionRectangle.width,
            this.collisionRectangle.height,
        )];
    }

    break(): void {
        if (this._broken) return;
        this._broken = true;
        const broken = this._textureBrokenB ?? this._textureBrokenA;
        this._spriteIntact.texture = broken;
    }

    update(_deltaMS: number): void { /* no tick logic */ }

    addToContainer(container: Container): void {
        container.addChild(this._spriteIntact);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._spriteIntact);
    }
}
