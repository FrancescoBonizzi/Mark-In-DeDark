import { Spritesheet, Texture } from 'pixi.js';
import { getTexture } from '../../assets/AssetsLoader';
import { SpriteAnimation } from '../../animation/SpriteAnimation';
import { AnimationsManager } from '../../animation/AnimationsManager';

const FRAME_DURATION_MS = 150;

function buildAnim(
    sheets: Spritesheet[],
    baseName: string,
    frameCount: number,
): SpriteAnimation {
    const frames: Texture[] = [];
    const normals: Texture[] = [];
    for (let i = 1; i <= frameCount; i++) {
        frames.push(getTexture(sheets, `${baseName}_${i}`));
        try { normals.push(getTexture(sheets, `${baseName}_${i}_normal`)); } catch { /* no normal */ }
    }
    return new SpriteAnimation(frames, normals, FRAME_DURATION_MS, true);
}

function buildSingleFrame(sheets: Spritesheet[], baseName: string): SpriteAnimation {
    return buildAnim(sheets, baseName, 1);
}

/**
 * Builds the standard enemy AnimationsManager.
 * type  — e.g. "Zombie/zombie"
 * Frames per animation: walk front/back/side, attack front/back/side.
 * Idle uses the first walk frame (single-frame animation).
 */
export function buildEnemyAnimations(
    sheets: Spritesheet[],
    type: string,
    walkFrames: { front: number; back: number; side: number },
    attackFrames: { front: number; back: number; side: number },
): AnimationsManager {
    const mgr = new AnimationsManager();

    mgr.add('WalkDown',  buildAnim(sheets, `${type}_front_walking`, walkFrames.front));
    mgr.add('WalkUp',    buildAnim(sheets, `${type}_back_walking`,  walkFrames.back));
    mgr.add('WalkSide',  buildAnim(sheets, `${type}_side_walking`,  walkFrames.side));

    // Idle = first walk frame (no separate "iddle" frames for enemies)
    mgr.add('IdleDown',  buildSingleFrame(sheets, `${type}_front_walking`));
    mgr.add('IdleUp',    buildSingleFrame(sheets, `${type}_back_walking`));
    mgr.add('IdleSide',  buildSingleFrame(sheets, `${type}_side_walking`));

    mgr.add('AttackDown', buildAnim(sheets, `${type}_front_attack`, attackFrames.front));
    mgr.add('AttackUp',   buildAnim(sheets, `${type}_back_attack`,  attackFrames.back));
    mgr.add('AttackSide', buildAnim(sheets, `${type}_side_attack`,  attackFrames.side));

    return mgr;
}
