import { Container, Rectangle, Sprite, Spritesheet } from 'pixi.js';
import { SpriteAnimation } from '../animation/SpriteAnimation';
import { getTexture } from '../assets/AssetsLoader';
import { TriggerObject } from './TriggerObject';
import { Character } from '../characters/Character';
import { Z_GROUND_OBJECTS } from '../grid/ZIndexCalculator';

const FRAME_DURATION_MS = 60;

/** A hole trap that instantly kills characters that walk over it (port of C# InstantlyDieTrap.cs). */
export class InstantlyDieTrap extends TriggerObject {
    private readonly _sprite: Sprite;
    private readonly _animation: SpriteAnimation;
    private _isPlaying = false;

    constructor(
        sheets: Spritesheet[],
        cellX: number,
        cellY: number,
        cellSize: number,
        container: Container,
    ) {
        // HitBoxTolerance(70, 70, 140, 140)
        const cx = cellX + cellSize / 2;
        const cy = cellY + cellSize / 2;
        super(new Rectangle(cx - 70, cy - 70, 140, 140), false);

        const frames = Array.from({ length: 10 }, (_, i) =>
            getTexture(sheets, `Grounds/trapHole/${i + 1}`),
        );
        this._animation = new SpriteAnimation(frames, [], FRAME_DURATION_MS, false);

        this._sprite = new Sprite(frames[0]!);
        this._sprite.anchor.set(0.5);
        this._sprite.x = cx;
        this._sprite.y = cy;
        this._sprite.zIndex = Z_GROUND_OBJECTS;
        container.addChild(this._sprite);
    }

    protected _onTrigger(character: Character): void {
        this._isPlaying = true;
        this._animation.reset();
        character.fallInTrap(
            this._triggerArea.x + this._triggerArea.width / 2,
            this._triggerArea.y + this._triggerArea.height / 2,
        );
    }

    update(deltaMS: number): void {
        if (this._isPlaying && !this._animation.isFinished) {
            this._animation.update(deltaMS);
            this._sprite.texture = this._animation.currentFrame;
        }
    }

    destroy(container: Container): void {
        container.removeChild(this._sprite);
        this._sprite.destroy();
    }
}
