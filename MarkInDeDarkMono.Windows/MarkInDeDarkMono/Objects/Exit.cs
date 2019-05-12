using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.Drawing;
using FlowsoftGamesMonogame.Extensions;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Mark.Engine
{
#warning Potrebbe diventare un trigger object per togliere un po' di logica al game
    public class Exit
    {
        private Sprite _sprite;
        private Sprite _openSprite;
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
