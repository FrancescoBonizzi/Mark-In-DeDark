import { Container, Rectangle, Sprite, Texture } from 'pixi.js';
import { LinkDirection } from './enums';
import type { IWall } from './IWall';
import type { CollisionOffset } from './LevelCustomization';
import { zForWall, zForWallDecal } from './ZIndexCalculator';

export class WallFixed implements IWall {
    readonly direction: LinkDirection;
    readonly collisionRectangle: Rectangle;
    readonly position: { x: number; y: number };
    readonly centerPosition: { x: number; y: number };
    readonly hullRects: Rectangle[] = [];

    private readonly _sprite: Sprite;
    private readonly _normalSprite: Sprite | null = null;
    private readonly _decalSprite: Sprite | null = null;
    private readonly _decalNormalSprite: Sprite | null = null;

    constructor(
        direction: LinkDirection,
        spriteX: number,
        spriteY: number,
        texture: Texture,
        offset: CollisionOffset,
        decalTexture?: Texture,
        normalTexture?: Texture,
        decalNormalTexture?: Texture,
    ) {
        this.direction = direction;
        this._sprite = new Sprite(texture);
        this._sprite.x = spriteX;
        this._sprite.y = spriteY;
        const wallZ = zForWall(spriteY + texture.height / 2);
        this._sprite.zIndex = wallZ;

        if (normalTexture) {
            this._normalSprite = new Sprite(normalTexture);
            this._normalSprite.x = spriteX;
            this._normalSprite.y = spriteY;
            this._normalSprite.zIndex = wallZ;
        }

        this.position = { x: spriteX, y: spriteY };
        this.centerPosition = {
            x: spriteX + texture.width / 2,
            y: spriteY + texture.height / 2,
        };

        this.collisionRectangle = new Rectangle(
            spriteX + offset.x,
            spriteY + offset.y,
            offset.w,
            offset.h,
        );

        // Bug fix: usa collisionRectangle invece del bbox del sprite (che per muri NorthLink
        // si estende verso il basso coprendo il pavimento del corridoio adiacente, blackenandolo)
        this.hullRects = [this.collisionRectangle];

        if (decalTexture) {
            this._decalSprite = new Sprite(decalTexture);
            this._decalSprite.x = spriteX + texture.width / 2 - decalTexture.width / 2;
            this._decalSprite.y = spriteY + texture.height / 2 - decalTexture.height / 2;
            this._decalSprite.zIndex = zForWallDecal(wallZ);
        }

        if (decalNormalTexture) {
            this._decalNormalSprite = new Sprite(decalNormalTexture);
            this._decalNormalSprite.x = spriteX + texture.width / 2 - decalNormalTexture.width / 2;
            this._decalNormalSprite.y = spriteY + texture.height / 2 - decalNormalTexture.height / 2;
            this._decalNormalSprite.zIndex = zForWallDecal(wallZ);
        }
    }

    update(_deltaMS: number): void { /* static wall */ }

    addToContainer(container: Container): void {
        container.addChild(this._sprite);
        if (this._decalSprite) container.addChild(this._decalSprite);
    }

    addNormalToContainer(container: Container): void {
        if (this._normalSprite) container.addChild(this._normalSprite);
        if (this._decalNormalSprite) container.addChild(this._decalNormalSprite);
    }

    removeFromContainer(container: Container): void {
        container.removeChild(this._sprite);
        if (this._decalSprite) container.removeChild(this._decalSprite);
    }
}
