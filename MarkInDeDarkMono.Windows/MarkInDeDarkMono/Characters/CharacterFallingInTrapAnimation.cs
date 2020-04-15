using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Characters
{
    public class CharacterFallingInTrapAnimation
    {
        private TimeSpan _currentStepDuration = TimeSpan.FromMilliseconds(35);
        private TimeSpan _intervalBetweenSteps = TimeSpan.FromMilliseconds(35);

        private TimeSpan _allAnimationDuration = TimeSpan.FromSeconds(1);
        private TimeSpan _currentAnimationDuration = TimeSpan.Zero;

        private readonly float _opacityDelta = -0.009f;
        private readonly float _scaleDelta = -0.07f;
        private readonly float _rotationDelta = 0.02f;
        private readonly float _deltaYPosition = 5f;
        private float _opacity = 1.0f;

        private readonly Sprite _frontIdleSprite;
        private readonly DrawingInfos _drawingInfos;
        private readonly Character _character;

        public CharacterFallingInTrapAnimation(
            Sprite idleSprite,
            Vector2 trapCenter,
            Character character)
        {
            _frontIdleSprite = idleSprite;
            _drawingInfos = new DrawingInfos()
            {
                Position = trapCenter + new Vector2(0f, 30f),
                Origin = _frontIdleSprite.SpriteCenter,
                LayerDepth = character.CurrentZIndex
            };

            _character = character;
        }

        public void SetPosition(Vector2 position)
        {
            _drawingInfos.Position = position;
        }

        public void Update(TimeSpan elapsed)
        {
            if (_currentAnimationDuration > _allAnimationDuration)
            {
                _character.CurrentCharacterState = Character.CharacterState.Dead;
                return;
            }

            _currentStepDuration -= elapsed;
            _currentAnimationDuration += elapsed;

            if (_currentStepDuration <= TimeSpan.Zero)
            {
                _currentStepDuration = _intervalBetweenSteps;
                _opacity += _opacityDelta;
                _drawingInfos.Scale += _scaleDelta;
                if (_drawingInfos.Scale <= 0)
                    _drawingInfos.Scale = 0.08f;
                _drawingInfos.OverlayColor *= _opacity;
                _drawingInfos.Rotation += _rotationDelta;

                _drawingInfos.Position = new Vector2(
                    _drawingInfos.Position.X, 
                    _drawingInfos.Position.Y + _deltaYPosition);
            }

        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_frontIdleSprite, _drawingInfos);
    }
}
