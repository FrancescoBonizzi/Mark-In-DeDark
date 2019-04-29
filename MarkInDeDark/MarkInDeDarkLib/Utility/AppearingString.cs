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
    public class AppearingString
    {
        #region Dichiarazioni

        private string text_;
        private Color color_;
        private Vector2 world_location_;
        private SpriteFont font_;

        private bool active_ = true;

        private const float fade_out_timer_ = 4.0f;
        private float fade_out_elapsed_ = 0.0f;
        private float opacity_ = 0.0f;

        #endregion

        #region Costruttore

        public AppearingString(string Text, Color TextColor)
        {
            text_ = Text;
            color_ = TextColor;
            font_ = Loader.font_;

            Vector2 string_dimension = font_.MeasureString(text_);
            world_location_ = new Vector2(
                400 - string_dimension.X / 2,
                10);
        }

        public bool Active
        {
            get { return active_; }
        }

        #endregion

        #region Update/Draw

        public void Update(float elapsed)
        {
            if (fade_out_elapsed_ >= 0.0f)
            {
                fade_out_elapsed_ += elapsed;

                if (fade_out_elapsed_ <= (fade_out_timer_ / 2))
                {
                    opacity_ = MathHelper.Lerp(0.0f, 1f, fade_out_elapsed_);
                }
                else
                {
                    opacity_ = MathHelper.Lerp(0.0f, 1f, fade_out_timer_ - fade_out_elapsed_);
                }

                if (fade_out_elapsed_ >= fade_out_timer_)
                {
                    active_ = false;
                } 
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font_, text_, world_location_, color_ * opacity_);
        }

        #endregion

    }
}
