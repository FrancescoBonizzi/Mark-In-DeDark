using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.TransformationObjects;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Lighting;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library
{
    public class Candle
    {
        public MarkLight SurroundingLight { get; }

        private readonly CandleFireParticleSystem _candleFireParticleSystem;

        private readonly Vector2 _fireEffectGenerationCenter;
        private readonly DrawingInfos _drawingInfos;
        private FadeObject _fadeObject;

        private TimeSpan _timeToLive;

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
                LayerDepth = ZIndexCalculator.ForCandle(position)
            };

            SurroundingLight = new MarkLight(
                assetsLoader: assetsLoader,
                firstAnimationColor: gameConfig.CandleColorRGB1,
                secondAnimationColor: gameConfig.CandleColorRGB2,
                z: 67f,
                opacity: 1f,
                distanceSquared: 7_280f)
            {
                Position = new Vector2(WorldCenter.X, WorldCenter.Y - 15),
            };

            IsDying = false;
            AreSomeMonsterIsFollowingMe = false;

            _timeToLive = TimeSpan.FromSeconds(gameConfig.CandleTimeToLiveWhenAttackedSeconds);

            _candleFireParticleSystem = new CandleFireParticleSystem(assetsLoader)
            {
                LayerDepth = _drawingInfos.LayerDepth + 0.00002f
            };

            _fireEffectGenerationCenter = _drawingInfos.Center(_animation.FirstFrameSprite.Width, _animation.FirstFrameSprite.Height) - new Vector2(7, 30.0f);
        }

        public void Die(TimeSpan? timeToLive = null)
        {
            if (timeToLive != null)
                _timeToLive = timeToLive.Value;

            IsDying = true;

            if (_fadeObject == null)
            {
                _fadeObject = new FadeObject(
                    _timeToLive,
                    _drawingInfos.OverlayColor);
                _fadeObject.FadeOut();
            }
        }

       
        public bool IsAlive
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
                
                _fadeObject.Update(elapsed);
                _drawingInfos.OverlayColor = _fadeObject.OverlayColor;
                SurroundingLight.ForceColor(_fadeObject.OverlayColor);
            }
            else
            {
                _candleFireParticleSystem.AddParticles(_fireEffectGenerationCenter);
            }

            _candleFireParticleSystem.Update(elapsed);
            _animation.Update(elapsed);
            SurroundingLight.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation.CurrentFrame, _drawingInfos);
        }

        public void DrawEffects(SpriteBatch spriteBatch)
        {
            _candleFireParticleSystem.Draw(spriteBatch);
        }
    }
}
