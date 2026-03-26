import { Container, Spritesheet } from 'pixi.js';
import { Character, TorchOffsets } from './Character';
import { CharacterActions } from '../interaction/CharacterActions';
import { AnimationsManager } from '../animation/AnimationsManager';
import { SpriteAnimation } from '../animation/SpriteAnimation';
import { MarkLight } from '../lighting/MarkLight';
import { GameGrid } from '../grid/GameGrid';
import { CandlesManager } from '../objects/CandlesManager';
import { getTexture } from '../assets/AssetsLoader';
import { GameTexts } from '../ui/GameTexts';

const PLAYER_SPEED = 0.6;      // seconds per cell (from C# GameConfig.PlayerSpeed)
const FRAME_DURATION_MS = 150; // ms per frame (from C# CharactersAnimationsMillisecondsBetweenFrames)

const TORCH_OFFSETS: TorchOffsets = {
    up:    { x: -30, y: -38 },
    left:  { x: -38, y: -38 },
    right: { x:  38, y: -38 },
    down:  { x:  35, y: -30 },
};

function buildAnimation(
    sheets: Spritesheet[],
    baseName: string,
    frameCount: number,
    loop = true,
): SpriteAnimation {
    const frames = Array.from({ length: frameCount }, (_, i) =>
        getTexture(sheets, `${baseName}_${i + 1}`),
    );
    const normalFrames = Array.from({ length: frameCount }, (_, i) => {
        try { return getTexture(sheets, `${baseName}_${i + 1}_normal`); }
        catch { return null; }
    }).filter((t): t is NonNullable<typeof t> => t !== null);

    return new SpriteAnimation(frames, normalFrames, FRAME_DURATION_MS, loop);
}

function buildPlayerAnimations(sheets: Spritesheet[]): AnimationsManager {
    const mgr = new AnimationsManager();
    mgr.add('WalkDown',  buildAnimation(sheets, 'Player/player_front_walking', 4));
    mgr.add('WalkUp',    buildAnimation(sheets, 'Player/player_back_walking',  4));
    mgr.add('WalkSide',  buildAnimation(sheets, 'Player/player_side_walking',  4));
    mgr.add('IdleDown',  buildAnimation(sheets, 'Player/player_front_iddle',   4));
    mgr.add('IdleUp',    buildAnimation(sheets, 'Player/player_back_iddle',    4));
    mgr.add('IdleSide',  buildAnimation(sheets, 'Player/player_side_iddle',    4));
    return mgr;
}

/** Concrete Player class (port of C# Player.cs). */
export class Player extends Character {
    private _candlesManager: CandlesManager | null = null;
    /** Called when the player tries to interact but gets a feedback message. */
    onMessage: ((text: string, color: string) => void) | undefined = undefined;

    constructor(
        grid: GameGrid,
        sheets: Spritesheet[],
        objectsContainer: Container,
        objectsNormalContainer: Container,
    ) {
        const startCell = grid.playerStartCell;
        const startX = startCell?.centerX ?? grid.playerStartCol * 192 + 96;
        const startY = startCell?.centerY ?? grid.playerStartRow * 192 + 96;

        const torchLight = new MarkLight({
            x: startX + TORCH_OFFSETS.down.x,
            y: startY + TORCH_OFFSETS.down.y,
            z: 90,
            opacity: 1,
            radius: 1200,
            castsShadows: false,
            color1: 0xFFEDD6,
            color2: 0xFFFFFF,
            distanceSquared: 20_600,
        });

        super(grid, objectsContainer, objectsNormalContainer, {
            startX,
            startY,
            movementSpeed: PLAYER_SPEED,
            animations: buildPlayerAnimations(sheets),
            torch: { light: torchLight, offsets: TORCH_OFFSETS },
        });
    }

    /** Wire up the CandlesManager so the player can place candles. */
    setCandlesManager(mgr: CandlesManager): void {
        this._candlesManager = mgr;
    }

    override handleInput(action: CharacterActions): void {
        if (this.isDead || this.isDying) return;

        // Space is the universal interact key (port of C# behaviour):
        // priority 1 — toggle a door if standing on a door cell;
        // priority 2 — place a candle otherwise.
        if (action === CharacterActions.Candle) {
            const cell = this.currentCell;
            // Door toggle requires the player to be still (avoid accidental triggers while walking)
            if (!this.isMoving && cell && this._grid.hasDoorOnCell(cell)) {
                this._grid.switchDoorIfAny(cell);
            } else if (this._candlesManager) {
                if (!this._candlesManager.doIHaveACandleLeft) {
                    this.onMessage?.(GameTexts.notEnoughCandles, '#ff6b6b');
                } else if (cell && !this._candlesManager.isThereACandle(cell)) {
                    this._candlesManager.placeCandle(cell.centerX, cell.centerY);
                }
            }
            return;
        }

        super.handleInput(action);
    }
}
