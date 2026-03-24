import { CharacterActions } from '../interaction/CharacterActions';
import type { Enemy } from '../characters/Enemy';

/** Strategy interface for all enemy AI behaviours (port of C# IEnemyAI). */
export interface IEnemyAI {
    update(enemy: Enemy, deltaMS: number): CharacterActions;
}
