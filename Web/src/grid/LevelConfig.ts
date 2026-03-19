import { Spritesheet, Texture } from 'pixi.js';

function tryGetTexture(sheets: Spritesheet[], name: string): Texture | null {
    for (const sheet of sheets) {
        const tex = sheet.textures[name];
        if (tex) return tex;
    }
    return null;
}

function getTexture(sheets: Spritesheet[], name: string): Texture {
    const tex = tryGetTexture(sheets, name);
    if (!tex) throw new Error(`Texture "${name}" not found in any spritesheet`);
    return tex;
}

export interface LevelConfig {
    envTexturePackId: number;
    groundTextures: Texture[];
    groundNormalTextures: (Texture | null)[];
    groundSpecTextures: (Texture | null)[];
    riempimentoTextures: Texture[];
    riempimentoNormalTextures: (Texture | null)[];
    /** Maps GroundType enum value → specific texture (Terra1→cell1, Riempimento1→riempimento_1, …) */
    groundTypeToTexture: Map<number, Texture>;
    darkCellTexture: Texture;
    randomHorizontalWalls: Texture[];
    randomHorizontalWallsNormal: (Texture | null)[];
    randomVerticalWalls: Texture[];
    randomVerticalWallsNormal: (Texture | null)[];
    pillarTextures: Texture[];
    pillarNormalTextures: (Texture | null)[];
    wallHorizontalBreakable: Texture;
    wallHorizontalBroken: Texture;
    wallVerticalBreakable: Texture;
    wallVerticalBrokenA: Texture;
    wallVerticalBrokenB: Texture;
    specialWalls: (Texture | null)[];
    ingressoClosed: Texture | null;
    ingressoOpen: Texture | null;
    ingressoUp: Texture | null;
    uscitaClosed: Texture | null;
    uscitaOpen: Texture | null;
    uscitaUp: Texture | null;
    wallDoor: Texture | null;
    wallDoorUp: Texture | null;
    doorFrames: Texture[];
    decalTextures: (Texture | null)[];
    dirtyTextures: Texture[];
}

export function createLevelConfig(envPackId: number, sheets: Spritesheet[]): LevelConfig {
    const p = `Grounds/pacco${envPackId}`;

    const groundTextures: Texture[] = [];
    const groundNormalTextures: (Texture | null)[] = [];
    const groundSpecTextures: (Texture | null)[] = [];
    for (let n = 1; n <= 10; n++) {
        const tex = tryGetTexture(sheets, `${p}/cell${n}`);
        if (!tex) break;
        groundTextures.push(tex);
        groundNormalTextures.push(tryGetTexture(sheets, `${p}/cell${n}_normal`));
        groundSpecTextures.push(tryGetTexture(sheets, `${p}/cell${n}_spec`));
    }

    const riempimentoTextures: Texture[] = [];
    const riempimentoNormalTextures: (Texture | null)[] = [];
    for (let n = 1; n <= 13; n++) {
        const tex = tryGetTexture(sheets, `${p}/Riempimento/riempimento_${n}`);
        if (!tex) break;
        riempimentoTextures.push(tex);
        riempimentoNormalTextures.push(tryGetTexture(sheets, `${p}/Riempimento/riempimento_${n}_normal`));
    }

    const randomHorizontalWalls: Texture[] = [];
    const randomHorizontalWallsNormal: (Texture | null)[] = [];
    for (let n = 1; n <= 19; n++) {
        const tex = tryGetTexture(sheets, `${p}/RandomWalls/wall_horizontal_${n}`);
        if (!tex) break;
        randomHorizontalWalls.push(tex);
        randomHorizontalWallsNormal.push(tryGetTexture(sheets, `${p}/RandomWalls/wall_horizontal_${n}_normal`));
    }

    const randomVerticalWalls: Texture[] = [];
    const randomVerticalWallsNormal: (Texture | null)[] = [];
    for (let n = 1; n <= 19; n++) {
        const tex = tryGetTexture(sheets, `${p}/RandomWalls/wall_vertical_${n}`);
        if (!tex) break;
        randomVerticalWalls.push(tex);
        randomVerticalWallsNormal.push(tryGetTexture(sheets, `${p}/RandomWalls/wall_vertical_${n}_normal`));
    }

    const pillarTextures: Texture[] = [];
    const pillarNormalTextures: (Texture | null)[] = [];
    for (let n = 1; n <= 19; n++) {
        const tex = tryGetTexture(sheets, `${p}/RandomPillars/pillar_${n}`);
        if (!tex) break;
        pillarTextures.push(tex);
        pillarNormalTextures.push(tryGetTexture(sheets, `${p}/RandomPillars/pillar_${n}_normal`));
    }

    const specialWalls: (Texture | null)[] = [];
    for (let n = 1; n <= 5; n++) {
        specialWalls.push(tryGetTexture(sheets, `${p}/SpecialWalls/wall_special_${n}`));
    }

    const doorFrames: Texture[] = [];
    for (let n = 1; n <= 9; n++) {
        const tex = tryGetTexture(sheets, `${p}/Door/door_${n}`);
        if (tex) doorFrames.push(tex);
    }

    const decalTextures: (Texture | null)[] = [];
    for (let n = 1; n <= 8; n++) {
        decalTextures.push(tryGetTexture(sheets, `${p}/Decals/decal_${n}`));
    }

    const dirtyTextures: Texture[] = [];
    for (let n = 1; n <= 20; n++) {
        const tex = tryGetTexture(sheets, `${p}/Dirty/dirty_${n}`);
        if (!tex) break;
        dirtyTextures.push(tex);
    }

    // Map each GroundType enum value to its specific texture (Terra1→cell1, Terra2→cell2, …)
    // Terra enum values in order: 1, 6, 9, 10, 11, 12, 13, 14, 15, 16
    const terraEnumValues = [1, 6, 9, 10, 11, 12, 13, 14, 15, 16];
    // Riempimento enum values in order: 8, 17–28
    const riempimentoEnumValues = [8, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28];
    const groundTypeToTexture = new Map<number, Texture>();
    const fallbackTex = groundTextures[0];
    for (let i = 0; i < terraEnumValues.length; i++) {
        const tex = groundTextures[i] ?? fallbackTex;
        if (tex) groundTypeToTexture.set(terraEnumValues[i]!, tex);
    }
    for (let i = 0; i < riempimentoEnumValues.length; i++) {
        const tex = riempimentoTextures[i];
        if (tex) groundTypeToTexture.set(riempimentoEnumValues[i]!, tex);
    }

    return {
        envTexturePackId: envPackId,
        groundTextures,
        groundNormalTextures,
        groundSpecTextures,
        riempimentoTextures,
        riempimentoNormalTextures,
        darkCellTexture: getTexture(sheets, 'Grounds/celldark'),
        randomHorizontalWalls,
        randomHorizontalWallsNormal,
        randomVerticalWalls,
        randomVerticalWallsNormal,
        pillarTextures,
        pillarNormalTextures,
        wallHorizontalBreakable: getTexture(sheets, `${p}/wall_horizontal_breakable`),
        wallHorizontalBroken: getTexture(sheets, `${p}/wall_horizontal_broken`),
        wallVerticalBreakable: getTexture(sheets, `${p}/wall_vertical_breakable`),
        wallVerticalBrokenA: getTexture(sheets, `${p}/wall_vertical_broken_A`),
        wallVerticalBrokenB: getTexture(sheets, `${p}/wall_vertical_broken_B`),
        specialWalls,
        ingressoClosed: tryGetTexture(sheets, `${p}/wall_ingresso_closed`),
        ingressoOpen: tryGetTexture(sheets, `${p}/wall_ingresso_open`),
        ingressoUp: tryGetTexture(sheets, `${p}/wall_ingresso_up`),
        uscitaClosed: tryGetTexture(sheets, `${p}/wall_uscita_closed`),
        uscitaOpen: tryGetTexture(sheets, `${p}/wall_uscita_open`),
        uscitaUp: tryGetTexture(sheets, `${p}/wall_uscita_up`),
        wallDoor: tryGetTexture(sheets, `${p}/wall_door`),
        wallDoorUp: tryGetTexture(sheets, `${p}/wall_door_up`),
        doorFrames,
        decalTextures,
        dirtyTextures,
        groundTypeToTexture,
    };
}
