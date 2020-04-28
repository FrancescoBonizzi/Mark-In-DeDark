using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Maths;
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

            Hull = Utilities.HullFromRectangle(new Rectangle(
                (int)position.X + 12, 
                (int)position.Y + _sprite.Height - 53, 
                40, 
                40));
            Hull.Points.Add(new Vector2(0, 0));
            Hull.Points.Add(new Vector2(0, 1));
            Hull.Points.Add(new Vector2(1, 1));
            Hull.Points.Add(new Vector2(1, 0));
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);
    }
}
