import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';

const DIRECTIONS: CharacterActions[] = [
    CharacterActions.Up,
    CharacterActions.Down,
    CharacterActions.Left,
    CharacterActions.Right,
];

const PHASE_DURATION_MS = 500;

/**
 * Fallback AI: alternates between an attack phase and a random-move phase.
 * In the move phase it first tries to restore the previous AI; if that returns
 * Idle it picks a random direction (port of C# RandomMovementAI).
 */
export class RandomMovementAI implements IEnemyAI {
    private readonly _previousAI: IEnemyAI;
    private _isAttackPhase = true;
    private _elapsed = 0;

    constructor(previousAI: IEnemyAI) {
        this._previousAI = previousAI;
    }

    update(enemy: Enemy, deltaMS: number): CharacterActions {
        this._elapsed += deltaMS;

        if (this._isAttackPhase) {
            enemy.attack();
            if (this._elapsed >= PHASE_DURATION_MS) {
                this._elapsed = 0;
                this._isAttackPhase = false;
            }
            return CharacterActions.Idle;
        }

        // Move phase — first try restoring previous AI
        const prevAction = this._previousAI.update(enemy, deltaMS);
        if (prevAction !== CharacterActions.Idle) {
            enemy.setNewAI(this._previousAI);
            return prevAction;
        }

        const dir = DIRECTIONS[Math.floor(Math.random() * DIRECTIONS.length)]!;
        this._elapsed = 0;
        this._isAttackPhase = true;
        return dir;
    }
}
