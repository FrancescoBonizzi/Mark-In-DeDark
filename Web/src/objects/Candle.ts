import { Container, Rectangle, Sprite, Spritesheet } from 'pixi.js';
import { SpriteAnimation } from '../animation/SpriteAnimation';
import { getTexture } from '../assets/AssetsLoader';
import { MarkLight } from '../lighting/MarkLight';
import { zForCharacter } from '../grid/ZIndexCalculator';

// Candle light colors from GameConfig: RGB(255,195,163) and RGB(255,247,207)
const CANDLE_COLOR1 = 0xFFC3A3;
const CANDLE_COLOR2 = 0xFFF7CF;

const FRAME_DURATION_MS = 120;
const LIGHT_OFFSET_Y = -55;

/** A placed candle with light (port of C# Candle.cs). */
export class Candle {
    readonly surroundingLight: MarkLight;
    readonly collisionRectangle: Rectangle;

    private readonly _sprite: Sprite;
    private readonly _animation: SpriteAnimation;
    private _isDying = false;
    private _dyingElapsed = 0;
    private _DYING_DURATION_MS = 3000;

    /** True when an enemy is already heading toward this candle (prevents double-targeting). */
    isBeingFollowed = false;

    constructor(
        sheets: Spritesheet[],
        x: number,
        y: number,
        container: Container,
    ) {
        // Build 9-frame animation
        const frames = Array.from({ length: 9 }, (_, i) =>
            getTexture(sheets, `Objects/Candle/candle_${i + 1}`),
        );
        this._animation = new SpriteAnimation(frames, [], FRAME_DURATION_MS, true);

        this._sprite = new Sprite(this._animation.currentFrame);
        this._sprite.anchor.set(0.5);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForCharacter(y) + 0.0001;
        container.addChild(this._sprite);

        // Candle light: z=67, distanceSquared=7280, radius=150
        this.surroundingLight = new MarkLight({
            x,
            y: y + LIGHT_OFFSET_Y,
            z: 45, // C# 67 × (2/3) = 44.7 ≈ 45
            opacity: 1,
            radius: 150,
            castsShadows: false,
            color1: CANDLE_COLOR1,
            color2: CANDLE_COLOR2,
            distanceSquared: 3_236, // C# 7_280 × (2/3)²
        });

        this.collisionRectangle = new Rectangle(x - 24, y - 24, 48, 48);
    }

    get isAlive(): boolean { return !this._isDying || this._dyingElapsed < this._DYING_DURATION_MS; }
    get isDead(): boolean { return this._isDying && this._dyingElapsed >= this._DYING_DURATION_MS; }

    /** Starts the fade-out death sequence. Optional durationMS overrides the default 3000ms. */
    die(durationMS = 3000): void {
        if (this._isDying) return;
        this._isDying = true;
        this._DYING_DURATION_MS = durationMS;
    }

    update(deltaMS: number): void {
        if (this.isDead) return;

        this._animation.update(deltaMS);
        this._sprite.texture = this._animation.currentFrame;
        this.surroundingLight.update(deltaMS);

        if (this._isDying) {
            this._dyingElapsed += deltaMS;
            const t = Math.min(this._dyingElapsed / this._DYING_DURATION_MS, 1);
            const alpha = 1 - t;
            this._sprite.alpha = alpha;
            this.surroundingLight.opacity = alpha;
        }
    }

    destroy(container: Container): void {
        container.removeChild(this._sprite);
        this._sprite.destroy();
    }
}
