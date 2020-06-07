using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Particles.Snowing
{
    public class SnowFlake : DrawingInfos
    {
        private readonly Sprite _snowFlake;

        private float _opacity;
        public float Opacity 
        {
            get => _opacity;
            set
            {
                _opacity = value;
                OverlayColor = Color.White.WithAlpha(_opacity);
            }
        }

        public Rectangle Bounds { get; }
        public Vector2 Velocity { get; set; }
        public bool IsRemoved { get; set; }

        public SnowFlake(Sprite snowFlakeSprite)
        {
            _snowFlake = snowFlakeSprite;
            Opacity = 1f;
            Origin = snowFlakeSprite.SpriteCenter;
            Bounds = _snowFlake.SourceRectangle;
        }

        public void Update(TimeSpan elapsed)
        {
            Position += Velocity * (float)elapsed.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_snowFlake, this);
        }
    }
}
