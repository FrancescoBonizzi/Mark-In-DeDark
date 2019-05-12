using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.Drawing;
using FlowsoftGamesMonogame.Extensions;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Characters
{
    public class PlayerDescendingStairs
    {
        private TimeSpan default_interval_ = TimeSpan.FromMilliseconds(35);
        private TimeSpan _animationDuration = TimeSpan.FromMilliseconds(35);
        private float _opacityDelta = -0.009f;
        private float _scaleDelta = -0.02f;
        private float _opacity = 1.0f;

        private readonly SpriteAnimation _walkUpAnimation;
        private readonly DrawingInfos _drawingInfos;

        public PlayerDescendingStairs(
            AssetsLoader assetsLoader)
        {
            _walkUpAnimation = assetsLoader.GetAnimation("playerWalkUp");
            _drawingInfos = new DrawingInfos()
            {
                Origin = _walkUpAnimation.FirstFrameSprite.SpriteCenter,
            };
        }

        public void SetPosition(Vector2 position)
        {
            _drawingInfos.Position = position;
        }

        public void Update(TimeSpan elapsed)
        {
            _animationDuration -= elapsed;

            if (_animationDuration <= TimeSpan.Zero)
            {
                _animationDuration = default_interval_;

                _opacity += _opacityDelta;
                _drawingInfos.Scale += _scaleDelta;
                _drawingInfos.OverlayColor *= _opacity;
            }

            _walkUpAnimation.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_walkUpAnimation.CurrentFrame, _drawingInfos);
    }
}
