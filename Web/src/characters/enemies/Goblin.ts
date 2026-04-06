import { Container, Spritesheet } from 'pixi.js';
import { Enemy, EnemyConfig } from '../Enemy';
import { MarkLight } from '../../lighting/MarkLight';
import { GameGrid } from '../../grid/GameGrid';
import { Player } from '../Player';
import { CandlesManager } from '../../objects/CandlesManager';
import { PlayerHunterAI } from '../../ai/PlayerHunterAI';
import { buildEnemyAnimations } from './enemyAnimations';

function lerpColor(c1: number, c2: number, t: number): number {
    const r = Math.round(((c1 >> 16) & 0xff) + (((c2 >> 16) & 0xff) - ((c1 >> 16) & 0xff)) * t);
    const g = Math.round(((c1 >> 8) & 0xff) + (((c2 >> 8) & 0xff) - ((c1 >> 8) & 0xff)) * t);
    const b = Math.round((c1 & 0xff) + ((c2 & 0xff) - (c1 & 0xff)) * t);
    return (r << 16) | (g << 8) | b;
}

const BASE_SPEED_S = 0.8;
const SPEED_DECREMENT = 0.1; // Gets faster with each candle placed
const MIN_SPEED_S = 0.2;
const SPEED_WARNING_THRESHOLD_MS = 500; // Below 0.5s/cell → warn player

const TORCH_OFFSETS = {
    up:    { x: -35, y: -10 },
    left:  { x:   5, y:   5 },
    right: { x:  -5, y:   5 },
    down:  { x:  30, y: -15 },
};

export class Goblin extends Enemy {
    private _lastUsedCandleCount = 0;
    private _speedWarningFired = false;

    /** Called once when Goblin crosses the speed warning threshold. */
    onSpeedWarning: (() => void) | undefined;

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
            z: 31, // C# 46 × (2/3) = 30.7 ≈ 31
            opacity: 0.5, radius: 300, castsShadows: false,
            color1: 0xCAF076, color2: 0xC3E27C, distanceSquared: 2_200, // C# 4_950 × (2/3)²
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

            // One-time warning when speed crosses threshold
            if (!this._speedWarningFired && this.movementSpeedMS < SPEED_WARNING_THRESHOLD_MS) {
                this._speedWarningFired = true;
                this.onSpeedWarning?.();
            }

            // Progressive tint: 0 candles = white, 3 = orange, 6+ = red
            if (currentUsed <= 3) {
                this._mainSprite.tint = lerpColor(0xFFFFFF, 0xFF8800, currentUsed / 3);
            } else {
                this._mainSprite.tint = lerpColor(0xFF8800, 0xFF2200, Math.min((currentUsed - 3) / 3, 1));
            }
        }
        super.update(deltaMS);
    }
}
