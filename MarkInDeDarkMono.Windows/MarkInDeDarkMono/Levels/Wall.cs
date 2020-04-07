using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDarkMono.Levels
{
    public class Wall
    {
        public LinkDirection Direction { get; }
        public bool IsDestroyable { get; }

        public DrawingInfos DrawingInfos { get; }
        private readonly Sprite _sprite;

        public Wall(
            LinkDirection direction,
            Vector2 position,
            bool isDestroyable,
            Sprite sprite)
        {
            Direction = direction;
            DrawingInfos = new DrawingInfos()
            {
                Position = position
            };

            IsDestroyable = isDestroyable;
            _sprite = sprite;
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);
    }
}
