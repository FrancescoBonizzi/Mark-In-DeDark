import { Container, Spritesheet } from 'pixi.js';
import { Enemy, EnemyConfig } from '../Enemy';
import { GameGrid } from '../../grid/GameGrid';
import { Player } from '../Player';
import { CandlesManager } from '../../objects/CandlesManager';
import { PatrolAI } from '../../ai/PatrolAI';
import { KillCandleAI } from '../../ai/KillCandleAI';
import { GameCell } from '../../grid/GameCell';
import { buildEnemyAnimations } from './enemyAnimations';

const SPEED_S = 0.6;

export class Bubu extends Enemy {
    constructor(
        grid: GameGrid,
        sheets: Spritesheet[],
        objectsContainer: Container,
        objectsNormalContainer: Container,
        startX: number,
        startY: number,
        waypoints: GameCell[],
        player: Player,
        candlesManager: CandlesManager,
    ) {
        const animations = buildEnemyAnimations(
            sheets, 'Bubu/bubu',
            { front: 4, back: 4, side: 4 },
            { front: 5, back: 4, side: 5 },
        );
        animations.play('IdleDown');

        // Bubu has no torch light — moves in darkness
        const ai = new PatrolAI(waypoints, grid, candlesManager, player, true);

        const config: EnemyConfig = {
            startX, startY,
            movementSpeed: SPEED_S,
            animations,
            canGoOverWalls: false,
            // No torch
            initialAI: ai,
            killsCandlesOnCollision: true,
            nCellsCandleRange: 3,
            player,
            candlesManager,
            killCandleDurationMS: 500,
        };
        super(grid, objectsContainer, objectsNormalContainer, config);
    }

    /** Bubu kills candle with 500ms duration. */
    protected override killCandleLogic(grid: GameGrid): void {
        const cell = this.currentCell;
        if (!cell) return;
        const result = this._candlesManager.getNearestCandlePath(cell, 0, grid);
        if (result) {
            this.setNewAI(new KillCandleAI(result.candle, [], this.currentAI, 500));
        }
    }
}
