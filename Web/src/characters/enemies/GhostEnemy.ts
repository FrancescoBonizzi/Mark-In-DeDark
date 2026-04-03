import { Container, Spritesheet } from 'pixi.js';
import { Enemy, EnemyConfig } from '../Enemy';
import { MarkLight } from '../../lighting/MarkLight';
import { GameGrid } from '../../grid/GameGrid';
import { Player } from '../Player';
import { CandlesManager } from '../../objects/CandlesManager';
import type { Candle } from '../../objects/Candle';
import { PatrolGhostAI } from '../../ai/PatrolGhostAI';
import { GameCell } from '../../grid/GameCell';
import { buildEnemyAnimations } from './enemyAnimations';

const SPEED_S = 1.0;
const TORCH_OFFSETS = {
    up:    { x: 0, y: 0 },
    left:  { x: 0, y: 0 },
    right: { x: 0, y: 0 },
    down:  { x: 0, y: 0 },
};

export class GhostEnemy extends Enemy {
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
            sheets, 'Ghost/ghost',
            { front: 5, back: 5, side: 5 },
            { front: 8, back: 8, side: 8 },
        );
        animations.play('IdleDown');

        const torch = new MarkLight({
            x: startX,
            y: startY,
            z: 31, // C# 46 scaled by 2/3 (zoom 8/15 vs 0.8)
            opacity: 1.0, radius: 300, castsShadows: false,
            color1: 0x9CEEC0, color2: 0xEFC0E3, distanceSquared: 2_200,
        });
        torch.deltaForWall = { x: 0, y: -30 };

        const ai = new PatrolGhostAI(waypoints, grid, player);

        const config: EnemyConfig = {
            startX, startY,
            movementSpeed: SPEED_S,
            animations,
            canGoOverWalls: true,
            torch: { light: torch, offsets: TORCH_OFFSETS },
            initialAI: ai,
            killsCandlesOnCollision: true,
            nCellsCandleRange: null,
            player,
            candlesManager,
            killCandleDurationMS: 250,
        };
        super(grid, objectsContainer, objectsNormalContainer, config);
    }

    /** Ghost cannot die. */
    override die(): void {
        // no-op
    }

    /** Ghost only attacks moving players. */
    protected override customConditionToAttackPlayer(): boolean {
        return this._player.isMoving;
    }

    /** Ghost instantly kills candle (250ms) without walking. C#: Ghost.KillCandleLogic calls possibleCandle.Die directly. */
    protected override killCandleLogic(candle: Candle): void {
        candle.die(250);
    }
}
