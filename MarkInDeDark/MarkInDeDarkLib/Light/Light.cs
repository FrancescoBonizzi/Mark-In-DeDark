#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class Light : GameObject
    {
        #region Dichiarazioni

        private Texture2D texture_;
        private Rectangle texture_rectangle_;
        private bool animate_;

        private bool active_;

        private double default_fade_interval_ = .035;
        private double fade_interval_ = .035;
        private float fade_increment_ = 0.009f;
        private float opacity_ = 0.0f;

        private float scale_lerp_increment_ = 0.05f;
        private float scale_lerp_amount_ = 0.0f;

        private Color a_color_;
        private Color b_color_;
        private float color_lerp_amount_ = 0.0f;
        private float color_lerp_increment_ = 0.05f;

        #endregion

        #region Costruttore

        public Light()
            : base()
        {
            this.active_ = true;
            this.animate_ = false;
            this.texture_ = Loader.atlas_;
            this.texture_rectangle_ = Loader.lightmask_;

            this.scale_ = 1.0f;

            this.frame_width_ = texture_rectangle_.Width;
            this.frame_heigth_ = texture_rectangle_.Height;
            this.origin_ = new Vector2(frame_width_ / 2, frame_heigth_ / 2);
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);
        }

        // Luce custom
        public Light(Rectangle LightRectangle)

        {
            // Ps. La lightmap non andrà più, perchè per lei l'origine era su 0 0
            this.active_ = true;
            this.animate_ = false;
            this.texture_ = Loader.atlas_;
            this.texture_rectangle_ = LightRectangle;

            this.scale_ = 1.0f;

            this.frame_width_ = texture_rectangle_.Width;
            this.frame_heigth_ = texture_rectangle_.Height;
            this.origin_ = new Vector2(frame_width_ / 2, frame_heigth_ / 2);
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);
        }

        #endregion

        #region Metodi / GetSet

        public void Animate(Color A_color, Color B_color)
        {
            animate_ = true;
            a_color_ = A_color;
            b_color_ = B_color;

            opacity_ = 0.0f;
        }

        public bool Active
        {
            get { return active_; }
            set { active_ = value; }
        }

        protected override void LoadEveryAnimation()
        {
        }

        #endregion

        #region Update/Draw

        public override void Update(float elapsed)
        {
            if (opacity_ <= 1.0f)
            {
                opacity_ += elapsed;
            }

            if (!animate_ || !active_)
                return;

            fade_interval_ -= elapsed;

            if (fade_interval_ <= 0.0)
            {
                fade_interval_ = default_fade_interval_;

                // Cambiamento di intensità
                opacity_ += fade_increment_;
                if (opacity_ >= 1.0f || opacity_ <= 0.9f)
                {
                    fade_increment_ *= -1;
                }

                // Cambiamento di colore
                color_lerp_amount_ += color_lerp_increment_;
                this.overlay_color_ = Color.Lerp(a_color_, b_color_, color_lerp_amount_);
                if (color_lerp_amount_ >= 1.0f || color_lerp_amount_ <= 0.0f)
                {
                    color_lerp_increment_ *= -1;
                }

                // Cambiamento di scala
                scale_lerp_amount_ += scale_lerp_increment_;
                this.scale_ = MathHelper.Lerp(1.0f, 0.9f, scale_lerp_amount_);
                if (scale_lerp_amount_ >= 1.0f || scale_lerp_amount_ <= 0.0f)
                {
                    scale_lerp_increment_*= -1;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (active_)
            {
                spriteBatch.Draw(
                    texture_,
                    Camera.WorldToScreen(world_location_),
                    texture_rectangle_,
                    overlay_color_ * opacity_,
                    0.0f,
                    origin_,
                    scale_,
                    effect_,
                    1.0f);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            if (active_)
            {
                spriteBatch.Draw(
                    texture_,
                    position,
                    texture_rectangle_,
                    overlay_color_ * opacity_,
                    0.0f,
                    origin_,
                    scale_,
                    effect_,
                    1.0f);
            }
        }

        #endregion
    }
}
