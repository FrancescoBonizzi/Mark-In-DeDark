/**
 * Game-level light source. Animates color between color1 and color2 with a
 * linear ping-pong cycle (port of C# ColorChangingObject + MarkLight).
 */
// Task 1 parity: preserve C# light semantics for glow masks and per-pass rendering behaviour.
export class MarkLight {
  position: { x: number; y: number };
  color1: number;
  color2: number;
  currentColor: number;
  opacity: number;
  /** World-space illumination radius in pixels. */
  radius: number;
  castsShadows: boolean;
  enabled: boolean;
  /** Height above ground plane — used by normal mapping (Task 5). */
  z: number;
  /** Radius of the physical light source — controls penumbra softness. */
  sourceRadius: number;

  /** World-space position offset applied when computing the light for ground tiles. */
  deltaForGround: { x: number; y: number } = { x: 0, y: 100 };
  /** World-space position offset applied when computing the light for wall/pillar tiles. */
  deltaForWall: { x: number; y: number } = { x: 0, y: -60 };
  /** Whether this light draws a glow sprite on the specular lightmask and the shadow lightmap (C# IsGlowEnabled). */
  isGlowEnabled = true;
  /** Whether this light contributes to the additional glow mask composited on the shadow lightmap. */
  drawInAdditionalLightMask = true;
  /** Scale of the glow sprite for this light (C# _lightMaskDrawingInfos.Scale = 2). */
  lightMaskScale = 2.0;

  private _t = 0;
  private _direction = 1;
  /** Full ping-pong cycle duration in ms. */
  private readonly _period = 3000;
  private readonly _distanceSquared: number;
  private readonly _distanceSquaredForWall: number;
  private _originalColor2: number;

  constructor(
    options: {
      x?: number;
      y?: number;
      color1?: number;
      color2?: number;
      opacity?: number;
      radius?: number;
      castsShadows?: boolean;
      z?: number;
      sourceRadius?: number;
      distanceSquared?: number;
      distanceSquaredForWall?: number;
    } = {},
  ) {
    this.position = { x: options.x ?? 0, y: options.y ?? 0 };
    this.color1 = options.color1 ?? 0xffaa44;
    this.color2 = options.color2 ?? 0xff6600;
    this._originalColor2 = this.color2;
    this.currentColor = this.color1;
    this.opacity = options.opacity ?? 1;
    this.radius = options.radius ?? 800;
    this.castsShadows = options.castsShadows ?? true;
    this.enabled = true;
    this.z = options.z ?? 90;
    this.sourceRadius = options.sourceRadius ?? 20;
    this._distanceSquared = options.distanceSquared ?? 20_600;
    this._distanceSquaredForWall =
      options.distanceSquaredForWall ?? this._distanceSquared;
  }

  update(deltaMS: number): void {
    this._t += deltaMS * this._direction;
    if (this._t >= this._period) {
      this._t = this._period;
      this._direction = -1;
    } else if (this._t <= 0) {
      this._t = 0;
      this._direction = 1;
    }
    this.currentColor = lerpColor(
      this.color1,
      this.color2,
      this._t / this._period,
    );
  }

  /** Distance squared for ground normal map pass. */
  get distanceSquaredForGround(): number {
    return this._distanceSquared;
  }

  /** Distance squared for wall/pillar normal map pass. */
  get distanceSquaredForWall(): number {
    return this._distanceSquaredForWall;
  }

  /** Current light colour as linear RGB [0-1] triple for the normal map shader. */
  get colorNormalVec3(): { r: number; g: number; b: number } {
    const c = this.currentColor;
    return {
      r: ((c >> 16) & 0xff) / 255,
      g: ((c >> 8) & 0xff) / 255,
      b: (c & 0xff) / 255,
    };
  }

  /** Sets a temporary color2 (used for Skeleton rage / Taurus charge tinting). */
  setTemporaryColor(color: number): void {
    this.color2 = color;
  }

  /** Restores the original color2. */
  resetColor(): void {
    this.color2 = this._originalColor2;
  }

  /** World-space position with the penumbra offset applied (C# getPenumbraPosition). */
  getPenumbraPosition(): { x: number; y: number } {
    return { x: this.position.x - 10, y: this.position.y + 75 };
  }
}

function lerpColor(c1: number, c2: number, t: number): number {
  const r1 = (c1 >> 16) & 0xff;
  const g1 = (c1 >> 8) & 0xff;
  const b1 = c1 & 0xff;
  const r2 = (c2 >> 16) & 0xff;
  const g2 = (c2 >> 8) & 0xff;
  const b2 = c2 & 0xff;
  return (
    (Math.round(r1 + (r2 - r1) * t) << 16) |
    (Math.round(g1 + (g2 - g1) * t) << 8) |
    Math.round(b1 + (b2 - b1) * t)
  );
}
