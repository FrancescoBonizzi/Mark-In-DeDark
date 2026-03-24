import { Container, Spritesheet } from 'pixi.js';
import { Character, TorchOffsets } from './Character';
import { CharacterActions } from '../interaction/CharacterActions';
import { AnimationsManager } from '../animation/AnimationsManager';
import { SpriteAnimation } from '../animation/SpriteAnimation';
import { MarkLight } from '../lighting/MarkLight';
import { GameGrid } from '../grid/GameGrid';
import { getTexture } from '../assets/AssetsLoader';

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

/**
 * Concrete Player class (port of C# Player.cs).
 * Wraps Character with player-specific torch configuration and
 * stubs for door/candle actions (implemented in Task 8).
 */
export class Player extends Character {
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

    override handleInput(action: CharacterActions): void {
        // Stubs for Task 8
        if (action === CharacterActions.DoorSwitch) return;
        if (action === CharacterActions.Candle) return;
        super.handleInput(action);
    }
}
