import { Assets, Spritesheet, Texture } from 'pixi.js';
import { AssetsRoot } from './constants';
import type MarkInDeDarkAssets from './MarkInDeDarkAssets';

const imagesRoot = `${AssetsRoot}assets/images`;
const spriteSheetsRoot = `${imagesRoot}/spriteSheets`;

export const loadAssets = async (): Promise<MarkInDeDarkAssets> => {
    const sheetPromises = Array.from({ length: 6 }, (_, i) =>
        Assets.load<Spritesheet>(`${spriteSheetsRoot}/Spritesheet-${i}.json`)
    );
    const spriteSheets = await Promise.all(sheetPromises);

    const [mainpage, levels, logo] = await Promise.all([
        Assets.load<Texture>(`${imagesRoot}/menu/mainpage.png`),
        Assets.load<Texture>(`${imagesRoot}/menu/levels.png`),
        Assets.load<Texture>(`${imagesRoot}/menu/logo.png`),
    ]);

    return {
        fontName: '',
        spriteSheets,
        menuImages: { mainpage, levels, logo },
    };
};

export const getTexture = (sheets: Spritesheet[], name: string): Texture => {
    for (const sheet of sheets) {
        const tex = sheet.textures[name];
        if (tex) return tex;
    }
    throw new Error(`Texture "${name}" not found in any spritesheet`);
};
