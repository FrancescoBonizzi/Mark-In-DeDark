using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Particles;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.TransformationObjects;
using MarkInDeDarkMono;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Extensions;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono
{
    public class Candle
    {
        private readonly Light _torch;

        private ParticleGeneratorTimedEffect _smokeEffect;

        private readonly CandleFireParticleSystem _playerCandleFireParticleSystem;
        private readonly CandleSmokeParticleSystem _smokeEffectParticleSystem;

        private readonly Vector2 _fireEffectGenerationCenter;
        private readonly DrawingInfos _drawingInfos;
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
                Origin = _animation.FirstFrameSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.Calculate(position + _animation.FirstFrameSprite.SpriteCenter)
            };

            LightMap = new Light(assetsLoader.Sprites["light_1"], gameConfig.CandleColorRGB1, gameConfig.CandleColorRGB2, gameConfig.CandleLightSize);
            _torch = new Light(assetsLoader.Sprites["light_pixel"], gameConfig.CandleColorRGB1, gameConfig.CandleColorRGB2, gameConfig.CandleLightSize);
            _torch.SetPosition(new Vector2(WorldCenter.X, WorldCenter.Y - 15));

            LightMap.SetPosition(WorldCenter);

            IsDying = false;
            AreSomeMonsterIsFollowingMe = false;

            _timeToLive = TimeSpan.FromSeconds(gameConfig.CandleTimeToLiveWhenAttackedSeconds);

            _smokeEffectParticleSystem = new CandleSmokeParticleSystem(assetsLoader);
            _playerCandleFireParticleSystem = new CandleFireParticleSystem(assetsLoader);
            _fireEffectGenerationCenter = _drawingInfos.Center(_animation.FirstFrameSprite.Width, _animation.FirstFrameSprite.Height) - new Vector2(7, 42.0f);
        }

        public void Die()
        {
            IsDying = true;

            if (_fadeObject == null)
            {
                _fadeObject = new FadeObject(
                    _timeToLive,
                    _drawingInfos.OverlayColor);

                _smokeEffect = new ParticleGeneratorTimedEffect(
                    _smokeEffectParticleSystem,
                    _drawingInfos.Center(_animation.FirstFrameSprite.Width, _animation.FirstFrameSprite.Height) - new Vector2(0.0f, 15.0f),
                    TimeSpan.FromMilliseconds(35),
                    _timeToLive);

                _fadeObject.FadeOut();
            }
        }

        public void DieFast()
        {
            _timeToLive = TimeSpan.FromMilliseconds(500);
            Die();
        }

        public bool IsActive
        {
            get
            {
                if (_timeToLive > TimeSpan.Zero)
                    return true;

                if (_smokeEffect != null)
                    return !_smokeEffect.HasFinished;

                return false;
            }
        }

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
                _torch.ForceColor(_fadeObject.OverlayColor);
                LightMap.ForceColor(_fadeObject.OverlayColor);
            }
            else
            {
                _playerCandleFireParticleSystem.AddParticles(_fireEffectGenerationCenter);
            }

            _playerCandleFireParticleSystem.Update(elapsed);
            _animation.Update(elapsed);
            LightMap.Update(elapsed);
            _torch.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation.CurrentFrame, _drawingInfos);
            _torch.Draw(spriteBatch);
            _playerCandleFireParticleSystem.Draw(spriteBatch);
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
