using FbonizziMonoGame.Particles;

using System;

namespace MarkInDeDarkMono.Particles
{
    public class GoblinLightParticleSystem : ParticleGenerator
    {
        public GoblinLightParticleSystem(AssetsLoader assetsLoader)
            : base(
                  assetsLoader.Sprites["goblin_light_spark"],
                  density: 1,
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
            { }
    }
}
