import { Container, Rectangle, Sprite } from 'pixi.js';
import { CharacterActions } from '../interaction/CharacterActions';
import { LinkDirection } from '../grid/enums';
import { GameGrid } from '../grid/GameGrid';
import { GameCell } from '../grid/GameCell';
import { MarkLight } from '../lighting/MarkLight';
import { AnimationsManager } from '../animation/AnimationsManager';
import { CharacterState } from './CharacterState';
import { zForCharacter } from '../grid/ZIndexCalculator';
import Numbers from '../services/Numbers';
const lerp = Numbers.lerp;

export interface ShadowOffsets {
    down:     { x: number; y: number };
    downIdle: { x: number; y: number };
    up:       { x: number; y: number };
    left:     { x: number; y: number };
    right:    { x: number; y: number };
}

export interface ShadowConfig {
    container:  Container;
    animations: AnimationsManager;
    offsets:    ShadowOffsets;
}

/** Directional offsets for the torch light position relative to worldCenter. */
export interface TorchOffsets {
    up: { x: number; y: number };
    down: { x: number; y: number };
    left: { x: number; y: number };
    right: { x: number; y: number };
}

export interface CharacterConfig {
    startX: number;
    startY: number;
    /** Seconds to traverse one cell. */
    movementSpeed: number;
    animations: AnimationsManager;
    canGoOverWalls?: boolean;
    torch?: {
        light: MarkLight;
        offsets: TorchOffsets;
    };
    shadow?: ShadowConfig;
}

/**
 * Abstract base class for all characters (port of C# Character.cs).
 * Implements cell-to-cell movement with linear interpolation, buffered
 * directional input with reversal support, sprite animation, and an
 * optional torch light that follows the character.
 */
export abstract class Character {
    protected readonly _grid: GameGrid;
    protected readonly _objectsContainer: Container;
    protected readonly _objectsNormalContainer: Container;

    protected _state: CharacterState = CharacterState.IdleDown;

    // Sprites
    protected readonly _mainSprite: Sprite;
    protected readonly _normalSprite: Sprite;

    // World position (equals sprite anchor = center of sprite)
    protected _x: number;
    protected _y: number;

    // Cell-to-cell lerp movement
    private _startX: number;
    private _startY: number;
    private _nextX: number;
    private _nextY: number;
    private _lerpAmount = 1; // 1 = idle (arrived)
    private _elapsedMS = 0;
    protected _movementSpeedMS: number; // ms per cell

    // Buffered input
    private _nextAction: CharacterActions = CharacterActions.Idle;

    // Movement config
    protected readonly _canGoOverWalls: boolean;

    // Torch
    readonly surroundingLight: MarkLight | null;
    private readonly _torchOffsets: TorchOffsets | null;

    // Shadow
    private readonly _shadowSprite: Sprite | null;
    private readonly _shadowAnimations: AnimationsManager | null;
    private readonly _shadowOffsets: ShadowOffsets | null;
    private readonly _shadowContainer: Container | null;
    /** Current offset applied to the shadow sprite — updated whenever direction changes, read every frame. */
    private _currentShadowOffset: { x: number; y: number } | null = null;

    /** When true, _playWalkAnimation and _playIdleAnimation are no-ops (used by Taurus charge). */
    dontPlayDefaultAnimations = false;

    // Falling-in-trap animation state
    private _isFallingInTrap = false;
    // Rates per ms (from C#: step every 35ms with per-step deltas)
    private static readonly TRAP_OPACITY_RATE  = 0.009 / 35;   // → 0 in ~3.9s
    private static readonly TRAP_SCALE_RATE    = 0.07  / 35;   // → 0 in ~0.5s
    private static readonly TRAP_ROTATION_RATE = 0.02  / 35;
    private static readonly TRAP_Y_RATE        = 3     / 35;   // px per ms

    // Placeholder dying timer: transitions Dying → Dead after a fixed delay.
    // Task 6 will replace this with a proper death animation check (animation.isFinished).
    private _dyingTimerMs = 0;
    private static readonly DYING_DURATION_MS = 1000;

    // HitBox tolerance (matches C# Rectangle(24, 24, 48, 48))
    private static readonly HIT_X = 24;
    private static readonly HIT_Y = 24;
    private static readonly HIT_W = 48;
    private static readonly HIT_H = 48;

    constructor(
        grid: GameGrid,
        objectsContainer: Container,
        objectsNormalContainer: Container,
        config: CharacterConfig,
    ) {
        this._grid = grid;
        this._objectsContainer = objectsContainer;
        this._objectsNormalContainer = objectsNormalContainer;
        this._movementSpeedMS = config.movementSpeed * 1000;
        this._canGoOverWalls = config.canGoOverWalls ?? false;

        // Initial position
        this._x = config.startX;
        this._y = config.startY;
        this._startX = this._x;
        this._startY = this._y;
        this._nextX = this._x;
        this._nextY = this._y;

        // Main sprite
        config.animations.play('IdleDown');
        this._mainSprite = new Sprite(config.animations.currentFrameTexture);
        this._mainSprite.anchor.set(0.5);
        this._mainSprite.x = this._x;
        this._mainSprite.y = this._y;
        objectsContainer.addChild(this._mainSprite);

        // Normal map sprite
        const normalTex = config.animations.currentNormalFrameTexture;
        this._normalSprite = new Sprite(normalTex ?? this._mainSprite.texture);
        this._normalSprite.visible = normalTex !== null;
        this._normalSprite.anchor.set(0.5);
        this._normalSprite.x = this._x;
        this._normalSprite.y = this._y;
        objectsNormalContainer.addChild(this._normalSprite);

        this._recalculateZIndex();

        // Torch
        if (config.torch) {
            this.surroundingLight = config.torch.light;
            this._torchOffsets = config.torch.offsets;
            this.surroundingLight.position.x = this._x + this._torchOffsets.down.x;
            this.surroundingLight.position.y = this._y + this._torchOffsets.down.y;
        } else {
            this.surroundingLight = null;
            this._torchOffsets = null;
        }

        // Shadow (C#: CharacterShadowConfiguration — black sprite, alpha 0.9, drawn on ground RT)
        if (config.shadow) {
            this._shadowAnimations = config.shadow.animations;
            this._shadowOffsets = config.shadow.offsets;
            this._shadowContainer = config.shadow.container;
            this._shadowAnimations.play('IdleDown');
            this._shadowSprite = new Sprite(this._shadowAnimations.currentFrameTexture);
            this._shadowSprite.anchor.set(0.5);
            this._shadowSprite.tint = 0x000000;
            this._shadowSprite.alpha = 0.9;
            // Position at idle-down offset
            this._shadowSprite.x = this._x + this._shadowOffsets.downIdle.x;
            this._shadowSprite.y = this._y + this._shadowOffsets.downIdle.y;
            this._shadowContainer.addChild(this._shadowSprite);
        } else {
            this._shadowSprite = null;
            this._shadowAnimations = null;
            this._shadowOffsets = null;
            this._shadowContainer = null;
        }

        // Store animations reference for subclasses
        this._animations = config.animations;
    }

    protected readonly _animations: AnimationsManager;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /** Buffers the next action to process on the next update. */
    handleInput(action: CharacterActions): void {
        this._nextAction = action;
    }

    update(deltaMS: number): void {
        if (this.isDead) return;

        if (this._isFallingInTrap) {
            this._updateFallingAnimation(deltaMS);
            return;
        }

        if (!this.isDying) {
            this._handleBufferedInput();
            this._advanceLerp(deltaMS);
        }

        if (this.isDying && !this._isFallingInTrap) {
            if (this._animations.has('Dying')) {
                // Animation-driven: wait for death animation to finish.
                if (this._animations.currentAnimation?.isFinished) {
                    this._state = CharacterState.Dead;
                    if (this.surroundingLight) this.surroundingLight.enabled = false;
                }
            } else {
                // Timer fallback for entities without a death animation.
                this._dyingTimerMs -= deltaMS;
                if (this._dyingTimerMs <= 0) {
                    this._state = CharacterState.Dead;
                    if (this.surroundingLight) this.surroundingLight.enabled = false;
                }
            }
        }

        this._animations.update(deltaMS);
        if (this._shadowAnimations) this._shadowAnimations.update(deltaMS);
        this._syncSprites();
        this._recalculateZIndex();

        if (this.surroundingLight) {
            this._updateLightPosition();
            this.surroundingLight.update(deltaMS);
        }
    }

    get worldCenter(): { x: number; y: number } {
        return { x: this._x, y: this._y };
    }

    get collisionRectangle(): Rectangle {
        const hw = this._mainSprite.texture.width / 2;
        const hh = this._mainSprite.texture.height / 2;
        return new Rectangle(
            this._x - hw + Character.HIT_X,
            this._y - hh + Character.HIT_Y,
            Character.HIT_W,
            Character.HIT_H,
        );
    }

    get currentCell(): GameCell | null {
        return this._grid.getCellAtPixel(this._x, this._y);
    }

    get isMoving(): boolean {
        return this._lerpAmount < 1 && !this.isDead && !this.isDying;
    }

    get state(): CharacterState { return this._state; }

    get movementSpeedMS(): number { return this._movementSpeedMS; }
    set movementSpeedMS(value: number) { this._movementSpeedMS = value; }

    get isDying(): boolean {
        return this._state === CharacterState.Dying;
    }

    get isDead(): boolean {
        return this._state === CharacterState.Dead;
    }

    die(): void {
        if (this.isDead || this.isDying) return;
        this._state = CharacterState.Dying;
        if (this._animations.has('Dying')) {
            // Animation-driven death: transition to Dead when animation completes.
            this._animations.forcePlay('Dying');
        } else {
            // Fallback timer for entities without a death animation (enemies).
            this._dyingTimerMs = Character.DYING_DURATION_MS;
        }
    }

    /**
     * Starts the falling-in-trap animation (port of C# CharacterFallingInTrapAnimation).
     * Character shrinks, rotates, moves toward trapCenter, and fades out.
     */
    fallInTrap(trapCenterX: number, trapCenterY: number): void {
        if (this.isDead || this._isFallingInTrap) return;
        this._isFallingInTrap = true;
        this._state = CharacterState.Dying;
        // Snap to trap center
        this._x = trapCenterX;
        this._y = trapCenterY;
        this._mainSprite.x = this._x;
        this._mainSprite.y = this._y;
        this._normalSprite.x = this._x;
        this._normalSprite.y = this._y;
    }

    private _updateFallingAnimation(deltaMS: number): void {
        const curAlpha = this._mainSprite.alpha;
        const newAlpha = Math.max(0, curAlpha - Character.TRAP_OPACITY_RATE * deltaMS);
        const newScale = Math.max(0, this._mainSprite.scale.x - Character.TRAP_SCALE_RATE * deltaMS);
        const newRot = this._mainSprite.rotation + Character.TRAP_ROTATION_RATE * deltaMS;
        this._y += Character.TRAP_Y_RATE * deltaMS;

        this._mainSprite.alpha = newAlpha;
        this._mainSprite.scale.set(newScale);
        this._mainSprite.rotation = newRot;
        this._mainSprite.y = this._y;

        this._normalSprite.alpha = newAlpha;
        this._normalSprite.scale.set(newScale);
        this._normalSprite.rotation = newRot;
        this._normalSprite.y = this._y;

        if (this.surroundingLight) {
            this.surroundingLight.opacity = newAlpha;
        }

        if (newAlpha <= 0) {
            this._state = CharacterState.Dead;
            if (this.surroundingLight) this.surroundingLight.enabled = false;
        }
    }

    /** Snaps character to current position and resets movement (used by Enemy after killing player). */
    protected _stopCurrentMovement(): void {
        this._lerpAmount = 1;
        this._startX = this._x;
        this._startY = this._y;
        this._nextX = this._x;
        this._nextY = this._y;
        this._elapsedMS = 0;
    }

    /** Plays the idle animation for a given direction action (used by Enemy.attack and ChargeDirectionAI). */
    setDirectionAnimation(action: CharacterActions): void {
        switch (action) {
            case CharacterActions.Up:    this._playIdleAnimation(CharacterState.IdleUp);    break;
            case CharacterActions.Down:  this._playIdleAnimation(CharacterState.IdleDown);  break;
            case CharacterActions.Left:  this._playIdleAnimation(CharacterState.IdleLeft);  break;
            case CharacterActions.Right: this._playIdleAnimation(CharacterState.IdleRight); break;
            case CharacterActions.IdleUp:    this._playIdleAnimation(CharacterState.IdleUp);    break;
            case CharacterActions.IdleDown:  this._playIdleAnimation(CharacterState.IdleDown);  break;
            case CharacterActions.IdleLeft:  this._playIdleAnimation(CharacterState.IdleLeft);  break;
            case CharacterActions.IdleRight: this._playIdleAnimation(CharacterState.IdleRight); break;
        }
    }

    destroy(): void {
        this._objectsContainer.removeChild(this._mainSprite);
        this._objectsNormalContainer.removeChild(this._normalSprite);
        this._mainSprite.destroy();
        this._normalSprite.destroy();
        if (this._shadowSprite && this._shadowContainer) {
            this._shadowContainer.removeChild(this._shadowSprite);
            this._shadowSprite.destroy();
        }
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------

    private _handleBufferedInput(): void {
        const action = this._nextAction;

        if (this.isMoving) {
            // Support direction reversal while moving
            this._handleReversal(action);
            return;
        }

        const cell = this.currentCell;
        if (!cell) return;

        switch (action) {
            case CharacterActions.Up:    this._tryMove(cell, LinkDirection.NorthLink, CharacterState.GoingUp); break;
            case CharacterActions.Down:  this._tryMove(cell, LinkDirection.SouthLink, CharacterState.GoingDown); break;
            case CharacterActions.Right: this._tryMove(cell, LinkDirection.EastLink, CharacterState.GoingRight); break;
            case CharacterActions.Left:  this._tryMove(cell, LinkDirection.WestLink, CharacterState.GoingLeft); break;
            case CharacterActions.IdleUp:    this._setIdle(CharacterState.IdleUp); break;
            case CharacterActions.IdleDown:  this._setIdle(CharacterState.IdleDown); break;
            case CharacterActions.IdleLeft:  this._setIdle(CharacterState.IdleLeft); break;
            case CharacterActions.IdleRight: this._setIdle(CharacterState.IdleRight); break;
            default:
                // Idle / unrecognized — maintain current idle state
                if (!this._isMovingState(this._state)) {
                    // already idle, no change needed
                }
                break;
        }
    }

    private _tryMove(fromCell: GameCell, dir: LinkDirection, movingState: CharacterState): void {
        let nextCell: GameCell | null;

        if (this._canGoOverWalls) {
            // Ghost: bypass wall links, navigate raw grid
            const [dc, dr] = this._dirDelta(dir);
            nextCell = this._grid.getCell(fromCell.col + dc, fromCell.row + dr);
        } else {
            nextCell = fromCell.links.get(dir) ?? null;
        }

        if (!nextCell) return;

        this._startX = this._x;
        this._startY = this._y;
        this._nextX = nextCell.centerX;
        this._nextY = nextCell.centerY;
        this._lerpAmount = 0;
        this._elapsedMS = 0;
        this._state = movingState;

        this._playWalkAnimation(movingState);
    }

    private _handleReversal(action: CharacterActions): void {
        const reverse = this._reverseOf(action);
        if (reverse !== this._state) return;

        // Swap endpoints and mirror elapsed time
        const tmpX = this._startX;
        const tmpY = this._startY;
        this._startX = this._nextX;
        this._startY = this._nextY;
        this._nextX = tmpX;
        this._nextY = tmpY;
        this._elapsedMS = this._movementSpeedMS - this._elapsedMS;
        this._lerpAmount = 1 - this._lerpAmount;

        const newState = this._stateForAction(action);
        if (newState !== null) {
            this._state = newState;
            this._playWalkAnimation(newState);
        }
    }

    private _advanceLerp(deltaMS: number): void {
        if (this._lerpAmount >= 1) return;
        this._elapsedMS += deltaMS;
        this._lerpAmount = Math.min(this._elapsedMS / this._movementSpeedMS, 1);
        this._x = lerp(this._startX, this._nextX, this._lerpAmount);
        this._y = lerp(this._startY, this._nextY, this._lerpAmount);

        if (this._lerpAmount >= 1) {
            this._x = this._nextX;
            this._y = this._nextY;
            // Transition to idle when arrived
            this._state = this._idleStateFor(this._state);
            this._playIdleAnimation(this._state);
        }
    }

    // -------------------------------------------------------------------------
    // Animation helpers
    // -------------------------------------------------------------------------

    private _playWalkAnimation(state: CharacterState): void {
        if (this.dontPlayDefaultAnimations) return;
        switch (state) {
            case CharacterState.GoingUp:
                this._animations.play('WalkUp');   this._setFlip(false);
                this._setShadowWalk('WalkUp', false, this._shadowOffsets?.up ?? null);
                break;
            case CharacterState.GoingDown:
                this._animations.play('WalkDown'); this._setFlip(false);
                this._setShadowWalk('WalkDown', false, this._shadowOffsets?.down ?? null);
                break;
            case CharacterState.GoingRight:
                this._animations.play('WalkSide'); this._setFlip(false);
                this._setShadowWalk('WalkSide', false, this._shadowOffsets?.right ?? null);
                break;
            case CharacterState.GoingLeft:
                this._animations.play('WalkSide'); this._setFlip(true);
                this._setShadowWalk('WalkSide', true, this._shadowOffsets?.left ?? null);
                break;
        }
    }

    private _playIdleAnimation(state: CharacterState): void {
        if (this.dontPlayDefaultAnimations) return;
        switch (state) {
            case CharacterState.IdleUp:
                this._animations.play('IdleUp');   this._setFlip(false);
                this._setShadowIdle('IdleUp', false, this._shadowOffsets?.up ?? null);
                break;
            case CharacterState.IdleDown:
                this._animations.play('IdleDown'); this._setFlip(false);
                this._setShadowIdle('IdleDown', false, this._shadowOffsets?.downIdle ?? null);
                break;
            case CharacterState.IdleRight:
                this._animations.play('IdleSide'); this._setFlip(false);
                this._setShadowIdle('IdleSide', false, this._shadowOffsets?.right ?? null);
                break;
            case CharacterState.IdleLeft:
                this._animations.play('IdleSide'); this._setFlip(true);
                this._setShadowIdle('IdleSide', true, this._shadowOffsets?.left ?? null);
                break;
        }
    }

    private _setShadowWalk(animName: string, flipped: boolean, offset: { x: number; y: number } | null): void {
        if (!this._shadowSprite || !this._shadowAnimations || !offset) return;
        this._shadowAnimations.play(animName);
        this._shadowSprite.scale.x = flipped ? -1 : 1;
        this._currentShadowOffset = offset;
        this._shadowSprite.x = this._x + offset.x;
        this._shadowSprite.y = this._y + offset.y;
    }

    private _setShadowIdle(animName: string, flipped: boolean, offset: { x: number; y: number } | null): void {
        if (!this._shadowSprite || !this._shadowAnimations || !offset) return;
        this._shadowAnimations.play(animName);
        this._shadowSprite.scale.x = flipped ? -1 : 1;
        this._currentShadowOffset = offset;
        this._shadowSprite.x = this._x + offset.x;
        this._shadowSprite.y = this._y + offset.y;
    }

    private _setFlip(flipped: boolean): void {
        this._mainSprite.scale.x = flipped ? -1 : 1;
        this._normalSprite.scale.x = flipped ? -1 : 1;
    }

    private _setIdle(idleState: CharacterState): void {
        if (this._state === idleState) return;
        this._state = idleState;
        this._playIdleAnimation(idleState);
    }

    // -------------------------------------------------------------------------
    // Sprite sync & zIndex
    // -------------------------------------------------------------------------

    private _syncSprites(): void {
        const tex = this._animations.currentFrameTexture;
        this._mainSprite.texture = tex;
        this._mainSprite.x = this._x;
        this._mainSprite.y = this._y;

        const normalTex = this._animations.currentNormalFrameTexture;
        if (normalTex) {
            this._normalSprite.texture = normalTex;
            this._normalSprite.visible = true;
        } else {
            this._normalSprite.visible = false;
        }
        this._normalSprite.x = this._x;
        this._normalSprite.y = this._y;

        if (this._shadowSprite && this._shadowAnimations) {
            this._shadowSprite.texture = this._shadowAnimations.currentFrameTexture;
            // Update position every frame so the shadow follows during cell-to-cell lerp
            if (this._currentShadowOffset) {
                this._shadowSprite.x = this._x + this._currentShadowOffset.x;
                this._shadowSprite.y = this._y + this._currentShadowOffset.y;
            }
        }
    }

    private _recalculateZIndex(): void {
        const z = zForCharacter(this._y);
        this._mainSprite.zIndex = z;
        this._normalSprite.zIndex = z;
    }

    // -------------------------------------------------------------------------
    // Torch
    // -------------------------------------------------------------------------

    private _updateLightPosition(): void {
        if (!this.surroundingLight || !this._torchOffsets) return;
        const offset = this._torchOffsetForState();
        const targetX = this._x + offset.x;
        const targetY = this._y + offset.y;
        this.surroundingLight.position.x = lerp(this.surroundingLight.position.x, targetX, 0.2);
        this.surroundingLight.position.y = lerp(this.surroundingLight.position.y, targetY, 0.2);
    }

    private _torchOffsetForState(): { x: number; y: number } {
        if (!this._torchOffsets) return { x: 0, y: 0 };
        switch (this._state) {
            case CharacterState.GoingUp:
            case CharacterState.IdleUp:    return this._torchOffsets.up;
            case CharacterState.GoingLeft:
            case CharacterState.IdleLeft:  return this._torchOffsets.left;
            case CharacterState.GoingRight:
            case CharacterState.IdleRight: return this._torchOffsets.right;
            default:                       return this._torchOffsets.down;
        }
    }

    // -------------------------------------------------------------------------
    // State helpers
    // -------------------------------------------------------------------------

    private _idleStateFor(movingState: CharacterState): CharacterState {
        switch (movingState) {
            case CharacterState.GoingUp:    return CharacterState.IdleUp;
            case CharacterState.GoingDown:  return CharacterState.IdleDown;
            case CharacterState.GoingLeft:  return CharacterState.IdleLeft;
            case CharacterState.GoingRight: return CharacterState.IdleRight;
            default:                        return CharacterState.IdleDown;
        }
    }

    private _isMovingState(state: CharacterState): boolean {
        return state === CharacterState.GoingUp
            || state === CharacterState.GoingDown
            || state === CharacterState.GoingLeft
            || state === CharacterState.GoingRight;
    }

    /** Returns the moving state for a given action, or null if not a direction. */
    private _stateForAction(action: CharacterActions): CharacterState | null {
        switch (action) {
            case CharacterActions.Up:    return CharacterState.GoingUp;
            case CharacterActions.Down:  return CharacterState.GoingDown;
            case CharacterActions.Left:  return CharacterState.GoingLeft;
            case CharacterActions.Right: return CharacterState.GoingRight;
            default:                     return null;
        }
    }

    /** Returns the CharacterState that is the reverse of the given action. */
    private _reverseOf(action: CharacterActions): CharacterState {
        switch (action) {
            case CharacterActions.Up:    return CharacterState.GoingDown;
            case CharacterActions.Down:  return CharacterState.GoingUp;
            case CharacterActions.Left:  return CharacterState.GoingRight;
            case CharacterActions.Right: return CharacterState.GoingLeft;
            default:                     return CharacterState.IdleDown;
        }
    }

    private _dirDelta(dir: LinkDirection): [number, number] {
        switch (dir) {
            case LinkDirection.NorthLink: return [0, -1];
            case LinkDirection.SouthLink: return [0, 1];
            case LinkDirection.EastLink:  return [1, 0];
            case LinkDirection.WestLink:  return [-1, 0];
        }
    }
}
