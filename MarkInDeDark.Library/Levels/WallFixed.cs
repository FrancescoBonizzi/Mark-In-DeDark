using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;

namespace MarkInDeDark.Library.Levels
{
    public class WallFixed : IWall
    {
        public LinkDirection Direction { get; }

        private readonly DrawingInfos _drawingInfos;
        private readonly Sprite _sprite;

        public Rectangle CollisionRectangle { get; }
        public Hull Hull { get; }
        public Vector2 Position => _drawingInfos.Position;

        private readonly DrawingInfos _decalDrawingInfos;
        private readonly Sprite _decal;

        public WallFixed(
            LinkDirection direction,
            Vector2 position,
            Sprite sprite,
            Sprite decal = null)
        {
            Direction = direction;
            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForWall(position + sprite.SpriteCenter)
            };

            if (direction == LinkDirection.EastLink || direction == LinkDirection.WestLink)
            {
                CollisionRectangle = new Rectangle(
                    (int)position.X + 20,
                    (int)position.Y,
                    sprite.Width - 40,
                    sprite.Height + 40);
            }
            else
            {
                // Quelli orizzontali hanno un boundingbox più stretto (alla base del muro)
                CollisionRectangle = new Rectangle(
                    (int)position.X,
                    (int)position.Y + 100,
                    sprite.Width,
                    40);
            }

            if (decal != null)
            {
                _decal = decal;
                _decalDrawingInfos = new DrawingInfos()
                {
                    Position = _drawingInfos.Position + sprite.SpriteCenter,
                    LayerDepth = _drawingInfos.LayerDepth + 0.00001f,
                    Origin = _decal.SpriteCenter
                };
            }

            _sprite = sprite;

            Hull = Utilities.HullFromRectangle(CollisionRectangle);

            Hull.Points.Add(new Vector2(0, 0));
            Hull.Points.Add(new Vector2(0, 1));
            Hull.Points.Add(new Vector2(1, 1));
            Hull.Points.Add(new Vector2(1, 0));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_sprite, _drawingInfos);

            if (_decal != null)
                spriteBatch.Draw(_decal, _decalDrawingInfos);
        }

        public void Update(TimeSpan elapsed)
        {

        }
    }

}
