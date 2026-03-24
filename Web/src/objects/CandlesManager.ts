import { Container, Spritesheet } from 'pixi.js';
import { Candle } from './Candle';
import { GameCell } from '../grid/GameCell';
import { ShadowSystem } from '../lighting/ShadowSystem';

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

    destroy(): void {
        for (const candle of this.candles) {
            this._shadowSystem.removeLight(candle.surroundingLight);
            candle.destroy(this._objectsContainer);
        }
        this.candles.length = 0;
    }
}
