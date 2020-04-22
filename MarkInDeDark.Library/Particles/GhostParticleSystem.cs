using FbonizziMonoGame.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkInDeDark.Library.Particles
{
    public class GhostParticleSystem : ParticleGenerator
    {
        public GhostParticleSystem(
            AssetsLoader assetsLoader,
            Color colorA,
            Color colorB)
            : base(
                  assetsLoader.Sprites["Effects/generic_spark"],
                  density: 10,
                  minNumParticles: 20,
                  maxNumParticles: 20,
                  minInitialSpeed: 39f,
                  maxInitialSpeed: 93f,
                  minAcceleration: 7f,
                  maxAcceleration: 15f,
                  minRotationSpeed: -0.26f,
                  maxRotationSpeed: 0f,
                  minLifetime: TimeSpan.FromMilliseconds(600),
                  maxLifetime: TimeSpan.FromMilliseconds(1500),
                  minScale: 1f,
                  maxScale: 1f,
                  minSpawnAngle: -45f,
                  maxSpawnAngle: -235f)
        {
            PrimaryParticleOverlayColor = colorA;
            SecondaryParticleOverlayColor = colorB;
        }
    }
}
