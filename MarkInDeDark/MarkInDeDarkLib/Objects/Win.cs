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

namespace Mark.Engine
{
    // Oggetto astratto per determinare un luogo in cui vincere
    public class Win : GameObject
    {
        #region Dichiarazioni

        private Texture2D texture_;
        private Light light_;
        private Rectangle texture_rectangle_;

        #endregion

        #region Costruttore

        public Win(
            Vector2 StartingPosition,
            Rectangle TextureRectangle)
            : base()
        {
            this.light_ = new Light();
          
            texture_ = Loader.atlas_;
            this.texture_rectangle_ = TextureRectangle;

            this.frame_width_ = TextureRectangle.Width;
            this.frame_heigth_ = TextureRectangle.Height;
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);

            this.world_location_ = StartingPosition;

            this.light_.OverlayColor *= 0.8f;
            light_.WorldLocation = WorldCenter;
        }

        protected override void LoadEveryAnimation()
        {
        }

        #endregion

        #region Get/Set

        public Light MyLight
        {
            get { return light_; }
        }

        public Rectangle TextureRectangle
        {
            set { texture_rectangle_ = value;  }
        }

        #endregion

        #region Update / Draw

        public override void Update(float elapsed)
        {
            light_.WorldLocation = WorldCenter;
            light_.Update(elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture_, Camera.WorldToScreen(world_location_), texture_rectangle_, Color.White);
        }

        #endregion
    }
}
