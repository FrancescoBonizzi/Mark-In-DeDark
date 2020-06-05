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
    public class WallBreakable : IWall
    {
        public LinkDirection Direction { get; }
        
        private readonly DrawingInfos _drawingInfos;
        private readonly DrawingInfos _drawingInfosA;
        private readonly DrawingInfos _drawingInfosB;

        private readonly DrawingInfos _obscurationDrawingInfos;
        private readonly DrawingInfos _obscurationDrawingInfosA;
        private readonly DrawingInfos _obscurationDrawingInfosB;

        private readonly Sprite _primarySprite;
        private readonly Sprite _primarySpriteNormal;

        private readonly Sprite _brokenSprite;
        private readonly Sprite _brokenSpriteNormal;

        private readonly Sprite _brokenSpriteB;
        private readonly Sprite _brokenSpriteBNormal;

        private bool _isBroken;

        public Rectangle CollisionRectangle { get; }
        public List<Hull> Hulls { get; }
        public Vector2 Position => _drawingInfos.Position;
        public Vector2 CenterPosition => _drawingInfos.Position + _primarySprite.SpriteCenter;

        private float _obscurationOpacity = 0f;
        public float ObscurationOpacity
        {
            get => _obscurationOpacity;
            set
            {
                _obscurationOpacity = value;
                if (_obscurationOpacity > 0)
                {
                    _obscurationDrawingInfos.OverlayColor = Color.Black.WithAlpha(_obscurationOpacity);
                    _obscurationDrawingInfosA.OverlayColor = Color.Black.WithAlpha(_obscurationOpacity);
                    _obscurationDrawingInfosB.OverlayColor = Color.Black.WithAlpha(_obscurationOpacity);
                }
                else
                {
                    _obscurationDrawingInfos.OverlayColor = Color.White;
                    _obscurationDrawingInfosA.OverlayColor = Color.White;
                    _obscurationDrawingInfosB.OverlayColor = Color.White;
                }
            }
        }

        private readonly Hull _firstHull;
        private readonly Hull _secondHull;

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

            _obscurationDrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = _drawingInfos.LayerDepth + 0.00001f
            };

            // Pezzo sud
            _drawingInfosA = new DrawingInfos()
            {
                Position = position + new Vector2(0f, _primarySprite.Height - _brokenSprite.Height),
                LayerDepth = ZIndexCalculator.OverAllLayerDepth
            };

            _obscurationDrawingInfosA = new DrawingInfos()
            {
                Position = position,
                LayerDepth = _drawingInfosA.LayerDepth + 0.00001f
            };

            // Pezzo nord
            _drawingInfosB = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForWall(position + primarySprite.SpriteCenter)
            };

            _obscurationDrawingInfosB = new DrawingInfos()
            {
                Position = position,
                LayerDepth = _drawingInfosB.LayerDepth + 0.00001f
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

            _firstHull = Utilities.HullFromRectangle(CollisionRectangle);
            _firstHull.Points.Add(new Vector2(0, 0));
            _firstHull.Points.Add(new Vector2(0, 1));
            _firstHull.Points.Add(new Vector2(1, 1));
            _firstHull.Points.Add(new Vector2(1, 0));

            _secondHull = Utilities.HullFromRectangle(CollisionRectangle);
            _secondHull.Points.Add(new Vector2(0, 0));
            _secondHull.Points.Add(new Vector2(0, 1));
            _secondHull.Points.Add(new Vector2(1, 1));
            _secondHull.Points.Add(new Vector2(1, 0));

            Hulls = new List<Hull>()
            {
                _firstHull,
                _secondHull
            };
        }

        public void Break()
        {
            _isBroken = true;

            if (Direction == LinkDirection.EastLink || Direction == LinkDirection.WestLink)
            {
                // Sopra
                _firstHull.Scale = new Vector2(_firstHull.Scale.X, 130f);

                // Sotto
                _secondHull.Origin = new Vector2(0, 1);
                _secondHull.Scale = new Vector2(_secondHull.Scale.X, 40f);
                _secondHull.Position = new Vector2(_secondHull.Position.X, _firstHull.Position.Y + _primarySprite.Height);
            }
            else
            {
                // Sinistra
                _firstHull.Scale = new Vector2(50, _firstHull.Scale.Y);

                // Destra
                _secondHull.Origin = new Vector2(1, 0);
                _secondHull.Scale = new Vector2(50, _secondHull.Scale.Y);
                _secondHull.Position = new Vector2(_firstHull.Position.X + _primarySprite.Width, _firstHull.Position.Y);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isBroken)
            {
                spriteBatch.Draw(_primarySprite, _drawingInfos);
                spriteBatch.Draw(_primarySprite, _obscurationDrawingInfos);
            }
            else
            {
                if (_brokenSpriteB == null)
                {
                    spriteBatch.Draw(_brokenSprite, _drawingInfos);
                    spriteBatch.Draw(_brokenSprite, _obscurationDrawingInfos);
                }
                else
                {
                    spriteBatch.Draw(_brokenSprite, _drawingInfosA);
                    spriteBatch.Draw(_brokenSprite, _obscurationDrawingInfosA);

                    spriteBatch.Draw(_brokenSpriteB, _drawingInfosB);
                    spriteBatch.Draw(_brokenSpriteB, _obscurationDrawingInfosB);
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
