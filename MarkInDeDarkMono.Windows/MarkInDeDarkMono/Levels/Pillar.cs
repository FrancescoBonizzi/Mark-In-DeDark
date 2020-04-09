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
                // I pilastri devono sempre stare sopra ai muri, anche se hanno la stessa Y
                LayerDepth = ZIndexCalculator.Calculate(position) + 0.001f
            };
            _sprite = sprite;
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);
    }
}
