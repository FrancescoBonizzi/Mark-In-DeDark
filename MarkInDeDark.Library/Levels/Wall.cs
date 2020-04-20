using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDark.Library.Levels
{
    public class Wall
    {
        public LinkDirection Direction { get; }
        public bool IsDestroyable { get; }

        public DrawingInfos DrawingInfos { get; }
        private readonly Sprite _sprite;
        private readonly Sprite _spriteNormalMap;

        public Wall(
            LinkDirection direction,
            Vector2 position,
            bool isDestroyable,
            Sprite sprite,
            Sprite spriteNormalMap)
        {
            Direction = direction;
            DrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForWall(position + sprite.SpriteCenter)
            };

            IsDestroyable = isDestroyable;
            _sprite = sprite;
            _spriteNormalMap = spriteNormalMap;
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);

        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            if (_spriteNormalMap != null)
                spriteBatch.Draw(_spriteNormalMap, DrawingInfos);
        }
    }
}
