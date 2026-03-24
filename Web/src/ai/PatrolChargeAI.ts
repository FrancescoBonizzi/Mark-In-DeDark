import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { GameCell } from '../grid/GameCell';
import type { Player } from '../characters/Player';
import type { CandlesManager } from '../objects/CandlesManager';
import { PatrolAI } from './PatrolAI';
import { ChargeDirectionAI } from './ChargeDirectionAI';
import { checkLineOfSight } from './LineOfSight';

/**
 * Taurus AI — patrols then charges when it sees the player or a candle in LoS
 * (port of C# PatrolChargeAI).
 */
export class PatrolChargeAI extends PatrolAI implements IEnemyAI {
    private readonly _playerRef: Player;
    private readonly _gridRef: GameGrid;
    private readonly _candlesRef: CandlesManager;

    constructor(
        waypoints: GameCell[],
        grid: GameGrid,
        candlesManager: CandlesManager,
        player: Player,
    ) {
        // searchesCandles=false — Taurus charges at them instead
        super(waypoints, grid, candlesManager, player, false);
        this._playerRef = player;
        this._gridRef = grid;
        this._candlesRef = candlesManager;
    }

    override update(enemy: Enemy, deltaMS: number): CharacterActions {
        // Check LoS toward player
        const playerLoS = checkLineOfSight(
            enemy,
            this._playerRef.worldCenter.x,
            this._playerRef.worldCenter.y,
            this._gridRef,
        );
        if (playerLoS !== null && playerLoS.length > 0) {
            return enemy.setNewAI(
                new ChargeDirectionAI(playerLoS[0]!, this._gridRef, this._candlesRef, this, enemy),
            );
        }

        // Check LoS toward each candle
        for (const candle of this._candlesRef.candles) {
            if (!candle.isAlive) continue;
            const cx = candle.collisionRectangle.x + candle.collisionRectangle.width / 2;
            const cy = candle.collisionRectangle.y + candle.collisionRectangle.height / 2;
            const candleLoS = checkLineOfSight(enemy, cx, cy, this._gridRef);
            if (candleLoS !== null && candleLoS.length > 0) {
                return enemy.setNewAI(
                    new ChargeDirectionAI(candleLoS[0]!, this._gridRef, this._candlesRef, this, enemy),
                );
            }
        }

        return super.update(enemy, deltaMS);
    }
}
