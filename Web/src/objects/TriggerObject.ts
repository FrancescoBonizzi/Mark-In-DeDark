import { Rectangle } from "pixi.js";
import { Character } from "../characters/Character";

/**
 * Base class for all trigger-based ground objects (traps, goblin triggers).
 * Port of C# TriggerObject.cs.
 */
export abstract class TriggerObject {
  protected readonly _triggerArea: Rectangle;
  protected readonly _shouldFireOnce: boolean;
  protected _currentTriggeringCharacter: Character | null = null;
  private readonly _affectedCharacters: Character[];
  private _hasFired = false;
  private _hadSomeoneOnTrigger = false;
  protected _active = true;

  constructor(
    triggerArea: Rectangle,
    shouldFireOnce: boolean,
    affectedCharacters: Character[] = [],
  ) {
    this._triggerArea = triggerArea;
    this._shouldFireOnce = shouldFireOnce;
    this._affectedCharacters = [...affectedCharacters];
  }

  get isActive(): boolean {
    return this._active;
  }

  addAffectedCharacter(character: Character): void {
    if (this._affectedCharacters.includes(character)) return;
    this._affectedCharacters.push(character);
  }

  removeAffectedCharacter(character: Character): void {
    const index = this._affectedCharacters.indexOf(character);
    if (index !== -1) this._affectedCharacters.splice(index, 1);
  }

  protected resetTriggerState(): void {
    this._hasFired = false;
    this._hadSomeoneOnTrigger = false;
    this._currentTriggeringCharacter = null;
  }

  update(deltaMS: number): void {
    if (!this._active) return;

    let someoneOnTrigger = false;

    if (!this._shouldFireOnce || !this._hasFired) {
      for (
        let index = this._affectedCharacters.length - 1;
        index >= 0;
        index--
      ) {
        const character = this._affectedCharacters[index]!;

        if (character.isDead) {
          this._affectedCharacters.splice(index, 1);
          continue;
        }

        if (!this._triggerArea.intersects(character.collisionRectangle)) {
          continue;
        }

        someoneOnTrigger = true;
        this._currentTriggeringCharacter = character;
        this._hasFired = true;
        this._onTrigger(character, deltaMS);

        if (character.isDead || character.isDying) {
          this._affectedCharacters.splice(index, 1);
        }

        if (this._shouldFireOnce) {
          this._active = false;
          break;
        }
      }
    }

    if (!someoneOnTrigger) {
      this._currentTriggeringCharacter = null;
    }

    if (this._hadSomeoneOnTrigger && !someoneOnTrigger) {
      this._onTriggerLeft?.();
      if (!this._shouldFireOnce) {
        this._hasFired = false;
      }
    }

    this._hadSomeoneOnTrigger = someoneOnTrigger;
  }

  protected abstract _onTrigger(character: Character, deltaMS: number): void;
  protected _onTriggerLeft?(): void;

  abstract destroy(): void;
}
