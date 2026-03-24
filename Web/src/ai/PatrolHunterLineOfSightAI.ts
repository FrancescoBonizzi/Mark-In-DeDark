import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { Player } from '../characters/Player';
import type { CandlesManager } from '../objects/CandlesManager';
import { PatrolAI } from './PatrolAI';
import { PlayerHunterLineOfSightAI } from './PlayerHunterLineOfSightAI';
import type { GameCell } from '../grid/GameCell';
import { checkLineOfSight } from './LineOfSight';

/**
 * Patrols normally; switches to PlayerHunterLineOfSightAI when it spots the player
 * in a straight line (port of C# PatrolHunterLineOfSightAI — Skeleton AI).
 */
export class PatrolHunterLineOfSightAI extends PatrolAI implements IEnemyAI {
    private readonly _playerRef: Player;
    private readonly _gridRef: GameGrid;
    private readonly _candlesRef: CandlesManager;

    constructor(
        waypoints: GameCell[],
        grid: GameGrid,
        candlesManager: CandlesManager,
        player: Player,
    ) {
        super(waypoints, grid, candlesManager, player, true);
        this._playerRef = player;
        this._gridRef = grid;
        this._candlesRef = candlesManager;
    }

    override update(enemy: Enemy, deltaMS: number): CharacterActions {
        // Check LoS to player
        const losPath = checkLineOfSight(
            enemy,
            this._playerRef.worldCenter.x,
            this._playerRef.worldCenter.y,
            this._gridRef,
        );

        if (losPath !== null) {
            const hunterAI = new PlayerHunterLineOfSightAI(
                enemy, this, losPath, this._gridRef, this._playerRef, this._candlesRef,
            );
            return enemy.setNewAI(hunterAI);
        }

        return super.update(enemy, deltaMS);
    }
}
