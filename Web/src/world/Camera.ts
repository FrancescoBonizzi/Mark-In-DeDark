import {Application, Container, Renderer} from "pixi.js";

class Camera {

    private readonly _worldHeight: number;

    private readonly _world: Container;
    private readonly _width: number;
    private readonly _height: number;

    private _shaking = false;
    private _shakeIntensity = 0;
    private _shakeAngle = 0;
    private _shakeOffsetX = 0;
    private _shakeOffsetY = 0;

    constructor(app: Application<Renderer>, worldHeight: number = 1500) {
        this._worldHeight = worldHeight;

        this._world = new Container();
        this._world.height = this._worldHeight;

        this._width = app.screen.width;
        this._height = app.screen.height;

        this._world.x = 0;
        this._world.y = app.screen.height;
        this._world.pivot.x = 0;
        this._world.pivot.y = this._world.height;
        this.setZoom(0.8);

        app.stage.addChild(this._world);
    }

    get worldHeight() {
        return this._worldHeight;
    }

    /** Exposes the internal world Container for ShadowSystem transform mirroring. */
    get worldContainer(): Container {
        return this._world;
    }

    get width() {
        return this._width / this._world.scale.x;
    }

    get height() {
        return this._height / this._world.scale.y;
    }

    get x() {
        return -this._world.x / this._world.scale.x;
    }

    get y() {
        return (-this._world.y + this._height) / this._world.scale.y;
    }

    set x(x: number) {
        this._world.x = -x * this._world.scale.x;
    }

    set y(y: number) {
        this._world.y = -y * this._world.scale.y + this._height;
    }

    addToWorld(child: Container) {
        this._world.addChild(child);
    }

    removeFromWorld(child: Container) {
        this._world.removeChild(child);
    }

    private worldToScreenX(worldX: number) {
        return (worldX - this.x) * this._world.scale.x;
    }

    isOutOfCameraLeft(sprite: { x: number, width: number }) {
        const globalX = this.worldToScreenX(sprite.x);
        return globalX + sprite.width <= 0;
    }

    getZoom() {
        return this._world.scale.x;
    }

    setZoom(zoom: number) {
        this._world.scale.set(zoom);
    }

    setZoomAround(z: number, focusX: number, focusY: number) {
        const s0 = this._world.scale.x, s1 = z;
        if (s0 === s1) return;
        const dx = (focusX - this._world.pivot.x) * (s0 - s1);
        const dy = (focusY - this._world.pivot.y) * (s0 - s1);
        this._world.scale.set(s1);
        this._world.position.set(this._world.position.x + dx, this._world.position.y + dy);
    }

    startShake(intensity: number): void {
        this._shaking = true;
        this._shakeIntensity = intensity;
        this._shakeAngle = Math.PI;
    }

    updateShake(): void {
        // Undo previous frame's offset
        this._world.x -= this._shakeOffsetX;
        this._world.y -= this._shakeOffsetY;
        if (!this._shaking) return;

        const scale = this._world.scale.x;
        this._shakeOffsetX = Math.sin(this._shakeAngle) * this._shakeIntensity * scale;
        this._shakeOffsetY = Math.cos(this._shakeAngle) * this._shakeIntensity * scale;
        this._world.x += this._shakeOffsetX;
        this._world.y += this._shakeOffsetY;

        this._shakeIntensity -= 0.25;
        this._shakeAngle += 150 + Math.random() * 60;
        if (this._shakeIntensity <= 0) {
            this._shaking = false;
            this._shakeIntensity = 0;
            this._shakeOffsetX = 0;
            this._shakeOffsetY = 0;
        }
    }
}

export default Camera;
