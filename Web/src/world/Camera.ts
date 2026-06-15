import { Application, Container, Renderer } from "pixi.js";

class Camera {
  private readonly _worldHeight: number;

  private readonly _world: Container;
  private readonly _width: number;
  private readonly _height: number;

  private _cameraX = 0;
  private _cameraY = 0;
  private _zoom = 1;

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

    this._world.pivot.x = 0;
    this._world.pivot.y = this._world.height;
    // C# virtual resolution: 1920×1080 at zoom 0.8 → visible area 2400×1350 world units.
    // PixiJS canvas: 1280×720 → to show same world area: zoom = 1280/2400 = 8/15 ≈ 0.5333.
    this.setZoom(8 / 15);

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
    return this._width / this._zoom;
  }

  get height() {
    return this._height / this._zoom;
  }

  get x() {
    return this._cameraX;
  }

  get y() {
    return this._cameraY;
  }

  set x(x: number) {
    this._cameraX = x;
    this._applyTransform();
  }

  set y(y: number) {
    this._cameraY = y;
    this._applyTransform();
  }

  addToWorld(child: Container) {
    this._world.addChild(child);
  }

  removeFromWorld(child: Container) {
    this._world.removeChild(child);
  }

  private worldToScreenX(worldX: number) {
    return (worldX - this.x) * this._zoom;
  }

  isOutOfCameraLeft(sprite: { x: number; width: number }) {
    const globalX = this.worldToScreenX(sprite.x);
    return globalX + sprite.width <= 0;
  }

  getZoom() {
    return this._zoom;
  }

  setZoom(zoom: number) {
    this._zoom = zoom;
    this._world.scale.set(zoom);
    this._applyTransform();
  }

  setZoomAround(z: number, focusX: number, focusY: number) {
    const s0 = this._zoom,
      s1 = z;
    if (s0 === s1) return;
    const dx = (focusX - this._world.pivot.x) * (s0 - s1);
    const dy = (focusY - this._world.pivot.y) * (s0 - s1);
    const worldX = this._world.position.x + dx;
    const worldY = this._world.position.y + dy;
    this._zoom = s1;
    this._world.scale.set(s1);
    this._cameraX = -worldX / s1;
    this._cameraY = (-worldY + this._height) / s1;
    this._applyTransform();
  }

  startShake(intensity: number): void {
    this._shaking = true;
    this._shakeIntensity = intensity;
    this._shakeAngle = Math.PI;
  }

  updateShake(): void {
    if (!this._shaking) {
      if (this._shakeOffsetX !== 0 || this._shakeOffsetY !== 0) {
        this._shakeOffsetX = 0;
        this._shakeOffsetY = 0;
        this._applyTransform();
      }
      return;
    }

    const scale = this._zoom;
    this._shakeOffsetX =
      Math.sin(this._shakeAngle) * this._shakeIntensity * scale;
    this._shakeOffsetY =
      Math.cos(this._shakeAngle) * this._shakeIntensity * scale;

    this._shakeIntensity -= 0.25;
    this._shakeAngle += 150 + Math.random() * 60;
    if (this._shakeIntensity <= 0) {
      this._shaking = false;
      this._shakeIntensity = 0;
      this._shakeOffsetX = 0;
      this._shakeOffsetY = 0;
    }

    this._applyTransform();
  }

  private _applyTransform(): void {
    const screenX = -this._cameraX * this._zoom + this._shakeOffsetX;
    const screenY =
      -this._cameraY * this._zoom + this._height + this._shakeOffsetY;

    this._world.x = Math.round(screenX);
    this._world.y = Math.round(screenY);
  }
}

export default Camera;
