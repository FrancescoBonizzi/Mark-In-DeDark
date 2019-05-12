using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.Drawing;
using FlowsoftGamesMonogame.Extensions;
using FlowsoftGamesMonogame.Particles;
using FlowsoftGamesMonogame.UI;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Mark.Engine
{
    public class Candle
    {
        private Light _torch;

        private ParticleGeneratorTimedEffect _smokeEffect;
        private DrawingInfos _drawingInfos;
        private FadeObject _fadeObject;
        private TimeSpan _timeToLive;

        public Light LightMap { get; }
        public bool IsDying { get; private set; }
        public bool AreSomeMonsterIsFollowingMe { get; set; }

        private readonly SpriteAnimation _animation;

        public Candle(
            AssetsLoader assetsLoader,
            GameConfig gameConfig,
            Vector2 position)
        {
            _animation = assetsLoader.GetAnimation("Candle");

            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                Origin = _animation.FirstFrameSprite.SpriteCenter
            };

            LightMap = new Light(assetsLoader.Sprites["light_1"], gameConfig.CandleColorRGB1, gameConfig.CandleColorRGB2, gameConfig.CandleLightSize);
            _torch = new Light(assetsLoader.Sprites["light_pixel"], gameConfig.CandleColorRGB1, gameConfig.CandleColorRGB2, gameConfig.CandleLightSize);
            _torch.SetPosition(new Vector2(WorldCenter.X, WorldCenter.Y - 15));

            LightMap.SetPosition(WorldCenter);

            IsDying = false;
            AreSomeMonsterIsFollowingMe = false;

            _timeToLive = TimeSpan.FromSeconds(gameConfig.CandleTimeToLiveWhenAttackedSeconds);

            _smokeEffect = new ParticleGeneratorTimedEffect(
                new CandleSmokeParticleSystem(assetsLoader),
                _drawingInfos.Center(_animation.FirstFrameSprite.Width, _animation.FirstFrameSprite.Height) - new Vector2(0.0f, 15.0f),
                TimeSpan.FromMilliseconds(35),
                _timeToLive);

            _fadeObject = new FadeObject(
                _timeToLive,
                _drawingInfos.OverlayColor);
        }

        public void Die()
        {
            IsDying = true;

            if (!_fadeObject.IsFading)
                _fadeObject.FadeOut();
        }

        public bool IsActive
            => _timeToLive > TimeSpan.Zero;

        public Vector2 WorldCenter
            => _drawingInfos.Center(_animation.CurrentFrameWidth, _animation.CurrentFrameHeight);

        public Vector2 Position
            => _drawingInfos.Position;

        public void Update(TimeSpan elapsed)
        {
            if (IsDying)
            {
                _timeToLive -= elapsed;
                _smokeEffect.Update(elapsed);
                _fadeObject.Update(elapsed);
                _drawingInfos.OverlayColor = _fadeObject.OverlayColor;
                _torch.SetTemporaryColor(_fadeObject.OverlayColor);
                LightMap.SetTemporaryColor(_fadeObject.OverlayColor);
            }

            _animation.Update(elapsed);
            LightMap.Update(elapsed);
            _torch.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation.CurrentFrame, _drawingInfos);
            _torch.Draw(spriteBatch);
        }

        public void DrawSmoke(SpriteBatch spriteBatch)
        {
            if (IsDying)
            {
                _smokeEffect.Draw(spriteBatch);
            }
        }
    }
}
