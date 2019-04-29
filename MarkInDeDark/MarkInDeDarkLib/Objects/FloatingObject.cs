#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class FloatingObject
    {
        #region Dichiarazioni

        private Texture2D atlas_;
        private Rectangle texture_rectangle_;
        private Vector2 world_location_;
        private Vector2 starting_world_location_;
        private Vector2 float_world_location_;
        private Rectangle collision_rectangle_;

        private bool active_ = true;

        private const float fade_out_timer_ = 2.0f;
        private float fade_out_elapsed_ = -1.0f;
        private float opacity_ = 1.0f;

        private double default_svolazzamento_interval_ = .035;
        private double svolazzamento_interval_ = .035;
        private float svolazzamento_lerp_increment_ = 0.05f;
        private float svolazzamento_lerp_amount_ = 0.0f;

        #endregion

        #region Costruttore

        public FloatingObject(
            Rectangle TextureRectangle,
            Vector2 Location)
        {
            this.atlas_ = Loader.atlas_;
            this.texture_rectangle_ = TextureRectangle;
            this.world_location_ = Location;
            this.collision_rectangle_ = new Rectangle(
                (int)world_location_.X, (int)world_location_.Y,
                texture_rectangle_.Width, texture_rectangle_.Height);

            this.starting_world_location_ = world_location_;
            this.float_world_location_ = new Vector2(world_location_.X, world_location_.Y - 10.0f);
            this.svolazzamento_lerp_amount_ = Helper.RandomBetween(0.0f, 1.0f);

            int collisionBoundingBoxRestriction = 20;
            this.collision_rectangle_.X += collisionBoundingBoxRestriction;
            this.collision_rectangle_.Y += collisionBoundingBoxRestriction;
            this.collision_rectangle_.Width -= collisionBoundingBoxRestriction;
            this.collision_rectangle_.Height -= collisionBoundingBoxRestriction;
        }

        #endregion

        #region Metodi / GetSet

        public Rectangle CollisionRectangle
        {
            get { return collision_rectangle_; }
        }

        public bool Active
        {
            get { return active_; }
        }

        public void Taken()
        {
            if (fade_out_elapsed_ == -1.0f)
                fade_out_elapsed_ = 0.0f;
        }

        public bool Fading
        {
            get { return fade_out_elapsed_ >= 0.0f; }
        }

        #endregion

        #region Update/Draw

        public void Update(float elapsed)
        {
            #region Fade

            if (fade_out_elapsed_ >= 0.0f)
            {
                fade_out_elapsed_ += elapsed + elapsed; //Per velocizzarlo

                // Fai il fade
                opacity_ = MathHelper.Lerp(0.0f, 1.0f, 1.0f - fade_out_elapsed_);

                if (fade_out_elapsed_ >= fade_out_timer_)
                {
                    active_ = false;
                }
            }

            #endregion

            #region Svolazzamento

            svolazzamento_interval_ -= elapsed;
            if (svolazzamento_interval_ <= 0.0)
            {
                svolazzamento_interval_ = default_svolazzamento_interval_;

                svolazzamento_lerp_amount_ += svolazzamento_lerp_increment_;

                this.world_location_ = Vector2.Lerp(
                    this.starting_world_location_,
                    this.float_world_location_,
                    svolazzamento_lerp_amount_);

                if (svolazzamento_lerp_amount_ >= 1.0f || svolazzamento_lerp_amount_ <= 0.0f)
                {
                    svolazzamento_lerp_increment_ *= -1;
                }
            }

            #endregion
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(atlas_, Camera.WorldToScreen(world_location_), texture_rectangle_, Color.White * opacity_);
        }

        #endregion
    }
}
