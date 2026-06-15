import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { CandlesManager } from '../objects/CandlesManager';
import { SoundManagerInstance } from '../services/SoundInstance';
import { AssetsRoot } from '../assets/constants';

const CHARGE_SPEED_MS = 100;       // 0.1s per cell
const START_DURATION_MS = 1000;    // Wind-up time
const STUN_DURATION_MS = 2000;     // Post-charge stun
const CHARGE_COLOR = 0x0066FF;     // Bright blue tint while charging
const CHARGE_COLOR_DARK = 0x000033; // Dark blue for pulse contrast

const enum ChargeStatus { Starting, Charging, Stunned }

/**
 * Taurus charge AI — 3 phases: wind-up → charge → stun (port of C# ChargeDirectionAI).
 */
export class ChargeDirectionAI implements IEnemyAI {
    private readonly _direction: CharacterActions;
    private readonly _grid: GameGrid;
    private readonly _candlesManager: CandlesManager;
    private readonly _nextAI: IEnemyAI;
    private readonly _onShake: ((intensity: number) => void) | undefined;
    private _status = ChargeStatus.Starting;
    private _elapsed = 0;
    private readonly _originalSpeedMS: number;
    private readonly _originalColor1: number | undefined;
    private readonly _originalOpacity: number | undefined;

    constructor(
        direction: CharacterActions,
        grid: GameGrid,
        candlesManager: CandlesManager,
        nextAI: IEnemyAI,
        enemy: Enemy,
        onShake?: (intensity: number) => void,
    ) {
        this._direction = direction;
        this._grid = grid;
        this._candlesManager = candlesManager;
        this._nextAI = nextAI;
        this._onShake = onShake;
        this._originalSpeedMS = enemy.movementSpeedMS;
        this._originalColor1 = enemy.surroundingLight?.color1;
        this._originalOpacity = enemy.surroundingLight?.opacity;

        enemy.movementSpeedMS = CHARGE_SPEED_MS;
        if (enemy.surroundingLight) {
            enemy.surroundingLight.color1 = CHARGE_COLOR_DARK;
            enemy.surroundingLight.setTemporaryColor(CHARGE_COLOR);
        }
        enemy.dontPlayDefaultAnimations = true;

        // Play charge wind-up animation
        enemy.playAnimationIfExists(this._chargeAnimName());
        SoundManagerInstance.play('taurusCharge', `${AssetsRoot}sounds/effects/TaurusStartingCharge.wav`, { volume: 0.6 });
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
                // Pulse opacity fast (≈4 Hz) to make wind-up visually aggressive
                if (enemy.surroundingLight) {
                    const t = (Math.sin(this._elapsed * 0.025) + 1) / 2;
                    enemy.surroundingLight.opacity = 0.25 + t * 0.55;
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
                        this._onShake?.(15);
                        this._transitionToStunned(enemy);
                        return CharacterActions.Idle;
                    }
                }
                this._onShake?.(5);
                return this._direction;
            }

            case ChargeStatus.Stunned: {
                if (this._elapsed >= STUN_DURATION_MS) {
                    enemy.movementSpeedMS = this._originalSpeedMS;
                    if (enemy.surroundingLight) {
                        if (this._originalColor1 !== undefined) {
                            enemy.surroundingLight.color1 = this._originalColor1;
                        }
                        if (this._originalOpacity !== undefined) {
                            enemy.surroundingLight.opacity = this._originalOpacity;
                        }
                        enemy.surroundingLight.resetColor();
                    }
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
