using FlowsoftGamesMonogame.Particles;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDarkMono.Particles
{
    public class CandleSmokeParticleSystem : ParticleGenerator
    {
        public CandleSmokeParticleSystem(
            AssetsLoader assetsLoader)
            : base(assetsLoader.Sprites["smoke"]) { }

        protected override int Density => 5;

        protected override int MinNumParticles => 5;
        protected override int MaxNumParticles => 8;

        protected override float MinInitialSpeed => 30.0f;
        protected override float MaxInitialSpeed => 60.0f;

        protected override float MinAcceleration => 30.0f;
        protected override float MaxAcceleration => 50.0f;

        protected override float MinRotationSpeed => -MathHelper.PiOver4 / 2.0f;
        protected override float MaxRotationSpeed => MathHelper.PiOver4 / 2.0f;

        protected override TimeSpan MinLifetime => TimeSpan.FromMilliseconds(1500);
        protected override TimeSpan MaxLifetime => TimeSpan.FromMilliseconds(2000);

        protected override float MinScale => 1.0f;
        protected override float MaxScale => 1.5f;

        protected override float MinSpawnAngle => 89.0f;
        protected override float MaxSpawnAngle => 91.0f;
    }
}
