using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;

using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Extensions;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Objects
{
    public class LittleLight
    {
        private readonly SpriteAnimation _animation;
        private readonly DrawingInfos _drawingInfos;
        private readonly LittleLightParticleSystem _playerCandleFireParticleSystem;
        private readonly Vector2 _fireEffectGenerationCenter;

        public Light LightMap { get; }

        public LittleLight(
            AssetsLoader assetsLoader,
            GameConfig gameConfig,
            Vector2 position)
        {
            _animation = assetsLoader.GetAnimation("LittleLight");

            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                Origin = _animation.FirstFrameSprite.SpriteCenter,
                Scale = 3.0f,
                LayerDepth = ZIndexCalculator.Calculate(position + _animation.FirstFrameSprite.SpriteCenter)
            };

            LightMap = new Light(assetsLoader.Sprites["light_1"], gameConfig.LittleLightColorRGB1, gameConfig.LittleLightColorRGB2, gameConfig.LittleLightLightSize);
            LightMap.SetPosition(WorldCenter);
            _playerCandleFireParticleSystem = new LittleLightParticleSystem(assetsLoader);
            _fireEffectGenerationCenter = _drawingInfos.Center(_animation.FirstFrameSprite.Width, _animation.FirstFrameSprite.Height) - new Vector2(0, 10f);
        }


        public void Update(TimeSpan elapsed)
        {
            _playerCandleFireParticleSystem.AddParticles(_fireEffectGenerationCenter);
            _playerCandleFireParticleSystem.Update(elapsed);
            _animation.Update(elapsed);
            LightMap.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation.CurrentFrame, _drawingInfos);
            _playerCandleFireParticleSystem.Draw(spriteBatch);
        }


        public Vector2 WorldCenter
            => _drawingInfos.Center(_animation.CurrentFrameWidth, _animation.CurrentFrameHeight);

        public Vector2 Position
            => _drawingInfos.Position;
    }
}
