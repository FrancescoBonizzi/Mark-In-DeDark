using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Characters
{
    public class PlayerWinAnimation
    {
        private TimeSpan _currentAnimationDuration = TimeSpan.FromMilliseconds(35);
        private TimeSpan _defaultInterval = TimeSpan.FromMilliseconds(10);
        private readonly float _opacityDelta = -0.0009f;
        private readonly Vector2 _positionDelta = new Vector2(0f, -2f);
        //private readonly float _scaleDelta = -0.02f;
        private float _opacity = 1.0f;

        private readonly SpriteAnimation _walkUpAnimation;
        private readonly DrawingInfos _drawingInfos;

        public PlayerWinAnimation(
            AssetsLoader assetsLoader)
        {
            _walkUpAnimation = assetsLoader.GetAnimation("playerWalkUp");
            _drawingInfos = new DrawingInfos()
            {
                Origin = _walkUpAnimation.FirstFrameSprite.SpriteCenter
            };
        }

        public void SetPosition(Vector2 position)
        {
            _drawingInfos.Position = position;
        }

        public void Update(TimeSpan elapsed)
        {
            _drawingInfos.LayerDepth = ZIndexCalculator.ForGenericObject(
                _drawingInfos.Position - _walkUpAnimation.CurrentFrame.SpriteCenter);

            _currentAnimationDuration -= elapsed;

            if (_currentAnimationDuration <= TimeSpan.Zero)
            {
                _currentAnimationDuration = _defaultInterval;

                _opacity += _opacityDelta;
                _drawingInfos.OverlayColor *= _opacity;
                _drawingInfos.Position += _positionDelta;
            }

            _walkUpAnimation.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_walkUpAnimation.CurrentFrame, _drawingInfos);
    }
}
