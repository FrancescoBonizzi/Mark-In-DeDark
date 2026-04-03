import { Container, Spritesheet } from 'pixi.js';
import { Enemy, EnemyConfig } from '../Enemy';
import { MarkLight } from '../../lighting/MarkLight';
import { GameGrid } from '../../grid/GameGrid';
import { Player } from '../Player';
import { CandlesManager } from '../../objects/CandlesManager';
import { PatrolAI } from '../../ai/PatrolAI';
import { GameCell } from '../../grid/GameCell';
import { buildEnemyAnimations } from './enemyAnimations';

const SPEED_S = 0.8;
const TORCH_OFFSETS = {
    up:    { x: 0, y: -10 },
    left:  { x: 0, y: -20 },
    right: { x: 0, y: -10 },
    down:  { x: 0, y: -10 },
};

export class Zombie extends Enemy {
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
            sheets, 'Zombie/zombie',
            { front: 4, back: 4, side: 4 },
            { front: 4, back: 4, side: 4 },
        );
        animations.play('IdleDown');

        const torch = new MarkLight({
            x: startX + TORCH_OFFSETS.down.x,
            y: startY + TORCH_OFFSETS.down.y,
            z: 31, // C# 46 scaled by 2/3 (zoom 8/15 vs 0.8)
            opacity: 0.5, radius: 300, castsShadows: false,
            color1: 0x8BEF5A, color2: 0x6DBC45, distanceSquared: 2_200,
        });

        const ai = new PatrolAI(waypoints, grid, candlesManager, player, false);

        const config: EnemyConfig = {
            startX, startY,
            movementSpeed: SPEED_S,
            animations,
            canGoOverWalls: false,
            torch: { light: torch, offsets: TORCH_OFFSETS },
            initialAI: ai,
            killsCandlesOnCollision: true,
            nCellsCandleRange: null,
            player,
            candlesManager,
        };
        super(grid, objectsContainer, objectsNormalContainer, config);
    }
}
