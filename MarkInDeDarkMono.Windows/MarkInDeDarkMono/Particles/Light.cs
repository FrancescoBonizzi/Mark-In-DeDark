using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.TransformationObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Particles
{
    public class Light
    {
        private readonly Sprite _sprite;
        public DrawingInfos DrawingInfos { get; }

        private ScalingObject _scalingObject;
        private readonly float _originalMinScale;
        private readonly float _originalMaxScale;
        private readonly float _originalScalingSpeed;

        private ColorChangingObject _colorChangingObject;
        private readonly Color _firstAnimationColor;
        private readonly Color _secondAnimationColor;
        private readonly double _originalColorChangingSpeed;

        private Color? _forcedColor;

        public Light(
            Sprite sprite,
            Color firstAnimationColor,
            Color secondAnimationColor,
            float scale)
        {
            _sprite = sprite;

            DrawingInfos = new DrawingInfos()
            {
                Origin = sprite.SpriteCenter
            };

            _firstAnimationColor = firstAnimationColor;
            _secondAnimationColor = secondAnimationColor;

            _originalMinScale = MathHelper.Clamp(scale - 0.2f, 0.1f, 1f);
            _originalMaxScale = scale;
            _originalScalingSpeed = 2.5f;
            _scalingObject = new ScalingObject(_originalMinScale, _originalMaxScale, _originalScalingSpeed);

            _originalColorChangingSpeed = 2.5;
            _colorChangingObject = new ColorChangingObject(
                firstAnimationColor, secondAnimationColor, _originalColorChangingSpeed);
        }

        public Light(Sprite sprite, float scale = 1.0f)
            : this(sprite, Color.White, Color.White.WithAlpha(0.9f), scale) { }

        public void SetTemporaryColorForAnimation(Color color)
        {
            _colorChangingObject = new ColorChangingObject(
                _firstAnimationColor, color, _originalColorChangingSpeed * 4);
            _scalingObject = new ScalingObject(
                _originalMinScale * 1.5f, _originalMaxScale * 1.5f, _originalScalingSpeed * 2f);
        }

        public void ForceColor(Color color)
        {
            _forcedColor = color;
        }

        public void ResetColor()
        {
            _colorChangingObject = new ColorChangingObject(
                _firstAnimationColor, _secondAnimationColor, _originalColorChangingSpeed);
            _scalingObject = new ScalingObject(
                _originalMinScale, _originalMaxScale, _originalScalingSpeed);
        }

        public void SetPosition(Vector2 newPosition)
            => DrawingInfos.Position = newPosition;

        public Vector2 Position
            => DrawingInfos.Position;

        public void Update(TimeSpan elapsed)
        {
            if (_forcedColor == null)
            {
                // Cambiamento di colore
                _colorChangingObject.Update(elapsed);
                DrawingInfos.OverlayColor = _colorChangingObject.Color;
            }
            else
            {
                DrawingInfos.OverlayColor = _forcedColor.Value;
            }

            // Cambiamento di scala
            _scalingObject.Update(elapsed);
            DrawingInfos.Scale = _scalingObject.Scale;
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);
    }
}