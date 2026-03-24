import { Rectangle } from 'pixi.js';
import { WallEsteticDoor } from '../grid/WallEsteticDoor';

/** The level exit zone (port of C# Exit.cs). */
export class Exit {
    readonly collisionRectangle: Rectangle;

    private readonly _exitDoor: WallEsteticDoor | null;
    private _isOpen = false;

    constructor(
        centerX: number,
        centerY: number,
        exitDoor: WallEsteticDoor | null,
    ) {
        this._exitDoor = exitDoor;
        // HitBoxTolerance(60, 60, 120, 120)
        this.collisionRectangle = new Rectangle(centerX - 60, centerY - 60, 120, 120);
    }

    get isOpen(): boolean { return this._isOpen; }

    setOpen(): void {
        if (this._isOpen) return;
        this._isOpen = true;
        this._exitDoor?.setOpen();
    }
}
