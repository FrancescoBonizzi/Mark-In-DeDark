import { Container, Spritesheet } from 'pixi.js';
import { Enemy, EnemyConfig } from '../Enemy';
import { MarkLight } from '../../lighting/MarkLight';
import { GameGrid } from '../../grid/GameGrid';
import { Player } from '../Player';
import { CandlesManager } from '../../objects/CandlesManager';
import { PlayerHunterAI } from '../../ai/PlayerHunterAI';
import { buildEnemyAnimations } from './enemyAnimations';

const BASE_SPEED_S = 0.8;
const SPEED_DECREMENT = 0.1; // Gets faster with each candle placed
const MIN_SPEED_S = 0.2;

const TORCH_OFFSETS = {
    up:    { x: -35, y: -10 },
    left:  { x:   5, y:   5 },
    right: { x:  -5, y:   5 },
    down:  { x:  30, y: -15 },
};

export class Goblin extends Enemy {
    private _lastUsedCandleCount = 0;

    constructor(
        grid: GameGrid,
        sheets: Spritesheet[],
        objectsContainer: Container,
        objectsNormalContainer: Container,
        startX: number,
        startY: number,
        player: Player,
        candlesManager: CandlesManager,
    ) {
        const animations = buildEnemyAnimations(
            sheets, 'Goblin/goblin',
            { front: 4, back: 4, side: 4 },
            { front: 5, back: 5, side: 5 },
        );
        animations.play('IdleDown');

        const torch = new MarkLight({
            x: startX + TORCH_OFFSETS.down.x,
            y: startY + TORCH_OFFSETS.down.y,
            z: 31, // C# 46 scaled by 2/3 (zoom 8/15 vs 0.8)
            opacity: 0.5, radius: 300, castsShadows: false,
            color1: 0xCAF076, color2: 0xC3E27C, distanceSquared: 2_200,
        });

        const ai = new PlayerHunterAI(player, grid, candlesManager);

        const config: EnemyConfig = {
            startX, startY,
            movementSpeed: BASE_SPEED_S,
            animations,
            canGoOverWalls: false,
            torch: { light: torch, offsets: TORCH_OFFSETS },
            initialAI: ai,
            killsCandlesOnCollision: true,
            nCellsCandleRange: 3,
            player,
            candlesManager,
        };
        super(grid, objectsContainer, objectsNormalContainer, config);
        this._lastUsedCandleCount = candlesManager.playerUsedCandles;
    }

    override update(deltaMS: number): void {
        // Speed increases each time the player places a candle
        const currentUsed = this._candlesManager.playerUsedCandles;
        if (currentUsed > this._lastUsedCandleCount) {
            const increments = currentUsed - this._lastUsedCandleCount;
            this._lastUsedCandleCount = currentUsed;
            const newSpeed = Math.max(MIN_SPEED_S, this.movementSpeedMS / 1000 - SPEED_DECREMENT * increments);
            this.movementSpeedMS = newSpeed * 1000;
        }
        super.update(deltaMS);
    }
}
