import { Container, Spritesheet } from 'pixi.js';
import { Enemy, EnemyConfig } from '../Enemy';
import { MarkLight } from '../../lighting/MarkLight';
import { GameGrid } from '../../grid/GameGrid';
import { Player } from '../Player';
import { CandlesManager } from '../../objects/CandlesManager';
import { PatrolHunterLineOfSightAI } from '../../ai/PatrolHunterLineOfSightAI';
import { GameCell } from '../../grid/GameCell';
import { buildEnemyAnimations } from './enemyAnimations';

const SPEED_S = 0.6;
const TORCH_OFFSETS = {
    up:    { x: -45, y: -15 },
    left:  { x: -30, y: -5 },
    right: { x:  30, y: -5 },
    down:  { x:  35, y:  0 },
};

export class Skeleton extends Enemy {
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
            sheets, 'Skeleton/skeleton',
            { front: 4, back: 4, side: 4 },
            { front: 4, back: 4, side: 4 },
        );
        animations.play('IdleDown');

        const torch = new MarkLight({
            x: startX + TORCH_OFFSETS.down.x,
            y: startY + TORCH_OFFSETS.down.y,
            z: 31, // C# 46 × (2/3) = 30.7 ≈ 31
            opacity: 0.5, radius: 300, castsShadows: false,
            color1: 0xC476F0, color2: 0x9159B2, distanceSquared: 2_200, // C# 4_950 × (2/3)²
        });

        const ai = new PatrolHunterLineOfSightAI(waypoints, grid, candlesManager, player);

        const config: EnemyConfig = {
            startX, startY,
            movementSpeed: SPEED_S,
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
    }
}
