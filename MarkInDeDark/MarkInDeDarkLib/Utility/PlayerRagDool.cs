#region Using

using Mark.Engine.Particles;
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
    public class PlayerRagDool : GameObject
    {
        #region Dichiarazioni

        private double default_interval_ = .035;
        private double animation_interval_ = .035;
        private float fade_increment_ = -0.009f;
        private float scale_increment_ = -0.02f;
        private float rotation_increment_ = -0.04f;
        private float opacity_ = 1.0f;

        #endregion

        #region Costruttore

        public PlayerRagDool(
            Vector2 StartingPosition)
        {
            this.world_location_ = StartingPosition + new Vector2(frame_width_ / 2, frame_heigth_ / 2);
        }

        protected override void LoadEveryAnimation()
        {
            LoadAnimation("rotate", Loader.p_rotating_, true, 0.2f, Loader.atlas_);
            PlayAnimation("rotate");

            this.frame_width_ = Loader.p_rotating_[0].Width;
            this.frame_heigth_ = Loader.p_rotating_[0].Height;
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);
            this.origin_ = new Vector2(frame_width_ / 2, frame_heigth_ / 2);
        }

        #endregion

        #region Update/Draw

        public override void Update(float elapsed)
        {
            animation_interval_ -= elapsed;

            if (animation_interval_ <= 0.0)
            {
                animation_interval_ = default_interval_;
                opacity_ += fade_increment_;
                this.scale_ += scale_increment_;
                this.overlay_color_ *= opacity_;
                this.rotation_ += rotation_increment_;
            }

            base.Update(elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        #endregion
    }
}
