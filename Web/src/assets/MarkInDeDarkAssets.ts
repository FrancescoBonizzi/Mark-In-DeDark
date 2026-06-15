import { Spritesheet, Texture } from 'pixi.js';

export default interface MarkInDeDarkAssets {
    fontName: string;
    spriteSheets: Spritesheet[];
    menuImages: {
        mainpage: Texture;
        levels: Texture;
        logo: Texture;
    };
}
