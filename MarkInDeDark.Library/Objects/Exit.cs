using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library
{
    public class Exit
    {
        private Sprite _sprite;
        private readonly Sprite _openSprite;
        private readonly DrawingInfos _drawingInfos;

        public Exit(
            AssetsLoader assetsLoader,
            GameConfig gameConfig,
            Vector2 position)
        {
            _sprite = assetsLoader.Sprites["win_closed"];
            _openSprite = assetsLoader.Sprites["win"];
            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                HitBoxTolerance = new Rectangle(60, 60, 120, 120),
                Origin = _sprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.GroundObjectsLayerDepth
            };
        }

        public Vector2 Position
            => _drawingInfos.Position;

        public Rectangle CollisionRectangle
            => _drawingInfos.HitBox(
                _sprite.Width,
                _sprite.Height);

        public void SetOpen()
            => _sprite = _openSprite;

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, _drawingInfos);

    }
}
