import { Texture } from "pixi.js";
import Camera from "../world/Camera.ts";
import ParticleSystem from "./ParticleSystem.ts";

/**
 * Small fire-crackle particle system for the player torch (port of C# CandleFireParticleSystem).
 * Spawns 2–3 upward-moving flame particles per call, blended additively for a glow effect.
 */
export class TorchFireParticleSystem extends ParticleSystem {
    constructor(texture: Texture, camera: Camera) {
        super(
            texture,
            camera,
            /* density */          1,
            /* numParticles */     { min: 2, max: 3 },
            /* speed px/s */       { min: 50, max: 106 },
            /* acceleration px/s² */{ min: 1, max: 10 },
            /* rotationSpeed r/s */{  min: -0.8, max: 0.2 },
            /* lifetimeSeconds */  { min: 0.25, max: 0.8 },
            /* scale */            { min: 0.1, max: 0.4 },
            /* spawnAngle degrees */{ min: -300, max: -264 }, // upward arc
            /* blendMode */        "add",
            /* randomizedAngle */  false,
            /* perspectiveEffect */null,
            /* colors */           ["#FFCC00", "#FF5400"],
        );
    }
}
