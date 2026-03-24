import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { GameCell } from '../grid/GameCell';
import type { Player } from '../characters/Player';

/**
 * Ghost patrol AI — same as PatrolAI but uses wall-ignoring pathfinding
 * (port of C# PatrolGhostAI).
 */
export class PatrolGhostAI implements IEnemyAI {
    private readonly _grid: GameGrid;
    private readonly _waypoints: GameCell[];
    private _waypointIndex: number;
    private _actions: CharacterActions[] = [];

    constructor(
        waypoints: GameCell[],
        grid: GameGrid,
        _player: Player,
    ) {
        this._waypoints = waypoints;
        this._grid = grid;
        this._waypointIndex = waypoints.length > 1 ? 1 : 0;
    }

    update(enemy: Enemy, _deltaMS: number): CharacterActions {
        if (this._actions.length > 0) {
            return this._actions.shift()!;
        }

        if (this._waypoints.length === 0) return CharacterActions.Idle;

        const destWaypoint = this._waypoints[this._waypointIndex]!;
        const fromCell = enemy.currentCell;
        if (fromCell) {
            this._actions = this._grid.findActionsToPathIgnoringWalls(fromCell, destWaypoint);
        }
        this._waypointIndex = (this._waypointIndex + 1) % this._waypoints.length;

        if (this._actions.length === 0) return CharacterActions.Idle;
        return this._actions.shift()!;
    }
}
