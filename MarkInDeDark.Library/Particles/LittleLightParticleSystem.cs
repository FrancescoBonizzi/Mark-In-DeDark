using FbonizziMonoGame.Particles;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class LittleLightParticleSystem : ParticleGenerator
    {
        public LittleLightParticleSystem(AssetsLoader assetsLoader)
            : base(
                  assetsLoader.Sprites["Effects/particle"],
                  density: 1,
                  minNumParticles: 9,
                  maxNumParticles: 10,
                  minInitialSpeed: 70f,
                  maxInitialSpeed: 106f,
                  minAcceleration: 7f,
                  maxAcceleration: 10f,
                  minRotationSpeed: 1.1f,
                  maxRotationSpeed: 1.6f,
                  minLifetime: TimeSpan.FromMilliseconds(250),
                  maxLifetime: TimeSpan.FromMilliseconds(800),
                  minScale: 0.1f,
                  maxScale: 0.3f,
                  minSpawnAngle: -264f,
                  maxSpawnAngle: -300f)
        {
            PrimaryParticleOverlayColor = new Color(0, 234, 255);
            SecondaryParticleOverlayColor = new Color(0, 255, 210);
        }
    }
}
