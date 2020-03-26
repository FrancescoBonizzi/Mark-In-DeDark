using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Mark.Engine
{
    public class Exit
    {
        private Sprite _sprite;
        private readonly Sprite _openSprite;
        private readonly DrawingInfos _drawingInfos;
        public Light LightMap { get; }

        public Exit(
            AssetsLoader assetsLoader,
            GameConfig gameConfig,
            Vector2 position)
        {
            LightMap = new Light(assetsLoader.Sprites["light_1"], gameConfig.ExitLightSize);
            _sprite = assetsLoader.Sprites["win_closed"];
            _openSprite = assetsLoader.Sprites["win"];
            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                HitBoxTolerance = new Rectangle(60, 60, 60, 60),
                Origin = _sprite.SpriteCenter
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

        public void Update(TimeSpan elapsed)
            => LightMap.Update(elapsed);

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, _drawingInfos);

    }
}
