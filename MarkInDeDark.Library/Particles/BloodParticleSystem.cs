using FbonizziMonoGame.Particles;

using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class BloodParticleSystem : ParticleGenerator
    {
        public BloodParticleSystem(
            AssetsLoader assetsLoader)
            : base(
                  assetsLoader.Sprites["blood"],
                  density: 5,
                  minNumParticles: 6,
                  maxNumParticles: 10,
                  minInitialSpeed: 120f,
                  maxInitialSpeed: 150f,
                  minAcceleration: 30f,
                  maxAcceleration: 45f,
                  minRotationSpeed: -MathHelper.PiOver2,
                  maxRotationSpeed: MathHelper.PiOver2,
                  minLifetime: TimeSpan.FromMilliseconds(1200),
                  maxLifetime: TimeSpan.FromMilliseconds(1800),
                  minScale: 1.0f,
                  maxScale: 1.6f,
                  minSpawnAngle: 0.0f,
                  maxSpawnAngle: 360.0f)
        { }
    }
}
