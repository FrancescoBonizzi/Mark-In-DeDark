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
    public class BloodParticleSystem : ParticleSystem
    {
        public BloodParticleSystem()
            : base(5, Loader.atlas_, Loader.blood_particle_, Loader.blood_particle_)
        {
        }

        protected override void InitializeConstants()
        {
            min_initial_speed_ = 120f;
            max_initial_speed_ = 150f;

            min_acceleration_ = 30f;
            max_acceleration_ = 45f;

            min_lifetime_ = 1.2f;
            max_lifetime_ = 1.8f;

            min_scale_ = 1.0f;
            max_scale_ = 1.6f;

            min_spawn_angle_ = 0.0f;
            max_spawn_angle_ = 360.0f;

            min_num_particles_ = 4;
            max_num_particles_ = 8;

            min_rotation_speed_ = -MathHelper.PiOver2;
            max_rotation_speed_ = MathHelper.PiOver2;
        }

        protected override void InitializeParticle(Particle p, Vector2 where)
        {
            base.InitializeParticle(p, where);
        }
    }
}
