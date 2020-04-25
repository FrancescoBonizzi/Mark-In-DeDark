using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.TransformationObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class Light
    {
        private readonly Sprite _additionalLightMask;
        private readonly DrawingInfos _additionalLightMaskDrawingInfos;

        private ScalingObject _scalingObject;
        private readonly float _originalMinScale;
        private readonly float _originalMaxScale;
        private readonly float _originalScalingSpeed;

        private ColorChangingObject _colorChangingObject;
        private readonly Color _firstAnimationColor;
        private readonly Color _secondAnimationColor;
        private readonly double _originalColorChangingSpeed;

        public PointLight PointLight { get; }
        public Vector2 Position
        {
            get
            {
                return PointLight.Position;
            }
            set
            {
                PointLight.Position = value;
                _additionalLightMaskDrawingInfos.Position = value;
            }
        }

        private Color? _forcedColor;

        public Light(
            Sprite lightMask,
            Color firstAnimationColor,
            Color secondAnimationColor,
            float scale)
        {
            _additionalLightMask = lightMask;

            _firstAnimationColor = firstAnimationColor;
            _secondAnimationColor = secondAnimationColor;

            _originalMinScale = MathHelper.Clamp(scale - 0.2f, 0.1f, 1f);
            _originalMaxScale = scale;
            _originalScalingSpeed = 2.5f;
            _scalingObject = new ScalingObject(_originalMinScale, _originalMaxScale, _originalScalingSpeed);

            _originalColorChangingSpeed = 2.5;
            _colorChangingObject = new ColorChangingObject(
                firstAnimationColor, secondAnimationColor, _originalColorChangingSpeed);

            PointLight = new PointLight()
            {
                ShadowType = ShadowType.Solid,
                Intensity = 1f
            };

            _additionalLightMaskDrawingInfos = new DrawingInfos()
            {
                Origin = _additionalLightMask.SpriteCenter
            };
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

        public void Update(TimeSpan elapsed)
        {
            if (_forcedColor == null)
            {
                // Cambiamento di colore
                _colorChangingObject.Update(elapsed);
                PointLight.Color = _colorChangingObject.Color;
            }
            else
            {
                PointLight.Color = _forcedColor.Value;
            }

            // Cambiamento di scala
            _scalingObject.Update(elapsed);

            PointLight.Scale = new Vector2(_scalingObject.Scale * 500);

            _additionalLightMaskDrawingInfos.OverlayColor = PointLight.Color;
            _additionalLightMaskDrawingInfos.Scale = _scalingObject.Scale;
        }

        public void DrawAdditionalLightMask(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_additionalLightMask, _additionalLightMaskDrawingInfos);
    }
}