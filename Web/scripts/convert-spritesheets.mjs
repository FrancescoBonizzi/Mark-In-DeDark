#!/usr/bin/env node

/**
 * Converts C# spritesheet .txt metadata files to PixiJS-compatible JSON format.
 * Also copies PNG atlases, menu images, and level files to Web/public/assets/.
 */

import { readFileSync, writeFileSync, copyFileSync, existsSync } from 'fs';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = resolve(__dirname, '..', '..');
const CSHARP_ASSETS = resolve(ROOT, 'Csharp', 'MarkInDeDark.Game', 'Assets');
const CONTENT_BUILDER = resolve(CSHARP_ASSETS, 'ContentBuilder');
const PUBLIC_ASSETS = resolve(__dirname, '..', 'public', 'assets');

const SPRITESHEET_COUNT = 6;
const ATLAS_SIZE = 2048;

// --- Step 1: Convert spritesheets ---

for (let i = 0; i < SPRITESHEET_COUNT; i++) {
    const txtPath = resolve(CSHARP_ASSETS, `Spritesheet-${i}.txt`);
    const pngSrc = resolve(CONTENT_BUILDER, `Spritesheet-${i}.png`);
    const jsonDst = resolve(PUBLIC_ASSETS, 'images', 'spriteSheets', `Spritesheet-${i}.json`);
    const pngDst = resolve(PUBLIC_ASSETS, 'images', 'spriteSheets', `Spritesheet-${i}.png`);

    if (!existsSync(txtPath)) {
        console.error(`Missing: ${txtPath}`);
        process.exit(1);
    }

    const text = readFileSync(txtPath, 'utf-8');
    const lines = text.split('\n').filter(line => line.trim().length > 0);

    const frames = {};
    for (const line of lines) {
        const parts = line.split('|');
        if (parts.length < 5) continue;

        const [name, xStr, yStr, wStr, hStr] = parts;
        const x = parseInt(xStr, 10);
        const y = parseInt(yStr, 10);
        const w = parseInt(wStr, 10);
        const h = parseInt(hStr, 10);

        if (isNaN(x) || isNaN(y) || isNaN(w) || isNaN(h)) continue;

        frames[name] = {
            frame: { x, y, w, h },
        };
    }

    const json = {
        frames,
        meta: {
            image: `Spritesheet-${i}.png`,
            format: 'RGBA8888',
            size: { w: ATLAS_SIZE, h: ATLAS_SIZE },
            scale: 1,
        },
    };

    writeFileSync(jsonDst, JSON.stringify(json, null, 2));
    console.log(`Created: Spritesheet-${i}.json (${Object.keys(frames).length} frames)`);

    if (existsSync(pngSrc)) {
        copyFileSync(pngSrc, pngDst);
        console.log(`Copied:  Spritesheet-${i}.png`);
    } else {
        console.warn(`WARNING: PNG not found: ${pngSrc}`);
    }
}

// --- Step 2: Copy menu images ---

const menuImages = ['mainpage.png', 'levels.png', 'logo.png'];
for (const img of menuImages) {
    const src = resolve(CONTENT_BUILDER, img);
    const dst = resolve(PUBLIC_ASSETS, 'images', 'menu', img);
    if (existsSync(src)) {
        copyFileSync(src, dst);
        console.log(`Copied:  menu/${img}`);
    } else {
        console.warn(`WARNING: Menu image not found: ${src}`);
    }
}

// --- Step 3: Copy level files ---

const LEVELS_SRC = resolve(CSHARP_ASSETS, 'Levels');
for (let i = 1; i <= 8; i++) {
    const src = resolve(LEVELS_SRC, `Livello ${i}.mark`);
    const dst = resolve(PUBLIC_ASSETS, 'levels', `${i}.mark`);
    if (existsSync(src)) {
        copyFileSync(src, dst);
        console.log(`Copied:  levels/${i}.mark`);
    } else {
        console.warn(`WARNING: Level file not found: ${src}`);
    }
}

// --- Step 4: Copy sounds (WAV → just copy for now, conversion to MP3 TODO) ---

const soundEffects = ['TaurusStartingCharge.wav', 'PatrolSeenYou.wav'];
for (const snd of soundEffects) {
    const src = resolve(CONTENT_BUILDER, 'Soundtrack', snd);
    const dst = resolve(PUBLIC_ASSETS, 'sounds', 'effects', snd);
    if (existsSync(src)) {
        copyFileSync(src, dst);
        console.log(`Copied:  sounds/effects/${snd}`);
    } else {
        console.warn(`WARNING: Sound not found: ${src}`);
    }
}

console.log('\nDone! Asset pipeline conversion complete.');
