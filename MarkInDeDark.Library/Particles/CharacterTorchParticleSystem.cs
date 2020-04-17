using FbonizziMonoGame.Particles;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class CharacterTorchParticleSystem : ParticleGenerator
    {
        public CharacterTorchParticleSystem(
            AssetsLoader assetsLoader,
            Color primaryColor,
            Color secondaryColor)
            : base(
                  assetsLoader.Sprites["generic_spark"],
                  density: 2,
                  minNumParticles: 5,
                  maxNumParticles: 6,
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
            PrimaryParticleOverlayColor = primaryColor;
            SecondaryParticleOverlayColor = secondaryColor;
        }
    }
}
