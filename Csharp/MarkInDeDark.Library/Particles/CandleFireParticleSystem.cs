using FbonizziMonoGame.Particles;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class CandleFireParticleSystem : ParticleGenerator
    {
        public CandleFireParticleSystem(AssetsLoader assetsLoader)
            : base(
                  assetsLoader.Sprites["Effects/particle"],
                  density: 1,
                  minNumParticles: 2,
                  maxNumParticles: 3,
                  minInitialSpeed: 50f,
                  maxInitialSpeed: 106f,
                  minAcceleration: 1f,
                  maxAcceleration: 10f,
                  minRotationSpeed: -0.8f,
                  maxRotationSpeed: 0.2f,
                  minLifetime: TimeSpan.FromMilliseconds(250),
                  maxLifetime: TimeSpan.FromMilliseconds(800),
                  minScale: 0.1f,
                  maxScale: 0.4f,
                  minSpawnAngle: -264f,
                  maxSpawnAngle: -300f)
        {
            PrimaryParticleOverlayColor = new Color(255, 204, 0);
            SecondaryParticleOverlayColor = new Color(255, 84, 0);
        }
    }
}
