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
        public float Opacity { get; private set; } = 1.0f;

        private readonly SpriteAnimation _walkUpAnimation;
        public DrawingInfos DrawingInfos { get; private set; }

        public PlayerWinAnimation(
            AssetsLoader assetsLoader)
        {
            _walkUpAnimation = assetsLoader.GetAnimation("playerWalkUp");
            DrawingInfos = new DrawingInfos()
            {
                Origin = _walkUpAnimation.FirstFrameSprite.SpriteCenter
            };
        }

        public void SetPosition(Vector2 position)
        {
            DrawingInfos.Position = position;
        }

        public void Update(TimeSpan elapsed)
        {
            DrawingInfos.LayerDepth = ZIndexCalculator.ForGenericObject(
                DrawingInfos.Position - _walkUpAnimation.CurrentFrame.SpriteCenter);

            _currentAnimationDuration -= elapsed;

            if (_currentAnimationDuration <= TimeSpan.Zero)
            {
                _currentAnimationDuration = _defaultInterval;

                Opacity += _opacityDelta;
                DrawingInfos.OverlayColor *= Opacity;
                DrawingInfos.Position += _positionDelta;
            }

            _walkUpAnimation.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_walkUpAnimation.CurrentFrame, DrawingInfos);
    }
}
