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
        public DrawingInfos AloneDrawingInfos { get; }

        private readonly Sprite _aloneSprite;

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
                return _additionalLightMaskDrawingInfos.Position;
            }
            set
            {
                PointLight.Position = value + new Vector2(0, 80f);
                _additionalLightMaskDrawingInfos.Position = value;
                AloneDrawingInfos.Position = value;
            }
        }

        private Color? _forcedColor;

        public bool AloneEnabled { get; set; }

        public Light(
            AssetsLoader assetsLoader,
            Color firstAnimationColor,
            Color secondAnimationColor,
            float glowingLightScale,
            float layerDepth,
            float penumbraLightScale)
        {
            _additionalLightMask = assetsLoader.Sprites["Effects/light_1"];
            _aloneSprite = assetsLoader.Sprites["Effects/light_pixel"];
            _firstAnimationColor = firstAnimationColor;
            _secondAnimationColor = secondAnimationColor;

            _originalMinScale = MathHelper.Clamp(glowingLightScale - 0.2f, 0.1f, 1f);
            _originalMaxScale = glowingLightScale;
            _originalScalingSpeed = 2.5f;
            _scalingObject = new ScalingObject(_originalMinScale, _originalMaxScale, _originalScalingSpeed);

            _originalColorChangingSpeed = 2.5;
            _colorChangingObject = new ColorChangingObject(
                firstAnimationColor, secondAnimationColor, _originalColorChangingSpeed);

            PointLight = new PointLight()
            {
                ShadowType = ShadowType.Solid,
                Intensity = 1,
                Scale = new Vector2(penumbraLightScale)
            };

            _additionalLightMaskDrawingInfos = new DrawingInfos()
            {
                Origin = _additionalLightMask.SpriteCenter
            };

            AloneDrawingInfos = new DrawingInfos()
            {
                Origin = _aloneSprite.SpriteCenter,
                LayerDepth = layerDepth
            };
        }

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

            _additionalLightMaskDrawingInfos.OverlayColor = PointLight.Color;
            _additionalLightMaskDrawingInfos.Scale = _scalingObject.Scale;

            AloneDrawingInfos.OverlayColor = PointLight.Color;
            AloneDrawingInfos.Scale = _scalingObject.Scale;
        }

        public void DrawAdditionalLightMask(SpriteBatch spriteBatch)
        {
            if (_additionalLightMask != null)
                spriteBatch.Draw(_additionalLightMask, _additionalLightMaskDrawingInfos);
        }

        public void DrawAlone(SpriteBatch spriteBatch)
        {
            if (AloneEnabled)
                spriteBatch.Draw(_aloneSprite, AloneDrawingInfos);
        }
    }
}