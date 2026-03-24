import { Container, Spritesheet } from 'pixi.js';
import { Enemy, EnemyConfig } from '../Enemy';
import { MarkLight } from '../../lighting/MarkLight';
import { GameGrid } from '../../grid/GameGrid';
import { Player } from '../Player';
import { CandlesManager } from '../../objects/CandlesManager';
import { PatrolChargeAI } from '../../ai/PatrolChargeAI';
import { GameCell } from '../../grid/GameCell';
import { SpriteAnimation } from '../../animation/SpriteAnimation';
import { buildEnemyAnimations } from './enemyAnimations';
import { getTexture } from '../../assets/AssetsLoader';

const SPEED_S = 1.5;
const TORCH_OFFSETS = {
    up:    { x: 25, y: 10 },
    left:  { x:  0, y: 25 },
    right: { x:  0, y: 25 },
    down:  { x: -25, y: 10 },
};

function buildTaurusAnim(sheets: Spritesheet[], dir: string, type: string, count: number): SpriteAnimation {
    const frames = Array.from({ length: count }, (_, i) =>
        getTexture(sheets, `Taurus/taurus_${dir}_${type}_${i + 1}`),
    );
    return new SpriteAnimation(frames, [], 150, true);
}

export class Taurus extends Enemy {
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
        onShake?: (intensity: number) => void,
    ) {
        const animations = buildEnemyAnimations(
            sheets, 'Taurus/taurus',
            { front: 6, back: 6, side: 5 },
            { front: 2, back: 2, side: 2 },
        );

        // Extra Taurus animations
        animations.add('TaurusChargeFront', buildTaurusAnim(sheets, 'front', 'charge', 4));
        animations.add('TaurusChargeBack',  buildTaurusAnim(sheets, 'back',  'charge', 4));
        animations.add('TaurusChargeSide',  buildTaurusAnim(sheets, 'side',  'charge', 4));
        animations.add('TaurusRunFront',    buildTaurusAnim(sheets, 'front', 'run', 5));
        animations.add('TaurusRunBack',     buildTaurusAnim(sheets, 'back',  'run', 5));
        animations.add('TaurusRunSide',     buildTaurusAnim(sheets, 'side',  'run', 5));

        animations.play('IdleDown');

        const torch = new MarkLight({
            x: startX + TORCH_OFFSETS.down.x,
            y: startY + TORCH_OFFSETS.down.y,
            z: 46, opacity: 0.5, radius: 300, castsShadows: false,
            color1: 0xFF2600, color2: 0xC9341A, distanceSquared: 4950,
        });

        const ai = new PatrolChargeAI(waypoints, grid, candlesManager, player, onShake);

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

    /** Taurus cannot die (no death animation). */
    override die(): void {
        // no-op
    }
}
