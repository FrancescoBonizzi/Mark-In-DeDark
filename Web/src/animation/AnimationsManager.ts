import { Texture } from 'pixi.js';
import { SpriteAnimation } from './SpriteAnimation';

/**
 * Registry of named animations with playback control
 * (port of C# AnimationsManager).
 */
export class AnimationsManager {
    private readonly _animations = new Map<string, SpriteAnimation>();
    private _current: SpriteAnimation | null = null;
    private _currentName = '';

    add(name: string, anim: SpriteAnimation): this {
        this._animations.set(name, anim);
        return this;
    }

    has(name: string): boolean {
        return this._animations.has(name);
    }

    /** Switch to animation by name, resetting it to frame 0. No-op if already playing. */
    play(name: string): void {
        if (this._currentName === name) return;
        const anim = this._animations.get(name);
        if (!anim) {
            console.warn(`AnimationsManager: animation "${name}" not found`);
            return;
        }
        anim.reset();
        this._current = anim;
        this._currentName = name;
    }

    /** Force play even if already playing (e.g. for non-looped death animations). */
    forcePlay(name: string): void {
        const anim = this._animations.get(name);
        if (!anim) {
            console.warn(`AnimationsManager: animation "${name}" not found`);
            return;
        }
        anim.reset();
        this._current = anim;
        this._currentName = name;
    }

    update(deltaMS: number): void {
        this._current?.update(deltaMS);
    }

    get currentFrameTexture(): Texture {
        if (!this._current) throw new Error('AnimationsManager: no animation playing');
        return this._current.currentFrame;
    }

    get currentNormalFrameTexture(): Texture | null {
        return this._current?.currentNormalFrame ?? null;
    }

    get currentAnimation(): SpriteAnimation | null {
        return this._current;
    }

    get currentName(): string {
        return this._currentName;
    }
}
