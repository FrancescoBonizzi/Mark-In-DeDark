import { Texture } from 'pixi.js';

/**
 * Frame-based sprite animation (port of C# SpriteAnimation).
 * Manages a sequence of textures with optional normal map variants.
 */
export class SpriteAnimation {
    readonly frames: Texture[];
    readonly normalFrames: Texture[];
    readonly frameDurationMS: number;
    readonly loop: boolean;

    private _elapsed = 0;
    private _frameIndex = 0;
    private _finished = false;

    constructor(
        frames: Texture[],
        normalFrames: Texture[],
        frameDurationMS: number,
        loop = true,
    ) {
        if (frames.length === 0) throw new Error('SpriteAnimation: frames array is empty');
        this.frames = frames;
        this.normalFrames = normalFrames;
        this.frameDurationMS = frameDurationMS;
        this.loop = loop;
    }

    update(deltaMS: number): void {
        if (this._finished) return;
        this._elapsed += deltaMS;
        while (this._elapsed >= this.frameDurationMS) {
            this._elapsed -= this.frameDurationMS;
            this._frameIndex++;
            if (this._frameIndex >= this.frames.length) {
                if (this.loop) {
                    this._frameIndex = 0;
                } else {
                    this._frameIndex = this.frames.length - 1;
                    this._finished = true;
                    return;
                }
            }
        }
    }

    reset(): void {
        this._elapsed = 0;
        this._frameIndex = 0;
        this._finished = false;
    }

    get currentFrame(): Texture {
        return this.frames[this._frameIndex]!;
    }

    get currentNormalFrame(): Texture | null {
        return this.normalFrames[this._frameIndex] ?? null;
    }

    get isFinished(): boolean {
        return this._finished;
    }
}
