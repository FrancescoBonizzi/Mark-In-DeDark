import { Sprite, Texture } from "pixi.js";
import Numbers from "../services/Numbers";
import Camera from "../world/Camera";

interface SnowflakeParticle {
  sprite: Sprite;
  velocityX: number;
  velocityY: number;
  removed: boolean;
}

class SnowingLayer {
  private readonly _camera: Camera;
  private readonly _snowflakes: SnowflakeParticle[] = [];
  private readonly _heightOffset: number;
  private readonly _texture: Texture;
  private _particleElapsedMS = 0;
  private _windElapsedMS = 0;

  constructor(texture: Texture, camera: Camera, heightOffset: number) {
    this._texture = texture;
    this._camera = camera;
    this._heightOffset = heightOffset;
  }

  update(deltaMS: number): void {
    this._particleElapsedMS += deltaMS;
    this._windElapsedMS += deltaMS;

    if (this._particleElapsedMS >= 1000) {
      this._particleElapsedMS = 0;
      if (this._snowflakes.length < 5) {
        this._snowflakes.push(this._createParticle());
      }
    }

    if (this._windElapsedMS >= 4000) {
      this._windElapsedMS = 0;
      const sway = Numbers.randomBetween(-15, 15);
      for (const particle of this._snowflakes) {
        particle.velocityX += sway;
      }
    }

    for (const particle of this._snowflakes) {
      if (!this._isOnScreen(particle)) {
        particle.removed = true;
      }

      particle.sprite.x += particle.velocityX * (deltaMS / 1000);
      particle.sprite.y += particle.velocityY * (deltaMS / 1000);
    }

    for (let index = this._snowflakes.length - 1; index >= 0; index--) {
      if (!this._snowflakes[index]!.removed) continue;
      const particle = this._snowflakes[index]!;
      this._camera.removeFromWorld(particle.sprite);
      particle.sprite.destroy();
      this._snowflakes.splice(index, 1);
    }
  }

  destroy(): void {
    for (const particle of this._snowflakes) {
      this._camera.removeFromWorld(particle.sprite);
      particle.sprite.destroy();
    }
    this._snowflakes.length = 0;
  }

  private _createParticle(): SnowflakeParticle {
    const sprite = new Sprite(this._texture);
    sprite.anchor.set(0.5);
    sprite.alpha = Numbers.randomBetween(0, 1);
    sprite.x = Numbers.randomBetween(
      this._camera.x,
      this._camera.x + this._camera.width,
    );
    sprite.y = this._camera.y;
    sprite.scale.set(Numbers.randomBetween(0.8, 1.5));
    this._camera.addToWorld(sprite);

    return {
      sprite,
      velocityX: 0,
      velocityY: Numbers.randomBetween(40, 45),
      removed: false,
    };
  }

  private _isOnScreen(particle: SnowflakeParticle): boolean {
    return (
      particle.sprite.y >= this._camera.y &&
      particle.sprite.y <=
        this._camera.y + this._camera.height + this._heightOffset &&
      particle.sprite.x >= this._camera.x &&
      particle.sprite.x <= this._camera.x + this._camera.width
    );
  }
}

export class SnowingManager {
  private readonly _layers: SnowingLayer[];

  constructor(texture: Texture, camera: Camera) {
    this._layers = [
      new SnowingLayer(texture, camera, 0),
      new SnowingLayer(texture, camera, -250),
      new SnowingLayer(texture, camera, -400),
    ];
  }

  update(deltaMS: number): void {
    for (const layer of this._layers) {
      layer.update(deltaMS);
    }
  }

  destroy(): void {
    for (const layer of this._layers) {
      layer.destroy();
    }
  }
}
