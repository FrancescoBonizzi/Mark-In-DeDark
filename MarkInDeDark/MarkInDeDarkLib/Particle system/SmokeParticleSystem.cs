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
    public class SmokeParticleSystem : ParticleSystem
    {
        public SmokeParticleSystem()
            : base(5, Loader.atlas_, Loader.smoke_, Loader.smoke_)
        {
        }

        protected override void InitializeConstants()
        {
            min_initial_speed_ = 30.0f;
            max_initial_speed_ = 60.0f;

            min_acceleration_ = 30.0f;
            max_acceleration_ = 50.0f;

            min_lifetime_ = 1.5f;
            max_lifetime_ = 2.0f;

            min_scale_ = .5f;
            max_scale_ = 1.0f;

            min_spawn_angle_ = 85.0f;
            max_spawn_angle_ = 95.0f;

            min_num_particles_ = 6;
            max_num_particles_ = 10;

            min_rotation_speed_ = -MathHelper.PiOver4 / 2.0f;
            max_rotation_speed_ = MathHelper.PiOver4 / 2.0f;
        }
    }
}
