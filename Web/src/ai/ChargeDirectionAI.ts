import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { CandlesManager } from '../objects/CandlesManager';

const CHARGE_SPEED_MS = 100;       // 0.1s per cell
const START_DURATION_MS = 1000;    // Wind-up time
const STUN_DURATION_MS = 2000;     // Post-charge stun
const CHARGE_COLOR = 0x0044FF;     // Blue tint while charging

const enum ChargeStatus { Starting, Charging, Stunned }

/**
 * Taurus charge AI — 3 phases: wind-up → charge → stun (port of C# ChargeDirectionAI).
 */
export class ChargeDirectionAI implements IEnemyAI {
    private readonly _direction: CharacterActions;
    private readonly _grid: GameGrid;
    private readonly _candlesManager: CandlesManager;
    private readonly _nextAI: IEnemyAI;
    private _status = ChargeStatus.Starting;
    private _elapsed = 0;
    private readonly _originalSpeedMS: number;

    constructor(
        direction: CharacterActions,
        grid: GameGrid,
        candlesManager: CandlesManager,
        nextAI: IEnemyAI,
        enemy: Enemy,
    ) {
        this._direction = direction;
        this._grid = grid;
        this._candlesManager = candlesManager;
        this._nextAI = nextAI;
        this._originalSpeedMS = enemy.movementSpeedMS;

        enemy.movementSpeedMS = CHARGE_SPEED_MS;
        enemy.surroundingLight?.setTemporaryColor(CHARGE_COLOR);
        enemy.dontPlayDefaultAnimations = true;

        // Play charge wind-up animation
        enemy.playAnimationIfExists(this._chargeAnimName());
    }

    update(enemy: Enemy, deltaMS: number): CharacterActions {
        this._elapsed += deltaMS;

        switch (this._status) {
            case ChargeStatus.Starting: {
                if (this._elapsed >= START_DURATION_MS) {
                    this._elapsed = 0;
                    this._status = ChargeStatus.Charging;
                    enemy.playAnimationIfExists(this._runAnimName());
                }
                // Face the charge direction while winding up
                enemy.setDirectionAnimation(this._direction);
                return CharacterActions.Idle;
            }

            case ChargeStatus.Charging: {
                const cell = enemy.currentCell;
                if (cell) {
                    // Kill any candle on current cell
                    if (this._candlesManager.isThereACandle(cell)) {
                        for (const candle of this._candlesManager.candles) {
                            if (candle.isAlive &&
                                Math.abs(candle.collisionRectangle.x + 24 - cell.centerX) < 10 &&
                                Math.abs(candle.collisionRectangle.y + 24 - cell.centerY) < 10) {
                                candle.die(500);
                            }
                        }
                    }

                    // Check if passable
                    if (!this._grid.isPassableWithAction(cell, this._direction)) {
                        // Hit a wall — break it if breakable
                        this._grid.breakWall(cell, this._direction);
                        this._transitionToStunned(enemy);
                        return CharacterActions.Idle;
                    }
                }
                return this._direction;
            }

            case ChargeStatus.Stunned: {
                if (this._elapsed >= STUN_DURATION_MS) {
                    enemy.movementSpeedMS = this._originalSpeedMS;
                    enemy.surroundingLight?.resetColor();
                    enemy.dontPlayDefaultAnimations = false;
                    enemy.playAnimationIfExists('IdleDown');
                    return enemy.setNewAI(this._nextAI);
                }
                return CharacterActions.Idle;
            }
        }
    }

    get currentStatus(): ChargeStatus { return this._status; }

    private _transitionToStunned(enemy: Enemy): void {
        this._elapsed = 0;
        this._status = ChargeStatus.Stunned;
        enemy.dontPlayDefaultAnimations = false;
    }

    private _chargeAnimName(): string {
        switch (this._direction) {
            case CharacterActions.Up:    return 'TaurusChargeBack';
            case CharacterActions.Down:  return 'TaurusChargeFront';
            default:                     return 'TaurusChargeSide';
        }
    }

    private _runAnimName(): string {
        switch (this._direction) {
            case CharacterActions.Up:    return 'TaurusRunBack';
            case CharacterActions.Down:  return 'TaurusRunFront';
            default:                     return 'TaurusRunSide';
        }
    }
}
