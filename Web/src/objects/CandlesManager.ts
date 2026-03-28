import { Container, Spritesheet } from 'pixi.js';
import { Candle } from './Candle';
import { GameCell } from '../grid/GameCell';
import { CELL_SIZE } from '../grid/GameCell';
import { ShadowSystem } from '../lighting/ShadowSystem';
import { GameGrid } from '../grid/GameGrid';
import { CharacterActions } from '../interaction/CharacterActions';

/** Manages placed candles and the player's candle inventory (port of C# CandlesManager.cs). */
export class CandlesManager {
    private readonly _sheets: Spritesheet[];
    private readonly _objectsContainer: Container;
    private readonly _shadowSystem: ShadowSystem;

    readonly candles: Candle[] = [];

    private _leftCandles = 0;
    private _playerUsedCandles = 0;

    constructor(
        sheets: Spritesheet[],
        objectsContainer: Container,
        shadowSystem: ShadowSystem,
        startingCandles = 0,
    ) {
        this._sheets = sheets;
        this._objectsContainer = objectsContainer;
        this._shadowSystem = shadowSystem;
        this._leftCandles = startingCandles;
    }

    get leftCandles(): number { return this._leftCandles; }
    get playerUsedCandles(): number { return this._playerUsedCandles; }
    get doIHaveACandleLeft(): boolean { return this._leftCandles > 0; }

    addCandles(count: number): void {
        this._leftCandles += count;
    }

    isThereACandle(cell: GameCell): boolean {
        return this.candles.some(c =>
            c.isAlive &&
            Math.abs(c.collisionRectangle.x + 24 - cell.centerX) < 10 &&
            Math.abs(c.collisionRectangle.y + 24 - cell.centerY) < 10,
        );
    }

    /**
     * Returns the first alive candle on the given cell (C#: IsThereACandle returns Candle).
     * Uses cell-reference comparison via grid, matching the C# GetCellAtPixel approach.
     * Does NOT filter by isBeingFollowed — collision-based kill should work on any alive candle.
     */
    getCandleAtCell(cell: GameCell, grid: GameGrid): Candle | null {
        for (const candle of this.candles) {
            if (!candle.isAlive) continue;
            const cx = candle.collisionRectangle.x + candle.collisionRectangle.width / 2;
            const cy = candle.collisionRectangle.y + candle.collisionRectangle.height / 2;
            if (grid.getCellAtPixel(cx, cy) === cell) return candle;
        }
        return null;
    }

    placeCandle(x: number, y: number): boolean {
        if (!this.doIHaveACandleLeft) return false;

        const candle = new Candle(this._sheets, x, y, this._objectsContainer);
        this.candles.push(candle);
        this._shadowSystem.addLight(candle.surroundingLight);
        this._leftCandles--;
        this._playerUsedCandles++;
        return true;
    }

    update(deltaMS: number): void {
        for (let i = this.candles.length - 1; i >= 0; i--) {
            const candle = this.candles[i]!;
            candle.update(deltaMS);
            if (candle.isDead) {
                this._shadowSystem.removeLight(candle.surroundingLight);
                candle.destroy(this._objectsContainer);
                this.candles.splice(i, 1);
            }
        }
    }

    /**
     * Returns the nearest alive candle reachable within maxCellRange cells from fromCell.
     * Used by KillCandleAI to find a candle target.
     */
    getNearestCandlePath(
        fromCell: GameCell,
        maxCellRange: number,
        grid: GameGrid,
    ): { candle: Candle; actions: CharacterActions[] } | null {
        const maxDistSq = (CELL_SIZE * (maxCellRange + 1)) ** 2;
        let best: { candle: Candle; actions: CharacterActions[] } | null = null;
        let bestLen = Infinity;

        for (const candle of this.candles) {
            if (!candle.isAlive || candle.isBeingFollowed) continue;

            const cx = candle.collisionRectangle.x + candle.collisionRectangle.width / 2;
            const cy = candle.collisionRectangle.y + candle.collisionRectangle.height / 2;
            const dx = cx - fromCell.centerX;
            const dy = cy - fromCell.centerY;
            if (dx * dx + dy * dy > maxDistSq) continue;

            const targetCell = grid.getCellAtPixel(cx, cy);
            if (!targetCell) continue;

            const actions = grid.findActionsToPath(fromCell, targetCell);
            // actions.length === 0 means same cell — valid when within range (C# fix: don't skip zero-length paths)
            if (actions.length > maxCellRange) continue;

            if (actions.length < bestLen) {
                bestLen = actions.length;
                best = { candle, actions };
                if (bestLen === 1) break;
            }
        }

        return best;
    }

    destroy(): void {
        for (const candle of this.candles) {
            this._shadowSystem.removeLight(candle.surroundingLight);
            candle.destroy(this._objectsContainer);
        }
        this.candles.length = 0;
    }
}
