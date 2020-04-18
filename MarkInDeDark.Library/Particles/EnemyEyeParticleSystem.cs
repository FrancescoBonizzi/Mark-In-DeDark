using FbonizziMonoGame.Particles;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class EnemyEyeParticleSystem : ParticleGenerator
    {
        public EnemyEyeParticleSystem(
            AssetsLoader assetsLoader,
            Color primaryColor,
            Color secondaryColor)
            : base(
                  assetsLoader.Sprites["Effects/generic_spark"],
                  density: 4,
                  minNumParticles: 3,
                  maxNumParticles: 4,
                  minInitialSpeed: 80f,
                  maxInitialSpeed: 100f,
                  minAcceleration: 1f,
                  maxAcceleration: 6f,
                  minRotationSpeed: -1.5f,
                  maxRotationSpeed: 0f,
                  minLifetime: TimeSpan.FromMilliseconds(100),
                  maxLifetime: TimeSpan.FromMilliseconds(200),
                  minScale: 0.1f,
                  maxScale: 0.2f,
                  minSpawnAngle: -265f,
                  maxSpawnAngle: -270f)
        {
            PrimaryParticleOverlayColor = primaryColor;
            SecondaryParticleOverlayColor = secondaryColor;
        }
    }
}
