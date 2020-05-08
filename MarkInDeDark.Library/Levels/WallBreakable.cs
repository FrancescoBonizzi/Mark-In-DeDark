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
    public class WallBreakable : IWall
    {
        public LinkDirection Direction { get; }
        
        private readonly DrawingInfos _drawingInfos;
        private readonly DrawingInfos _drawingInfosA;
        private readonly DrawingInfos _drawingInfosB;

        private readonly Sprite _primarySprite;
        private readonly Sprite _primarySpriteNormal;

        private readonly Sprite _brokenSprite;
        private readonly Sprite _brokenSpriteNormal;

        private readonly Sprite _brokenSpriteB;
        private readonly Sprite _brokenSpriteBNormal;

        private bool _isBroken;

        public Rectangle CollisionRectangle { get; }
        public Hull Hull { get; }
        public Vector2 Position => _drawingInfos.Position;

        public WallBreakable(
            LinkDirection direction,
            Vector2 position,
            Sprite primarySprite,
            Sprite primarySpriteNormal,
            Sprite brokenSprite,
            Sprite brokenSpriteNormal,
            Sprite brokenSpriteB = null,
            Sprite brokenSpriteBNormal = null)
        {
            Direction = direction;
            
            _primarySprite = primarySprite;
            _primarySpriteNormal = primarySpriteNormal;

            _brokenSprite = brokenSprite;
            _brokenSpriteNormal = brokenSpriteNormal;

            _brokenSpriteB = brokenSpriteB;
            _brokenSpriteBNormal = brokenSpriteBNormal;

            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForWall(position + primarySprite.SpriteCenter)
            };

            // Pezzo sud
            _drawingInfosA = new DrawingInfos()
            {
                Position = position + new Vector2(0f, _primarySprite.Height - _brokenSprite.Height),
                LayerDepth = ZIndexCalculator.OverAllLayerDepth
            };

            // Pezzo nord
            _drawingInfosB = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForWall(position + primarySprite.SpriteCenter)
            };

            if (direction == LinkDirection.EastLink || direction == LinkDirection.WestLink)
            {
                CollisionRectangle = new Rectangle(
                    (int)position.X + 20,
                    (int)position.Y,
                    _primarySprite.Width - 40,
                    _primarySprite.Height + 55);
            }
            else
            {
                // Quelli orizzontali hanno un boundingbox più stretto (alla base del muro)
                CollisionRectangle = new Rectangle(
                    (int)position.X,
                    (int)position.Y + 100,
                    _primarySprite.Width,
                    40);
            }

            Hull = Utilities.HullFromRectangle(CollisionRectangle);

            Hull.Points.Add(new Vector2(0, 0));
            Hull.Points.Add(new Vector2(0, 1));
            Hull.Points.Add(new Vector2(1, 1));
            Hull.Points.Add(new Vector2(1, 0));
        }

        public void Break()
        {
            _isBroken = true;
            Hull.Enabled = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isBroken)
            {
                spriteBatch.Draw(_primarySprite, _drawingInfos);
            }
            else
            {
                if (_brokenSpriteB == null)
                {
                    spriteBatch.Draw(_brokenSprite, _drawingInfos);
                }
                else
                {
                    spriteBatch.Draw(_brokenSprite, _drawingInfosA);
                    spriteBatch.Draw(_brokenSpriteB, _drawingInfosB);
                }
            }
        }

        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            if (!_isBroken)
            {
                spriteBatch.Draw(_primarySpriteNormal, _drawingInfos);
            }
            else
            {
                if (_brokenSpriteBNormal == null)
                {
                    spriteBatch.Draw(_brokenSpriteNormal, _drawingInfos);
                }
                else
                {
                    spriteBatch.Draw(_brokenSpriteNormal, _drawingInfosA);
                    spriteBatch.Draw(_brokenSpriteBNormal, _drawingInfosB);
                }
            }
        }

        public void Update(TimeSpan elapsed)
        {

        }
    }
}
