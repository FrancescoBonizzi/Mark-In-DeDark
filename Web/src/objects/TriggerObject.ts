import { Rectangle } from 'pixi.js';
import { Character } from '../characters/Character';

/**
 * Base class for all trigger-based ground objects (traps, goblin triggers).
 * Port of C# TriggerObject.cs.
 */
export abstract class TriggerObject {
    protected readonly _triggerArea: Rectangle;
    protected readonly _shouldFireOnce: boolean;
    private _hasFired = false;
    protected _active = true;

    constructor(triggerArea: Rectangle, shouldFireOnce: boolean) {
        this._triggerArea = triggerArea;
        this._shouldFireOnce = shouldFireOnce;
    }

    get isActive(): boolean { return this._active; }

    /**
     * Check if the character's collision rectangle intersects the trigger area.
     * Returns true if the trigger fires this frame.
     */
    checkAndTrigger(character: Character): boolean {
        if (!this._active) return false;
        if (this._shouldFireOnce && this._hasFired) return false;
        if (character.isDead || character.isDying) return false;

        const charRect = character.collisionRectangle;
        const triggered = this._triggerArea.intersects(charRect);

        if (triggered) {
            if (!this._hasFired) {
                this._hasFired = true;
                this._onTrigger(character);
                if (this._shouldFireOnce) this._active = false;
                return true;
            }
        } else {
            this._hasFired = this._shouldFireOnce; // only reset if multi-fire
            if (!this._shouldFireOnce) {
                this._hasFired = false;
                this._onTriggerLeft?.();
            }
        }

        return false;
    }

    protected abstract _onTrigger(character: Character): void;
    protected _onTriggerLeft?(): void;

    abstract update(deltaMS: number): void;
}
