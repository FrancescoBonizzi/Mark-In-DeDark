import { Container, Rectangle } from 'pixi.js';
import { Character, CharacterConfig } from './Character';
import { CharacterActions } from '../interaction/CharacterActions';
import { CharacterState } from './CharacterState';
import type { GameGrid } from '../grid/GameGrid';
import type { Player } from './Player';
import type { CandlesManager } from '../objects/CandlesManager';
import type { Candle } from '../objects/Candle';
import type { IEnemyAI } from '../ai/IEnemyAI';
import { IdleAI } from '../ai/IdleAI';
import { KillCandleAI } from '../ai/KillCandleAI';
import { ChargeDirectionAI } from '../ai/ChargeDirectionAI';

export interface EnemyConfig extends CharacterConfig {
    initialAI: IEnemyAI;
    killsCandlesOnCollision: boolean;
    nCellsCandleRange: number | null;
    player: Player;
    candlesManager: CandlesManager;
    /** Duration (ms) to kill a candle on collision (Ghost=250, Bubu=500, default=3000). */
    killCandleDurationMS?: number;
}

function intersects(a: Rectangle, b: Rectangle): boolean {
    return a.x < b.x + b.width &&
        a.x + a.width > b.x &&
        a.y < b.y + b.height &&
        a.y + a.height > b.y;
}

/**
 * Abstract base for all enemy characters (port of C# Enemy.cs).
 * Handles player collision, candle collision, and AI-driven movement.
 */
export abstract class Enemy extends Character {
    currentAI: IEnemyAI;
    readonly nCellsCandleRange: number | null;
    protected readonly _player: Player;
    protected readonly _candlesManager: CandlesManager;
    private readonly _killsCandlesOnCollision: boolean;
    private readonly _killCandleDurationMS: number;
    private _gameFinished = false;

    constructor(
        grid: GameGrid,
        objectsContainer: Container,
        objectsNormalContainer: Container,
        config: EnemyConfig,
    ) {
        super(grid, objectsContainer, objectsNormalContainer, config);
        this.currentAI = config.initialAI;
        this.nCellsCandleRange = config.nCellsCandleRange;
        this._player = config.player;
        this._candlesManager = config.candlesManager;
        this._killsCandlesOnCollision = config.killsCandlesOnCollision;
        this._killCandleDurationMS = config.killCandleDurationMS ?? 3000;
    }

    /** Switch AI and return Idle (used by AI classes to chain to the next AI). */
    setNewAI(ai: IEnemyAI): CharacterActions {
        this.currentAI = ai;
        return CharacterActions.Idle;
    }

    /** Plays the attack animation toward the player's relative position. */
    attack(player?: Player): void {
        this._state = CharacterState.Attacking;
        const target = player ?? this._player;
        const dx = target.worldCenter.x - this._x;
        const dy = target.worldCenter.y - this._y;

        // Determine primary attack direction
        if (Math.abs(dy) >= Math.abs(dx)) {
            if (dy <= 0) {
                this._animations.play('AttackUp');
                this._mainSprite.scale.x = 1;
                this._normalSprite.scale.x = 1;
            } else {
                this._animations.play('AttackDown');
                this._mainSprite.scale.x = 1;
                this._normalSprite.scale.x = 1;
            }
        } else {
            if (dx < 0) {
                this._animations.play('AttackSide');
                this._mainSprite.scale.x = -1;
                this._normalSprite.scale.x = -1;
            } else {
                this._animations.play('AttackSide');
                this._mainSprite.scale.x = 1;
                this._normalSprite.scale.x = 1;
            }
        }
    }

    /** Plays a named animation if it exists (used by ChargeDirectionAI for Taurus-only anims). */
    playAnimationIfExists(name: string): void {
        if (this._animations.has(name)) {
            this._animations.forcePlay(name);
        }
    }

    get isAttacking(): boolean {
        return this._state === CharacterState.Attacking;
    }

    /** Returns to idle animation after smashing. */
    stopAttacking(): void {
        if (this._state === CharacterState.Attacking) {
            this._state = CharacterState.IdleDown;
            this._animations.play('IdleDown');
        }
    }

    // -------------------------------------------------------------------------
    // Override hooks for subclasses
    // -------------------------------------------------------------------------

    /** Default: enemy always tries to attack the player. Override in Ghost. */
    protected customConditionToAttackPlayer(): boolean {
        return true;
    }

    /** Called when the enemy kills the player. Override for special effects. */
    protected customActionOnPlayerDead(): void {
        // no-op by default
    }

    /**
     * Default candle kill logic: switch to KillCandleAI with the candle already in hand.
     * C# Enemy.KillCandleLogic passes the candle directly — no extra pathfinding needed
     * since collision detection already confirmed the enemy is on the candle's cell.
     */
    protected killCandleLogic(candle: Candle): void {
        this.setNewAI(new KillCandleAI(candle, [], this.currentAI, this._killCandleDurationMS));
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    override update(deltaMS: number): void {
        if (this._gameFinished || this.isDead) return;

        // 1. Candle collision (if configured and not in a charge or kill-candle state)
        if (this._killsCandlesOnCollision &&
            !(this.currentAI instanceof ChargeDirectionAI) &&
            !(this.currentAI instanceof KillCandleAI)) {
            const cell = this.currentCell;
            if (cell) {
                const candle = this._candlesManager.getCandleAtCell(cell, this._grid);
                if (candle) {
                    this.killCandleLogic(candle);
                }
            }
        }

        // 2. Player collision — kill player
        if (!this._player.isDead &&
            !this._player.isDying &&
            !this.isDying &&
            this.customConditionToAttackPlayer() &&
            intersects(this.collisionRectangle, this._player.collisionRectangle)) {
            this.customActionOnPlayerDead();
            this.attack(this._player);
            this._player.die();
            this.currentAI = new IdleAI();
            this._gameFinished = true;
            this._stopCurrentMovement();
        }

        // 3. Wait for attack animation to finish, then return to idle.
        //    C#: AI keeps running during Attacking (e.g. KillCandleAI.Smashing polls candle.isAlive).
        if (this._state === CharacterState.Attacking) {
            if (this._animations.currentAnimation?.isFinished) {
                this.stopAttacking();
            }
            // AI still runs for state transitions, but movement input is ignored
            if (!this.isMoving && !this.isDying) {
                this.currentAI.update(this, deltaMS);
            }
            super.update(deltaMS);
            return;
        }

        // 4. AI-driven movement
        if (!this.isMoving && !this.isDying) {
            const action = this.currentAI.update(this, deltaMS);
            this.handleInput(action);
        }

        // 5. Base Character update (lerp, sprites, torch)
        super.update(deltaMS);
    }
}
