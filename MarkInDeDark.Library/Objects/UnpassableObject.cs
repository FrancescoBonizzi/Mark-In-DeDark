using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDark.Library.Objects
{
    public class UnpassableObject : IDrawableObject
    {
        private readonly Sprite _sprite;
        private readonly DrawingInfos _drawingInfos;

        public Rectangle CollisionRectangle { get; private set; }

        public UnpassableObject(
            Sprite sprite,
            GameCell gameCell)
        {
            gameCell.IsUnwalkable = true;
            _sprite = sprite;
            _drawingInfos = new DrawingInfos()
            {
                Position = gameCell.CenterPosition,
                Origin = _sprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForGenericObject(gameCell.CenterPosition)
            };

            CollisionRectangle = new Rectangle(
                (int)_drawingInfos.Position.X - sprite.Width / 2,
                (int)_drawingInfos.Position.Y - sprite.Height / 2,
                _sprite.Width,
                _sprite.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
           => spriteBatch.Draw(_sprite, _drawingInfos);

        public void DrawForDebug(SpriteBatch spriteBatch)
            => spriteBatch.DrawRectangle(CollisionRectangle, Color.Red.WithAlpha(0.5f));
    }
}
