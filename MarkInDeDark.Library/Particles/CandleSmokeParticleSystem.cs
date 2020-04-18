using FbonizziMonoGame.Particles;

using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class CandleSmokeParticleSystem : ParticleGenerator
    {
        public CandleSmokeParticleSystem(
            AssetsLoader assetsLoader)
            : base(
                  assetsLoader.Sprites["Effects/smoke"],
                  density: 5,
                  minNumParticles: 5,
                  maxNumParticles: 8,
                  minInitialSpeed: 30f,
                  maxInitialSpeed: 60f,
                  minAcceleration: 30f,
                  maxAcceleration: 50f,
                  minRotationSpeed: (-MathHelper.PiOver4) / 2f,
                  maxRotationSpeed: (MathHelper.PiOver4) / 2f,
                  minLifetime: TimeSpan.FromMilliseconds(1500),
                  maxLifetime: TimeSpan.FromMilliseconds(2000),
                  minScale: 1.0f,
                  maxScale: 1.5f,
                  minSpawnAngle: 89.0f,
                  maxSpawnAngle: 91.0f)
        { }
    }
}
