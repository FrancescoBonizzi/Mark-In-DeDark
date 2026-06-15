import { CharacterActions } from '../interaction/CharacterActions';
import { CharacterState } from '../characters/CharacterState';
import { GameGrid } from '../grid/GameGrid';
import type { Enemy } from '../characters/Enemy';

/**
 * Shared line-of-sight algorithm (port of C# PatrolHunterLineOfSightAI.PathToPositionIfFacingAndSameDirection).
 *
 * Returns the CharacterActions[] path if the enemy has a clear straight-line view of the target,
 * or null if the target is out of sight.
 *
 * LoS conditions:
 * 1. Enemy and target are on the same row OR same column.
 * 2. Enemy is not facing away from the target.
 * 3. The BFS path from enemy to target contains only one distinct direction (no turns = no walls).
 */
export function checkLineOfSight(
    enemy: Enemy,
    targetX: number,
    targetY: number,
    grid: GameGrid,
): CharacterActions[] | null {
    const enemyCell = enemy.currentCell;
    if (!enemyCell) return null;

    const targetCell = grid.getCellAtPixel(targetX, targetY);
    if (!targetCell) return null;

    // 1. Same row or same column
    const sameRow = enemyCell.row === targetCell.row;
    const sameCol = enemyCell.col === targetCell.col;
    if (!sameRow && !sameCol) return null;

    // 2. Enemy must not be facing away from target
    const st = enemy.state;
    if (sameCol) {
        const targetIsAbove = targetCell.row < enemyCell.row;
        // "above" in grid = north (lower row index). Facing up = GoingUp / IdleUp.
        if (targetIsAbove && (st === CharacterState.GoingUp || st === CharacterState.IdleUp)) return null;
        if (!targetIsAbove && (st === CharacterState.GoingDown || st === CharacterState.IdleDown)) return null;
    } else {
        const targetIsLeft = targetCell.col < enemyCell.col;
        if (targetIsLeft && (st === CharacterState.GoingLeft || st === CharacterState.IdleLeft)) return null;
        if (!targetIsLeft && (st === CharacterState.GoingRight || st === CharacterState.IdleRight)) return null;
    }

    // 3. Path must be a straight line (all actions the same direction)
    const actions = grid.findActionsToPath(enemyCell, targetCell);
    if (actions.length === 0) return null;

    const first = actions[0];
    if (actions.every(a => a === first)) return actions;

    return null;
}
