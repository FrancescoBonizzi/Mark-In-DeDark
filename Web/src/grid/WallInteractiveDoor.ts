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
    hullRects: Rectangle[] = [];

    private readonly _sprite: AnimatedSprite;
    private readonly _baseSprite: Sprite;
    private readonly _arcSprite: Sprite | null;
    private readonly _hullRect: Rectangle;
    private _state: DoorState = DoorState.Open;
    private _frameTimer = 0;
    private _frameIndex: number;
    private readonly _frames: Texture[];

    private readonly _baseNormalSprite: Sprite | null = null;
    private readonly _animNormalSprite: Sprite | null = null;
    private readonly _frameNormals: (Texture | null)[];

    constructor(
        direction: LinkDirection,
        x: number,
        y: number,
        closedTexture: Texture,
        frames: Texture[],
        arcTexture?: Texture,
        closedNormalTexture?: Texture,
        frameNormals?: (Texture | null)[],
    ) {
        this.direction = direction;
        this._frames = frames;
        this._frameNormals = frameNormals ?? [];
        this._frameIndex = frames.length > 0 ? frames.length - 1 : 0;

        this._baseSprite = new Sprite(closedTexture);
        this._baseSprite.x = x;
        this._baseSprite.y = y;
        this._baseSprite.zIndex = zForWall(y + closedTexture.height / 2);

        if (closedNormalTexture) {
            const s = new Sprite(closedNormalTexture);
            s.x = x;
            s.y = y;
            s.zIndex = zForWall(y + closedTexture.height / 2);
            this._baseNormalSprite = s;
        }

        if (arcTexture) {
            this._arcSprite = new Sprite(arcTexture);
            this._arcSprite.x = x;
            this._arcSprite.y = y;
            this._arcSprite.zIndex = Z_OVERALL;
        } else {
            this._arcSprite = null;
        }

        // Build full animation: closed → open → closed
        const allFrames = frames.length > 0 ? frames : [closedTexture];
        this._sprite = new AnimatedSprite(allFrames);
        this._sprite.x = x;
        this._sprite.y = y - 11;
        this._sprite.zIndex = zForWall(y + closedTexture.height / 2) + 0.0001;
        this._sprite.stop();
        this._sprite.currentFrame = this._frameIndex;
        this._sprite.loop = false;
        // Start at open frame
        if (frames.length > 0) {
            this._sprite.texture = frames[this._frameIndex]!;
        }

        // Normal sprite for the animated door frames
        if (this._frameNormals.length > 0) {
            const firstNormal = this._frameNormals[this._frameIndex] ?? this._frameNormals[0];
            if (firstNormal) {
                const s = new Sprite(firstNormal);
                s.x = x;
                s.y = y - 11;
                s.zIndex = zForWall(y + closedTexture.height / 2) + 0.0001;
                this._animNormalSprite = s;
            }
        }

        this.position = { x, y };
        this.centerPosition = {
            x: x + closedTexture.width / 2,
            y: y + closedTexture.height / 2,
        };

        this.collisionRectangle = new Rectangle(x, y + 164, closedTexture.width, 32);
        this._hullRect = this.collisionRectangle;
    }

    get state(): DoorState { return this._state; }
    get isOpen(): boolean { return this._state === DoorState.Open; }
    get isMoving(): boolean { return this._state === DoorState.Opening || this._state === DoorState.Closing; }

    open(): void {
        if (this._state === DoorState.Open || this._state === DoorState.Opening) return;
        this._state = DoorState.Opening;
        this._frameIndex = 0;
        this._frameTimer = 0;
        this.hullRects = [];
    }

    close(): void {
        if (this._state === DoorState.Closed || this._state === DoorState.Closing) return;
        this._state = DoorState.Closing;
        this._frameIndex = this._frames.length - 1;
        this._frameTimer = 0;
        this.hullRects = [this._hullRect];
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
        if (this._animNormalSprite && this._frameNormals.length > 0) {
            const normalTex = this._frameNormals[this._frameIndex];
            if (normalTex) this._animNormalSprite.texture = normalTex;
        }
    }

    addNormalToContainer(container: Container): void {
        if (this._baseNormalSprite) container.addChild(this._baseNormalSprite);
        if (this._animNormalSprite) container.addChild(this._animNormalSprite);
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
