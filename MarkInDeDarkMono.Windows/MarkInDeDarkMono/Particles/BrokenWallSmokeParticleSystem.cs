using FlowsoftGamesMonogame.Particles;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDarkMono.Particles
{
    public class BrokenWallSmokeParticleSystem : ParticleGenerator
    {
        public BrokenWallSmokeParticleSystem(
            AssetsLoader assetsLoader)
            : base(assetsLoader.Sprites["smoke"]) { }

        protected override int Density => 5;

        protected override int MinNumParticles => 4;
        protected override int MaxNumParticles => 8;

        protected override float MinInitialSpeed => 20.0f;
        protected override float MaxInitialSpeed => 70.0f;

        protected override float MinAcceleration => 30.0f;
        protected override float MaxAcceleration => 50.0f;

        protected override float MinRotationSpeed => -MathHelper.Pi;
        protected override float MaxRotationSpeed => MathHelper.Pi;

        protected override TimeSpan MinLifetime => TimeSpan.FromMilliseconds(1000);
        protected override TimeSpan MaxLifetime => TimeSpan.FromMilliseconds(1600);

        protected override float MinScale => 2.0f;
        protected override float MaxScale => 3.0f;

        protected override float MinSpawnAngle => 0.0f;
        protected override float MaxSpawnAngle => 360.0f;
    }
}
