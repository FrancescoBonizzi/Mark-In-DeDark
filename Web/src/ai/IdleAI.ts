import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';

/** Does nothing — used after the enemy kills the player (port of C# IdleAI). */
export class IdleAI implements IEnemyAI {
    update(_enemy: Enemy, _deltaMS: number): CharacterActions {
        return CharacterActions.Idle;
    }
}
