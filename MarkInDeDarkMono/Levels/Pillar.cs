using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDarkMono.Levels
{
    public class Pillar
    {
        public DrawingInfos DrawingInfos { get; }
        private readonly Sprite _sprite;

        public Pillar(
            Vector2 position,
            Sprite sprite)
        {
            DrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForPillars(position + sprite.SpriteCenter)
            };
            _sprite = sprite;
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);
    }
}
