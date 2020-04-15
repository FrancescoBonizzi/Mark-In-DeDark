using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Extensions;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDarkMono.Objects
{
    public class PassableObject : IDrawableObject
    {
        private readonly Sprite _sprite;
        private readonly DrawingInfos _drawingInfos;

        public PassableObject(
            Sprite sprite,
            GameCell gameCell)
        {
            gameCell.IsUnwalkable = false;
            _sprite = sprite;
            _drawingInfos = new DrawingInfos()
            {
                Position = gameCell.CenterPosition,
                Origin = _sprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForGenericObject(gameCell.CenterPosition)
            };
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, _drawingInfos);
    }
}
