using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Characters
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
        private readonly MarkInDeDarkGame _game;

        public CharacterFallingInTrapAnimation(
            Sprite idleSprite,
            Vector2 trapCenter,
            Character character,
            MarkInDeDarkGame game)
        {
            _frontIdleSprite = idleSprite;
            _drawingInfos = new DrawingInfos()
            {
                Position = trapCenter + new Vector2(0f, 30f),
                Origin = _frontIdleSprite.SpriteCenter,
                LayerDepth = character.CurrentZIndex
            };

            _character = character;
            _game = game;
        }

        public void Update(TimeSpan elapsed)
        {
            if (_currentAnimationDuration > _allAnimationDuration)
            {
                _game.StartCameraShake(10);
                _character.CurrentCharacterState = Character.CharacterState.Dead;
                if (_character.SurroundingLight != null)
                    _character.SurroundingLight.IsEnabled = false;
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

                // Lo faccio per far muovere con lui la luce
                _character.MainFigureDrawingInfos.Position = _drawingInfos.Position;
                if (_character.SurroundingLight != null)
                    _character.SurroundingLight.ForceColor(_drawingInfos.OverlayColor);
            }

        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_frontIdleSprite, _drawingInfos);
    }
}
