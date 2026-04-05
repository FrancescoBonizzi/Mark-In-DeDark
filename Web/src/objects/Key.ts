import { Container, Rectangle, Sprite, Spritesheet } from 'pixi.js';
import { getTexture } from '../assets/AssetsLoader';
import { MarkLight } from '../lighting/MarkLight';
import { zForCharacter } from '../grid/ZIndexCalculator';

/** Collectible key that unlocks the level exit (port of C# CollectableObject for key). */
export class Key {
    readonly collisionRectangle: Rectangle;
    readonly surroundingLight: MarkLight;

    private readonly _sprite: Sprite;
    private _collected = false;
    private _fadeAlpha = 1;
    private readonly _FADE_RATE = 1 / 500;

    private _bobTimer = 0;
    private readonly _BOB_SPEED = 0.002;
    private readonly _BOB_AMP = 0.06;

    constructor(
        sheets: Spritesheet[],
        x: number,
        y: number,
        container: Container,
    ) {
        const tex = getTexture(sheets, 'Objects/key');
        this._sprite = new Sprite(tex);
        this._sprite.anchor.set(0.5);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForCharacter(y);

        // HitBoxTolerance(50, 50, 100, 100)
        this.collisionRectangle = new Rectangle(x - 50, y - 50, 100, 100);
        container.addChild(this._sprite);

        this.surroundingLight = new MarkLight({
            x,
            y,
            z: 31,
            opacity: 0.5,
            radius: 80,
            castsShadows: false,
            color1: 0xffdd44,
            color2: 0xffaa00,
            distanceSquared: 2_200,
        });
        // lightMaskScale default=2.0 coprirebbe >2 celle; 0.5 ≈ mezza cella
        this.surroundingLight.lightMaskScale = 0.9;
        this.surroundingLight.opacity = 0.3;
    }

    get isCollected(): boolean { return this._collected; }
    get isGone(): boolean { return this._collected && this._fadeAlpha <= 0; }

    collect(): void {
        if (this._collected) return;
        this._collected = true;
    }

    update(deltaMS: number): void {
        if (this.isGone) return;

        this.surroundingLight.update(deltaMS);

        if (this._collected) {
            this._fadeAlpha = Math.max(0, this._fadeAlpha - this._FADE_RATE * deltaMS);
            this._sprite.alpha = this._fadeAlpha;
            this.surroundingLight.opacity = this._fadeAlpha * 0.5;
            if (this._fadeAlpha <= 0) this.surroundingLight.enabled = false;
            return;
        }

        this._bobTimer += deltaMS * this._BOB_SPEED;
        this._sprite.scale.set(1 + Math.sin(this._bobTimer) * this._BOB_AMP);
    }

    destroy(container: Container): void {
        container.removeChild(this._sprite);
        this._sprite.destroy();
    }
}
