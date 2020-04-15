using FbonizziMonoGame.Particles;

using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDarkMono.Particles
{
    public class GameStartSparkleParticleSystem : ParticleGenerator
    {
        public GameStartSparkleParticleSystem(AssetsLoader assetsLoader)
            : base(
                  assetsLoader.Sprites["scintilla"],
                  density: 2,
                  minNumParticles: 4,
                  maxNumParticles: 8,
                  minInitialSpeed: 20f,
                  maxInitialSpeed: 35f,
                  minAcceleration: 20f,
                  maxAcceleration: 35f,
                  minRotationSpeed: (-MathHelper.PiOver2) / 2,
                  maxRotationSpeed: (MathHelper.PiOver2) / 2,
                  minLifetime: TimeSpan.FromMilliseconds(200),
                  maxLifetime: TimeSpan.FromMilliseconds(600),
                  minScale: 0.2f,
                  maxScale: 2.0f,
                  minSpawnAngle: 0.0f,
                  maxSpawnAngle: 360.0f) { } // TODO è molto importante anche per altre cose... tipo il fumo della cendela potrebbe fare ombra nera!!! 
            // e anche la magia del wolf, AssetsLoader.scintilla_lightmap_)
    }
}
