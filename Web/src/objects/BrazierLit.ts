// Port of C# BrazierLit.cs — static ambient light source (no interaction).
// Unlike candles, braziers have shadow + normal map layers and do not die.
// Normal map goes into objectsNormalContainer (used by ShadowSystem normal-map pass),
// NOT into objectsContainer — otherwise the blue normal map texture is drawn visibly.
import { Container, Sprite, Spritesheet } from 'pixi.js';
import { SpriteAnimation } from '../animation/SpriteAnimation';
import { getTexture } from '../assets/AssetsLoader';
import { MarkLight } from '../lighting/MarkLight';
import { zForCharacter, Z_OVERALL, Z_SHADOWS } from '../grid/ZIndexCalculator';

const CANDLE_COLOR1 = 0xFFC3A3;
const CANDLE_COLOR2 = 0xFFF7CF;

const FRAME_DURATION_MS = 100;
const LIGHT_OFFSET_Y = -50;
const GLOW_ALPHA = 0.25; // matches C# SetGlowOpacity(0.25f)

/** Environmental brazier with animated flame, shadow, normal map, glow, and light (port of C# BrazierLit.cs). */
export class BrazierLit {
    readonly surroundingLight: MarkLight;

    private readonly _sprite: Sprite;
    private readonly _animation: SpriteAnimation;
    private readonly _shadowSprite: Sprite;
    private readonly _shadowAnimation: SpriteAnimation;
    private readonly _normalSprite: Sprite;
    private readonly _normalContainer: Container;
    private readonly _glowSprite: Sprite;

    constructor(
        sheets: Spritesheet[],
        x: number,
        y: number,
        container: Container,
        normalContainer: Container,
    ) {
        const brazierFrames = Array.from({ length: 7 }, (_, i) =>
            getTexture(sheets, `Objects/Brazier/brazier_lit_${i + 1}`),
        );
        this._animation = new SpriteAnimation(brazierFrames, [], FRAME_DURATION_MS, true);

        this._sprite = new Sprite(this._animation.currentFrame);
        this._sprite.anchor.set(0.5);
        this._sprite.x = x;
        this._sprite.y = y;
        this._sprite.zIndex = zForCharacter(y);
        container.addChild(this._sprite);

        // Shadow (below brazier, drawn in objectsContainer at Z_SHADOWS)
        const shadowFrames = Array.from({ length: 7 }, (_, i) =>
            getTexture(sheets, `Shadows/Brazier/brazier_shadow_${i + 1}`),
        );
        this._shadowAnimation = new SpriteAnimation(shadowFrames, [], FRAME_DURATION_MS, true);

        this._shadowSprite = new Sprite(this._shadowAnimation.currentFrame);
        this._shadowSprite.anchor.set(0.5);
        this._shadowSprite.x = x;
        this._shadowSprite.y = y;
        this._shadowSprite.zIndex = Z_SHADOWS;
        this._shadowSprite.tint = 0x000000;
        this._shadowSprite.alpha = 0.8;
        container.addChild(this._shadowSprite);

        // Normal map — goes into objectsNormalContainer, consumed only by the ShadowSystem pipeline
        this._normalContainer = normalContainer;
        this._normalSprite = new Sprite(getTexture(sheets, 'Objects/Brazier/brazier_normal'));
        this._normalSprite.anchor.set(0.5);
        this._normalSprite.x = x;
        this._normalSprite.y = y;
        this._normalSprite.zIndex = zForCharacter(y);
        normalContainer.addChild(this._normalSprite);

        // Glow — additive blend, warm tint, reduced alpha (C# DrawOverAllGlow / SetGlowOpacity 0.25)
        this._glowSprite = new Sprite(getTexture(sheets, 'Effects/glow'));
        this._glowSprite.anchor.set(0.5);
        this._glowSprite.x = x;
        this._glowSprite.y = y - 20;
        this._glowSprite.scale.set(0.7);
        this._glowSprite.zIndex = Z_OVERALL;
        this._glowSprite.blendMode = 'add';
        this._glowSprite.tint = CANDLE_COLOR1;
        this._glowSprite.alpha = GLOW_ALPHA;
        container.addChild(this._glowSprite);

        // Light source: same colors as candles, shorter range, no shadow casting
        this.surroundingLight = new MarkLight({
            x,
            y: y + LIGHT_OFFSET_Y,
            z: 67,
            opacity: 1,
            radius: 150,
            castsShadows: false,
            color1: CANDLE_COLOR1,
            color2: CANDLE_COLOR2,
            distanceSquared: 7_280,
            distanceSquaredForWall: 17_000,
        });
    }

    update(deltaMS: number): void {
        this._animation.update(deltaMS);
        this._sprite.texture = this._animation.currentFrame;

        this._shadowAnimation.update(deltaMS);
        this._shadowSprite.texture = this._shadowAnimation.currentFrame;

        this.surroundingLight.update(deltaMS);
    }

    destroy(container: Container): void {
        container.removeChild(this._sprite);
        container.removeChild(this._shadowSprite);
        container.removeChild(this._glowSprite);
        this._normalContainer.removeChild(this._normalSprite);
        this._sprite.destroy();
        this._shadowSprite.destroy();
        this._normalSprite.destroy();
        this._glowSprite.destroy();
    }
}
