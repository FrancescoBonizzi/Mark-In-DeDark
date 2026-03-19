import { AnimatedSprite, Container, Rectangle, Sprite, Texture } from 'pixi.js';
import { LinkDirection } from './enums';
import type { IWall } from './IWall';
import { Z_OVERALL, zForWall } from './ZIndexCalculator';

export const enum DoorState {
    Closed = 0,
    Opening = 1,
    Open = 2,
    Closing = 3,
}

const FRAME_DURATION_MS = 20;

export class WallInteractiveDoor implements IWall {
    readonly direction: LinkDirection;
    readonly collisionRectangle: Rectangle;
    readonly position: { x: number; y: number };
    readonly centerPosition: { x: number; y: number };
    readonly hullRects: Rectangle[] = [];

    private readonly _sprite: AnimatedSprite;
    private readonly _baseSprite: Sprite;
    private readonly _arcSprite: Sprite | null;
    private _state: DoorState = DoorState.Closed;
    private _frameTimer = 0;
    private _frameIndex = 0;
    private readonly _frames: Texture[];

    constructor(
        direction: LinkDirection,
        x: number,
        y: number,
        closedTexture: Texture,
        frames: Texture[],
        arcTexture?: Texture,
    ) {
        this.direction = direction;
        this._frames = frames;

        this._baseSprite = new Sprite(closedTexture);
        this._baseSprite.x = x;
        this._baseSprite.y = y;
        this._baseSprite.zIndex = zForWall(y + closedTexture.height / 2);

        if (arcTexture) {
            this._arcSprite = new Sprite(arcTexture);
            this._arcSprite.x = x;
            this._arcSprite.y = y - 40;
            this._arcSprite.zIndex = Z_OVERALL;
        } else {
            this._arcSprite = null;
        }

        // Build full animation: closed → open → closed
        const allFrames = frames.length > 0 ? frames : [closedTexture];
        this._sprite = new AnimatedSprite(allFrames);
        this._sprite.x = x;
        this._sprite.y = y - 11;
        this._sprite.zIndex = zForWall(y + closedTexture.height / 2);
        this._sprite.stop();
        this._sprite.currentFrame = 0;
        this._sprite.loop = false;

        this.position = { x, y };
        this.centerPosition = {
            x: x + closedTexture.width / 2,
            y: y + closedTexture.height / 2,
        };

        this.collisionRectangle = new Rectangle(x, y + 100, closedTexture.width, 40);
    }

    get state(): DoorState { return this._state; }

    open(): void {
        if (this._state === DoorState.Open || this._state === DoorState.Opening) return;
        this._state = DoorState.Opening;
        this._frameIndex = 0;
        this._frameTimer = 0;
    }

    close(): void {
        if (this._state === DoorState.Closed || this._state === DoorState.Closing) return;
        this._state = DoorState.Closing;
        this._frameIndex = this._frames.length - 1;
        this._frameTimer = 0;
    }

    update(deltaMS: number): void {
        if (this._state === DoorState.Closed || this._state === DoorState.Open) return;
        if (this._frames.length === 0) return;

        this._frameTimer += deltaMS;
        if (this._frameTimer < FRAME_DURATION_MS) return;
        this._frameTimer = 0;

        if (this._state === DoorState.Opening) {
            this._frameIndex++;
            if (this._frameIndex >= this._frames.length) {
                this._frameIndex = this._frames.length - 1;
                this._state = DoorState.Open;
            }
        } else if (this._state === DoorState.Closing) {
            this._frameIndex--;
            if (this._frameIndex < 0) {
                this._frameIndex = 0;
                this._state = DoorState.Closed;
            }
        }

        this._sprite.texture = this._frames[this._frameIndex]!;
    }

    addToContainer(container: Container): void {
        container.addChild(this._baseSprite);
        if (this._arcSprite) container.addChild(this._arcSprite);
        container.addChild(this._sprite);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._baseSprite);
        if (this._arcSprite) container.removeChild(this._arcSprite);
        container.removeChild(this._sprite);
    }
}
