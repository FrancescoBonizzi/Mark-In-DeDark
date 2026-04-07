import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { GameCell } from '../grid/GameCell';
import type { Player } from '../characters/Player';

const enum GhostPhase {
    Patrol,
    Hovering,
}

const HOVER_TRIGGER_DISTANCE = 6;
const HOVER_PROBABILITY = 0.25;
const HOVER_DURATION_MS = 500;
/** Minimum cells traversed between hover checks (cooldown). */
const HOVER_CELL_COOLDOWN = 5;

/**
 * Ghost patrol AI — same as PatrolAI but uses wall-ignoring pathfinding
 * (port of C# PatrolGhostAI).
 *
 * Aggiunta fase Hovering: se il Ghost è entro 4 celle dal player,
 * ha il 15% di probabilità ogni 60 frame di fermarsi 500ms ("annusare").
 */
export class PatrolGhostAI implements IEnemyAI {
    private readonly _grid: GameGrid;
    private readonly _waypoints: GameCell[];
    private readonly _player: Player;
    private _waypointIndex: number;
    private _actions: CharacterActions[] = [];
    private _phase: GhostPhase = GhostPhase.Patrol;
    private _hoverTimer: number = 0;
    private _cellCooldown: number = 0;

    constructor(
        waypoints: GameCell[],
        grid: GameGrid,
        player: Player,
    ) {
        this._waypoints = waypoints;
        this._grid = grid;
        this._player = player;
        this._waypointIndex = waypoints.length > 1 ? 1 : 0;
    }

    get isHovering(): boolean {
        return this._phase === GhostPhase.Hovering;
    }

    update(enemy: Enemy, deltaMS: number): CharacterActions {
        if (this._phase === GhostPhase.Hovering) {
            this._hoverTimer -= deltaMS;
            if (this._hoverTimer <= 0) {
                this._phase = GhostPhase.Patrol;
                this._cellCooldown = HOVER_CELL_COOLDOWN;
            }
            return CharacterActions.Idle;
        }

        // Controlla se passare a Hovering (una volta per cella attraversata)
        if (this._cellCooldown > 0) {
            this._cellCooldown--;
        } else {
            const enemyCell = enemy.currentCell;
            const playerCell = this._player.currentCell;
            if (enemyCell && playerCell) {
                const dist =
                    Math.abs(enemyCell.col - playerCell.col) +
                    Math.abs(enemyCell.row - playerCell.row);
                if (dist <= HOVER_TRIGGER_DISTANCE && Math.random() < HOVER_PROBABILITY) {
                    this._phase = GhostPhase.Hovering;
                    this._hoverTimer = HOVER_DURATION_MS;
                    this._actions = [];
                    return CharacterActions.Idle;
                }
            }
        }

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
