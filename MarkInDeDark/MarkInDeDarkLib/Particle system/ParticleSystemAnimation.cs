#region Using

using Mark.Engine.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class ParticleSystemAnimation
    {
        #region Dichiarazioni

        private int n_particles_added_ = 0;
        private ParticleSystem particle_system_;
        private Vector2 world_location_;

        private float default_interval_ = 0.035f;
        private float animation_interval_ = 0.035f;

        #endregion

        #region Costruttore

        public ParticleSystemAnimation(
            Vector2 StartingPosition,
            ParticleSystem ParticleSystem)
        {
            this.world_location_ = StartingPosition;
            this.particle_system_ = ParticleSystem;
            this.particle_system_.AddParticles(world_location_);
        }

        #endregion

        #region Get/Set

        public bool Finished
        {
            get { return !particle_system_.IsActive(); }
        }

        #endregion

        #region Update/Draw

        public void Update(float elapsed)
        {
            animation_interval_ -= elapsed;
    
            particle_system_.Update(elapsed);

            if (animation_interval_ <= 0.0)
            {
                animation_interval_ = default_interval_;

                if (n_particles_added_ < 5)
                {
                    ++n_particles_added_;
                    particle_system_.AddParticles(world_location_);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            particle_system_.Draw(spriteBatch);
        }

        public void DrawSparkleLightmap(SpriteBatch spriteBatch)
        {
            particle_system_.DrawLightmap(spriteBatch);
        }

        #endregion
    }
}
