import { Container, Rectangle, Sprite, Spritesheet } from 'pixi.js';
import { SpriteAnimation } from '../animation/SpriteAnimation';
import { getTexture } from '../assets/AssetsLoader';
import { TriggerObject } from './TriggerObject';
import { Character } from '../characters/Character';
import { Z_GROUND_OBJECTS } from '../grid/ZIndexCalculator';

const BREAK_TIME_MS = 2000; // GameConfig.BreakableTrapBreakingTimeSeconds
const COVER_FRAME_MS = 200;
const BREAK_FRAME_MS = 80;

const enum BreakState { Idle, GoingToBreak, Breaking, Broken }

/** Breakable cover trap — breaks after standing on it (port of C# TimedBreakableTrap.cs). */
export class TimedBreakableTrap extends TriggerObject {
    private _breakState: BreakState = BreakState.Idle;
    private _elapsed = 0;
    private _triggeringCharacter: Character | null = null;

    private readonly _coverSprite: Sprite;
    private readonly _coverAnim: SpriteAnimation;
    private readonly _breakingAnim: SpriteAnimation;
    private readonly _brokenSprite: Sprite;

    private readonly _cx: number;
    private readonly _cy: number;
    private readonly _container: Container;

    constructor(
        sheets: Spritesheet[],
        cellX: number,
        cellY: number,
        cellSize: number,
        container: Container,
    ) {
        const cx = cellX + cellSize / 2;
        const cy = cellY + cellSize / 2;
        // HitBoxTolerance(30, 30, 60, 60)
        super(new Rectangle(cx - 30, cy - 30, 60, 60), false);

        this._cx = cx;
        this._cy = cy;
        this._container = container;

        // Cover animation (idle: 2 frames)
        const coverFrames = [
            getTexture(sheets, 'Grounds/trap/trap_1'),
            getTexture(sheets, 'Grounds/trap/trap_2'),
        ];
        this._coverAnim = new SpriteAnimation(coverFrames, [], COVER_FRAME_MS, true);
        this._coverSprite = new Sprite(coverFrames[0]!);
        this._coverSprite.anchor.set(0.5);
        this._coverSprite.x = cx;
        this._coverSprite.y = cy;
        this._coverSprite.zIndex = Z_GROUND_OBJECTS;
        container.addChild(this._coverSprite);

        // Breaking animation (7 frames)
        const breakFrames = Array.from({ length: 7 }, (_, i) =>
            getTexture(sheets, `Grounds/trap/trap_breaking_${i + 1}`),
        );
        this._breakingAnim = new SpriteAnimation(breakFrames, [], BREAK_FRAME_MS, false);

        // Broken state sprite
        const brokenTex = getTexture(sheets, 'Grounds/trap/trap_broken');
        this._brokenSprite = new Sprite(brokenTex);
        this._brokenSprite.anchor.set(0.5);
        this._brokenSprite.x = cx;
        this._brokenSprite.y = cy;
        this._brokenSprite.zIndex = Z_GROUND_OBJECTS;
        this._brokenSprite.visible = false;
        container.addChild(this._brokenSprite);
    }

    protected _onTrigger(character: Character): void {
        if (this._breakState === BreakState.Broken) {
            character.fallInTrap(this._cx, this._cy);
            return;
        }
        if (this._breakState === BreakState.Idle) {
            this._breakState = BreakState.GoingToBreak;
            this._elapsed = 0;
            this._triggeringCharacter = character;
        }
    }

    protected override _onTriggerLeft(): void {
        if (this._breakState === BreakState.GoingToBreak) {
            this._breakState = BreakState.Idle;
            this._elapsed = 0;
            this._triggeringCharacter = null;
        }
    }

    update(deltaMS: number): void {
        switch (this._breakState) {
            case BreakState.Idle:
                this._coverAnim.update(deltaMS);
                this._coverSprite.texture = this._coverAnim.currentFrame;
                break;

            case BreakState.GoingToBreak:
                this._coverAnim.update(deltaMS);
                this._coverSprite.texture = this._coverAnim.currentFrame;
                this._elapsed += deltaMS;
                if (this._elapsed >= BREAK_TIME_MS) {
                    this._breakState = BreakState.Breaking;
                    this._breakingAnim.reset();
                    this._elapsed = 0;
                }
                break;

            case BreakState.Breaking:
                this._breakingAnim.update(deltaMS);
                this._coverSprite.texture = this._breakingAnim.currentFrame;
                if (this._breakingAnim.isFinished) {
                    this._breakState = BreakState.Broken;
                    this._coverSprite.visible = false;
                    this._brokenSprite.visible = true;
                    // Kill the character that triggered this
                    this._triggeringCharacter?.fallInTrap(this._cx, this._cy);
                    this._triggeringCharacter = null;
                }
                break;

            case BreakState.Broken:
                // Once broken, the trigger fires immediately on any character
                break;
        }
    }

    destroy(): void {
        this._container.removeChild(this._coverSprite);
        this._container.removeChild(this._brokenSprite);
        this._coverSprite.destroy();
        this._brokenSprite.destroy();
    }
}
