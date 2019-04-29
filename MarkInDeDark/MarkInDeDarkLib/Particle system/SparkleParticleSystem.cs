#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

#endregion

namespace Mark.Engine.Particles
{
    public class SparkleParticleSystem : ParticleSystem
    {
        public SparkleParticleSystem()
            : base(2, Loader.atlas_, Loader.scintilla_, Loader.scintilla_lightmap_)
        {
        }

        protected override void InitializeConstants()
        {
            min_initial_speed_ = 20f;
            max_initial_speed_ = 35f;

            min_acceleration_ = 20f;
            max_acceleration_ = 35f;

            min_lifetime_ = 0.1f;
            max_lifetime_ = 0.4f;

            min_scale_ = 0.2f;
            max_scale_ = 2.0f;

            min_spawn_angle_ = 0.0f;
            max_spawn_angle_ = 360.0f;

            min_num_particles_ = 4;
            max_num_particles_ = 8;

            min_rotation_speed_ = (-MathHelper.PiOver2)/2;
            max_rotation_speed_ = (MathHelper.PiOver2)/2;
        }

        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            base.InitializeParticle(p, where);
        }
    }
}
