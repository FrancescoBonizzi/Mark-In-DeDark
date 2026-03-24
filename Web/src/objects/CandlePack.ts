import { Container, Rectangle, Sprite, Spritesheet } from 'pixi.js';
import { getTexture } from '../assets/AssetsLoader';
import { zForCharacter } from '../grid/ZIndexCalculator';

/** Collectable candle pack on the ground (port of C# CandlePack.cs). */
export class CandlePack {
    readonly count: number;
    readonly collisionRectangle: Rectangle;

    private readonly _sprite: Sprite;
    private _collected = false;
    private _fadeAlpha = 1;
    private readonly _FADE_RATE = 1 / 500; // fade out over 500ms

    private _bobTimer = 0;
    private readonly _BOB_SPEED = 0.003;
    private readonly _BOB_AMP = 0.05;

    constructor(
        sheets: Spritesheet[],
        x: number,
        y: number,
        count: number,
        container: Container,
    ) {
        this.count = count;

        const texName = count >= 3 ? 'Objects/Candle/candle_pack_3'
            : count === 2 ? 'Objects/Candle/candle_pack_2'
            : 'Objects/Candle/candle_pack_1';

        const tex = getTexture(sheets, texName);
        this._sprite = new Sprite(tex);
        this._sprite.anchor.set(0.5);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForCharacter(y);

        // HitBoxTolerance(50, 50, 100, 100) from C#
        this.collisionRectangle = new Rectangle(
            x - 50, y - 50, 100, 100,
        );

        container.addChild(this._sprite);
    }

    get isCollected(): boolean { return this._collected; }
    get isGone(): boolean { return this._collected && this._fadeAlpha <= 0; }

    collect(): void {
        if (this._collected) return;
        this._collected = true;
    }

    update(deltaMS: number): void {
        if (this.isGone) return;

        if (this._collected) {
            this._fadeAlpha = Math.max(0, this._fadeAlpha - this._FADE_RATE * deltaMS);
            this._sprite.alpha = this._fadeAlpha;
            return;
        }

        // Bobbing scale animation
        this._bobTimer += deltaMS * this._BOB_SPEED;
        this._sprite.scale.set(1 + Math.sin(this._bobTimer) * this._BOB_AMP);
    }

    destroy(container: Container): void {
        container.removeChild(this._sprite);
        this._sprite.destroy();
    }
}
