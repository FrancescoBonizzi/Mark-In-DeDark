using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace MarkInDeDark.Library.Levels
{
    public class Pillar
    {
        public DrawingInfos DrawingInfos { get; }
        private readonly Sprite _sprite;
        public Hull Hull { get; }


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

            Hull = HullFromRectangle(new Rectangle((int)position.X + 30, (int)position.Y + 30, _sprite.Width - 30, _sprite.Height - 30));
            Hull.Points.Add(new Vector2(0, 0));
            Hull.Points.Add(new Vector2(0, 1));
            Hull.Points.Add(new Vector2(1, 1));
            Hull.Points.Add(new Vector2(1, 0));

        }


        private static Hull HullFromRectangle(Rectangle bounds)
        {
            return new Hull()
            {
                Position = new Vector2(bounds.X, bounds.Y),
                Scale = new Vector2(bounds.Width, bounds.Height)
            };
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);
    }
}
