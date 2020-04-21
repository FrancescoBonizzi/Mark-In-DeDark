using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDark.Library.Levels
{
    public class Pillar
    {
        public DrawingInfos DrawingInfos { get; }
        private readonly Sprite _sprite;

        public Rectangle CollisionRectangle { get; }

        public Pillar(
            Vector2 position,
            Sprite sprite)
        {
            DrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForPillars(position + sprite.SpriteCenter)
            };

            CollisionRectangle = new Rectangle(
                (int)position.X,
                (int)position.Y + 100,
                sprite.Width,
                sprite.Height - 150);

            _sprite = sprite;
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);

        public void DrawForDebug(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle(CollisionRectangle, Color.Red.WithAlpha(0.5f));
        }
    }
}
