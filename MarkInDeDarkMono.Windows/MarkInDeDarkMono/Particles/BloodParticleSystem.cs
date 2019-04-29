using FlowsoftGamesMonogame.Particles;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDarkMono.Particles
{
    public class BloodParticleSystem : ParticleGenerator
    {
        public BloodParticleSystem(
            AssetsLoader assetsLoader)
            : base(assetsLoader.Sprites["blood"]) { }

        protected override int Density => 5;

        protected override int MinNumParticles => 6;
        protected override int MaxNumParticles => 10;

        protected override float MinInitialSpeed => 120f;
        protected override float MaxInitialSpeed => 150f;

        protected override float MinAcceleration => 30f;
        protected override float MaxAcceleration => 45f;

        protected override float MinRotationSpeed => -MathHelper.PiOver2;
        protected override float MaxRotationSpeed => MathHelper.PiOver2;

        protected override TimeSpan MinLifetime => TimeSpan.FromMilliseconds(1200);
        protected override TimeSpan MaxLifetime => TimeSpan.FromMilliseconds(1800);

        protected override float MinScale => 1.0f;
        protected override float MaxScale => 1.6f;

        protected override float MinSpawnAngle => 0.0f;
        protected override float MaxSpawnAngle => 360.0f;
    }
}
