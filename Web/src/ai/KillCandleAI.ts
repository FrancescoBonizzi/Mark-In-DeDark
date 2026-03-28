import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { Candle } from '../objects/Candle';

const enum KillCandleStatus { WalkingToCandle, Smashing, Ended }

/**
 * Walks toward a target candle, extinguishes it, then returns to the previous AI
 * (port of C# KillCandleAI).
 */
export class KillCandleAI implements IEnemyAI {
    private readonly _candle: Candle;
    private readonly _nextAI: IEnemyAI;
    private _actions: CharacterActions[];
    private _status = KillCandleStatus.WalkingToCandle;
    /** Optional die duration override in ms (Ghost=250, Bubu=500, default=3000). */
    private readonly _killDurationMS: number;

    constructor(
        candle: Candle,
        actions: CharacterActions[],
        nextAI: IEnemyAI,
        killDurationMS = 3000,
    ) {
        this._candle = candle;
        this._actions = [...actions];
        this._nextAI = nextAI;
        this._killDurationMS = killDurationMS;
        candle.isBeingFollowed = true;
    }

    update(enemy: Enemy, _deltaMS: number): CharacterActions {
        switch (this._status) {
            case KillCandleStatus.WalkingToCandle: {
                // If candle already died (killed by another enemy) abort
                if (!this._candle.isAlive) {
                    return enemy.setNewAI(this._nextAI);
                }
                if (this._actions.length > 0) {
                    return this._actions.shift()!;
                }
                // Arrived — kill the candle
                this._candle.die(this._killDurationMS);
                enemy.attack();
                this._status = KillCandleStatus.Smashing;
                return CharacterActions.Idle;
            }
            case KillCandleStatus.Smashing: {
                if (!this._candle.isAlive) {
                    enemy.stopAttacking();
                    this._status = KillCandleStatus.Ended;
                } else if (!enemy.isAttacking) {
                    // Previous attack animation finished but candle still alive — loop it
                    enemy.attack();
                }
                return CharacterActions.Idle;
            }
            case KillCandleStatus.Ended:
                return enemy.setNewAI(this._nextAI);
        }
    }
}
