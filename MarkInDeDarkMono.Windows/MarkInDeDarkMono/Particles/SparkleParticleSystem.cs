using FlowsoftGamesMonogame.Particles;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDarkMono.Particles
{
    public class SparkleParticleSystem : ParticleGenerator
    {
        public SparkleParticleSystem(
            AssetsLoader assetsLoader)
            : base(assetsLoader.Sprites["scintilla"]) { } // TODO è molto importante anche per altre cose... tipo il fumo della cendela potrebbe fare ombra nera!!! 
            // e anche la magia del wolf, AssetsLoader.scintilla_lightmap_)

        protected override int Density => 2;

        protected override int MinNumParticles => 4;
        protected override int MaxNumParticles => 8;

        protected override float MinInitialSpeed => 20f;
        protected override float MaxInitialSpeed => 35f;

        protected override float MinAcceleration => 20f;
        protected override float MaxAcceleration => 35f;

        protected override float MinRotationSpeed => (-MathHelper.PiOver2) / 2;
        protected override float MaxRotationSpeed => (MathHelper.PiOver2) / 2;

        protected override TimeSpan MinLifetime => TimeSpan.FromMilliseconds(200);
        protected override TimeSpan MaxLifetime => TimeSpan.FromMilliseconds(600);

        protected override float MinScale => 0.2f;
        protected override float MaxScale => 2.0f;

        protected override float MinSpawnAngle => 0.0f;
        protected override float MaxSpawnAngle => 360.0f;    
    }
}
