import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { GameCell } from '../grid/GameCell';
import type { Player } from '../characters/Player';
import type { CandlesManager } from '../objects/CandlesManager';
import { KillCandleAI } from './KillCandleAI';
import { RandomMovementAI } from './RandomMovementAI';

/**
 * Continuously pathfinds toward the player, recalculating when the player changes cells.
 * Also checks for nearby candles before chasing (port of C# PlayerHunterAI).
 */
export class PlayerHunterAI implements IEnemyAI {
    private readonly _player: Player;
    private readonly _grid: GameGrid;
    private readonly _candlesManager: CandlesManager;
    private _lastPlayerCell: GameCell | null = null;
    private _actions: CharacterActions[] = [];

    constructor(player: Player, grid: GameGrid, candlesManager: CandlesManager) {
        this._player = player;
        this._grid = grid;
        this._candlesManager = candlesManager;
    }

    update(enemy: Enemy, deltaMS: number): CharacterActions {
        // Check candles first
        const cell = enemy.currentCell;
        if (cell && enemy.nCellsCandleRange !== null) {
            const result = this._candlesManager.getNearestCandlePath(cell, enemy.nCellsCandleRange, this._grid);
            if (result) {
                // C#: immediately invoke the new AI on the same frame (no one-tick delay)
                enemy.setNewAI(new KillCandleAI(result.candle, result.actions, this));
                return enemy.currentAI.update(enemy, deltaMS);
            }
        }

        // Determine player's current cell
        const destCell = this._grid.getCellAtPixel(
            this._player.worldCenter.x,
            this._player.worldCenter.y,
        );

        if (!destCell) return CharacterActions.Idle;

        // Recalculate path when player changes cells
        if (destCell !== this._lastPlayerCell || this._actions.length === 0) {
            this._lastPlayerCell = destCell;
            const fromCell = enemy.currentCell;
            this._actions = fromCell ? this._grid.findActionsToPath(fromCell, destCell) : [];
        }

        if (this._actions.length > 0) {
            return this._actions.shift()!;
        }

        return enemy.setNewAI(new RandomMovementAI(this));
    }
}
