using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Levels
{
    public class WallFixed : IWall
    {
        public LinkDirection Direction { get; }

        private readonly DrawingInfos _drawingInfos;
        private readonly Sprite _sprite;
        private readonly Sprite _normalSprite;

        public Rectangle CollisionRectangle { get; }
        public List<Hull> Hulls { get; }
        public Vector2 Position => _drawingInfos.Position;
        public Vector2 CenterPosition => _drawingInfos.Position + _sprite.SpriteCenter;

        private readonly DrawingInfos _decalDrawingInfos;
        private readonly Sprite _decal;
        private readonly Sprite _decalNormal;

        public WallFixed(
            LinkDirection direction,
            Vector2 position,
            Sprite sprite,
            Sprite normalSprite,
            Sprite decal = null,
            Sprite decalNormal = null)
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
                    (int)position.X + MarkInDeDarkGame.LevelCustomization.WallFixedVerticalOffset.X, 
                    (int)position.Y + MarkInDeDarkGame.LevelCustomization.WallFixedVerticalOffset.Y,
                    MarkInDeDarkGame.LevelCustomization.WallFixedVerticalOffset.Width,
                    MarkInDeDarkGame.LevelCustomization.WallFixedVerticalOffset.Height);
            }
            else
            {
                // Quelli orizzontali hanno un boundingbox più stretto (alla base del muro)
                CollisionRectangle = new Rectangle(
                    (int)position.X + MarkInDeDarkGame.LevelCustomization.WallFixedHorizontalOffset.X,
                    (int)position.Y + MarkInDeDarkGame.LevelCustomization.WallFixedHorizontalOffset.Y,
                    MarkInDeDarkGame.LevelCustomization.WallFixedHorizontalOffset.Width,
                    MarkInDeDarkGame.LevelCustomization.WallFixedHorizontalOffset.Height);
            }

            if (decal != null)
            {
                _decal = decal;
                _decalNormal = decalNormal;
                _decalDrawingInfos = new DrawingInfos()
                {
                    Position = _drawingInfos.Position + sprite.SpriteCenter,
                    LayerDepth = _drawingInfos.LayerDepth + 0.00001f,
                    Origin = _decal.SpriteCenter
                };
            }

            _sprite = sprite;
            _normalSprite = normalSprite;

            Hulls = new List<Hull>()
            {
                Utilities.HullFromRectangle(CollisionRectangle)
            };

            Hulls[0].Points.Add(new Vector2(0, 0));
            Hulls[0].Points.Add(new Vector2(0, 1));
            Hulls[0].Points.Add(new Vector2(1, 1));
            Hulls[0].Points.Add(new Vector2(1, 0));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_sprite, _drawingInfos);

            if (_decal != null)
                spriteBatch.Draw(_decal, _decalDrawingInfos);
        }

        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            if (_normalSprite != null)
                spriteBatch.Draw(_normalSprite, _drawingInfos);

            if (_decalNormal != null)
                spriteBatch.Draw(_decalNormal, _decalDrawingInfos);
        }

        public void Update(TimeSpan elapsed)
        {

        }
    }

}
