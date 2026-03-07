using System;
using FbonizziMonoGame.Particles;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Particles
{
    public class BodyPartBloodParticleSystem : ParticleGenerator
    {
        public BodyPartBloodParticleSystem(Sprite bloodParticle)
            : base(
                bloodParticle,
                density: 1,
                minNumParticles: 6,
                maxNumParticles: 7,
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
                minSpawnAngle: 0f,
                maxSpawnAngle: 360f
            )
        {
            LayerDepth = ZIndexCalculator.OverAllLayerDepth;
            PrimaryParticleOverlayColor = Color.Red;
            SecondaryParticleOverlayColor = Color.DarkRed;
        }
    }
}
