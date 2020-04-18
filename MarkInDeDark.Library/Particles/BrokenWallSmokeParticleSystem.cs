using FbonizziMonoGame.Particles;

using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class BrokenWallSmokeParticleSystem : ParticleGenerator
    {
        public BrokenWallSmokeParticleSystem(
            AssetsLoader assetsLoader)
            : base(
                  assetsLoader.Sprites["Effects/smoke"],
                  density: 5,
                  minNumParticles: 4,
                  maxNumParticles: 8,
                  minInitialSpeed: 20f,
                  maxInitialSpeed: 70f,
                  minAcceleration: 30f,
                  maxAcceleration: 50f,
                  minRotationSpeed: -MathHelper.Pi,
                  maxRotationSpeed: MathHelper.Pi,
                  minLifetime: TimeSpan.FromMilliseconds(1000),
                  maxLifetime: TimeSpan.FromMilliseconds(1600),
                  minScale: 2.0f,
                  maxScale: 3.0f,
                  minSpawnAngle: 0.0f,
                  maxSpawnAngle: 360.0f)
        { }
    }
}
