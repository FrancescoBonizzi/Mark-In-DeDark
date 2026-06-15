// GENERATO AUTOMATICAMENTE — non modificare manualmente.
// Rigenera con: npx tsx Web/scripts/generate-spritepack-manifest.ts
// oppure tramite: npm run build

export interface FrameCoord { ss: number; x: number; y: number }
export interface PackFrame   { frame: FrameCoord; real: boolean }

export interface PackData {
    cells:     PackFrame[];  // cell1..N in ordine
    decals:    PackFrame[];  // decal_1..N in ordine
    specwalls: PackFrame[];  // wall_special_1..N in ordine
}

/** Frame placeholder — coordinata usata dai pack che non hanno la texture. */
export const PLACEHOLDER_FRAME: FrameCoord = {ss:0,x:195,y:1353};

/** Pack disponibili (rilevati dai nomi frame negli spritesheet). */
export const PACK_IDS: readonly number[] = [1, 2, 3, 4, 5, 6, 7, 8] as const;

/** Frame dei 13 Riempimento — identici per tutti i pack. */
export const RIEMPIMENTO_FRAMES: FrameCoord[] = [{ss:0,x:503,y:1665},{ss:0,x:777,y:713},{ss:0,x:583,y:1315},{ss:0,x:697,y:1639},{ss:0,x:869,y:1833},{ss:0,x:931,y:195},{ss:0,x:995,y:1},{ss:0,x:931,y:389},{ss:0,x:777,y:1113},{ss:0,x:737,y:195},{ss:0,x:737,y:389},{ss:0,x:583,y:1121},{ss:0,x:737,y:919}];

/** Dati per pack: celle, decal, muri speciali con flag real/placeholder. */
export const SPRITEPACK_MANIFEST: Readonly<Record<number, PackData>> = {
    1: { cells:[{frame:{ss:0,x:931,y:907},real:true},{frame:{ss:0,x:891,y:1631},real:true},{frame:{ss:0,x:1189,y:1},real:true},{frame:{ss:0,x:1125,y:907},real:true},{frame:{ss:0,x:195,y:1353},real:false}], decals:[{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}], specwalls:[{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}] },
    2: { cells:[{frame:{ss:0,x:931,y:907},real:true},{frame:{ss:0,x:891,y:1631},real:true},{frame:{ss:0,x:1189,y:1},real:true},{frame:{ss:0,x:1125,y:907},real:true},{frame:{ss:1,x:195,y:389},real:true},{frame:{ss:1,x:195,y:971},real:true},{frame:{ss:1,x:195,y:1553},real:true},{frame:{ss:1,x:583,y:195},real:true},{frame:{ss:1,x:1165,y:195},real:true},{frame:{ss:1,x:1553,y:1},real:true}], decals:[{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}], specwalls:[{frame:{ss:1,x:1,y:1553},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:1,x:389,y:1},real:true},{frame:{ss:1,x:777,y:1},real:true},{frame:{ss:1,x:1165,y:1},real:true}] },
    3: { cells:[{frame:{ss:1,x:971,y:1359},real:true},{frame:{ss:1,x:1165,y:971},real:true},{frame:{ss:1,x:1747,y:971},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}], decals:[{frame:{ss:1,x:777,y:389},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:1,x:1359,y:389},real:true},{frame:{ss:1,x:583,y:583},real:true}], specwalls:[{frame:{ss:1,x:1553,y:777},real:true},{frame:{ss:1,x:971,y:971},real:true},{frame:{ss:0,x:195,y:735},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}] },
    4: { cells:[{frame:{ss:1,x:971,y:1359},real:true},{frame:{ss:1,x:1165,y:971},real:true},{frame:{ss:1,x:1747,y:971},real:true},{frame:{ss:2,x:1,y:1},real:true},{frame:{ss:2,x:1,y:583},real:true}], decals:[{frame:{ss:1,x:777,y:389},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:1,x:1359,y:389},real:true},{frame:{ss:1,x:583,y:583},real:true}], specwalls:[{frame:{ss:1,x:1553,y:777},real:true},{frame:{ss:1,x:971,y:971},real:true},{frame:{ss:1,x:1553,y:1359},real:true},{frame:{ss:1,x:1553,y:1553},real:true},{frame:{ss:1,x:1747,y:1553},real:true}] },
    5: { cells:[{frame:{ss:2,x:1,y:583},real:true},{frame:{ss:2,x:195,y:1359},real:true},{frame:{ss:2,x:389,y:195},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}], decals:[{frame:{ss:2,x:195,y:1},real:true},{frame:{ss:2,x:583,y:1},real:true},{frame:{ss:2,x:971,y:1},real:true},{frame:{ss:2,x:1359,y:1},real:true},{frame:{ss:2,x:1747,y:1},real:true}], specwalls:[{frame:{ss:2,x:195,y:777},real:true},{frame:{ss:2,x:195,y:1165},real:true},{frame:{ss:0,x:413,y:1},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}] },
    6: { cells:[{frame:{ss:2,x:1,y:583},real:true},{frame:{ss:2,x:195,y:1359},real:true},{frame:{ss:2,x:389,y:195},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}], decals:[{frame:{ss:2,x:195,y:1},real:true},{frame:{ss:2,x:583,y:1},real:true},{frame:{ss:0,x:675,y:1859},real:true},{frame:{ss:2,x:1359,y:1},real:true},{frame:{ss:2,x:1747,y:1},real:true}], specwalls:[{frame:{ss:2,x:195,y:777},real:true},{frame:{ss:2,x:195,y:1165},real:true},{frame:{ss:0,x:413,y:1},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}] },
    7: { cells:[{frame:{ss:2,x:1,y:583},real:true},{frame:{ss:2,x:195,y:1359},real:true},{frame:{ss:2,x:389,y:195},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}], decals:[{frame:{ss:2,x:195,y:1},real:true},{frame:{ss:2,x:583,y:1},real:true},{frame:{ss:2,x:971,y:1},real:true},{frame:{ss:2,x:1359,y:1},real:true},{frame:{ss:2,x:1747,y:1},real:true}], specwalls:[{frame:{ss:2,x:195,y:777},real:true},{frame:{ss:2,x:195,y:1165},real:true},{frame:{ss:0,x:413,y:1},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}] },
    8: { cells:[{frame:{ss:2,x:1,y:583},real:true},{frame:{ss:2,x:195,y:1359},real:true},{frame:{ss:2,x:389,y:195},real:true},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}], decals:[{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}], specwalls:[{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false},{frame:{ss:0,x:195,y:1353},real:false}] },
};
