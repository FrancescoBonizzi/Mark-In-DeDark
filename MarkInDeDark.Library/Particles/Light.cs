using FbonizziMonoGame;
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
        public DrawingInfos AloneDrawingInfos { get; }

        private readonly Sprite _aloneSprite;

        private ScalingObject _scalingObjectForAlone;
        private ScalingObject _scalingObjectForPenumbraLight;
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
                return AloneDrawingInfos.Position;
            }
            set
            {
                PointLight.Position = value + new Vector2(0, 80f);
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
            _aloneSprite = assetsLoader.Sprites["Effects/light_pixel"];
            _firstAnimationColor = firstAnimationColor;
            _secondAnimationColor = secondAnimationColor;

            _originalMinScale = MathHelper.Clamp(glowingLightScale - 0.2f, 0.1f, 1f);
            _originalMaxScale = glowingLightScale;
            _originalScalingSpeed = 2.5f;
            _scalingObjectForAlone = new ScalingObject(_originalMinScale, _originalMaxScale, _originalScalingSpeed);
            _scalingObjectForPenumbraLight = new ScalingObject(penumbraLightScale * 0.8f, penumbraLightScale * 1.5f, 2.5f);

            _originalColorChangingSpeed = 2.5;
            _colorChangingObject = new ColorChangingObject(
                firstAnimationColor, secondAnimationColor, _originalColorChangingSpeed);

            PointLight = new PointLight()
            {
                ShadowType = ShadowType.Solid,
                Intensity = 1,
                Scale = new Vector2(penumbraLightScale)
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
            _scalingObjectForAlone = new ScalingObject(
                _originalMinScale * 1.5f, _originalMaxScale * 1.5f, _originalScalingSpeed * 2f);
            _scalingObjectForPenumbraLight = new ScalingObject(
                PointLight.Scale.X * 1.5f, PointLight.Scale.Y * 1.5f, _originalScalingSpeed * 2f);
        }

        public void ForceColor(Color color)
        {
            _forcedColor = color;
        }

        public void ResetColor()
        {
            _colorChangingObject = new ColorChangingObject(
                _firstAnimationColor, _secondAnimationColor, _originalColorChangingSpeed);
            _scalingObjectForAlone = new ScalingObject(
                _originalMinScale, _originalMaxScale, _originalScalingSpeed);
        }

        public void Update(TimeSpan elapsed)
        {
            if (_forcedColor == null)
            {
                // Cambiamento di colore
                _colorChangingObject.Update(elapsed);
                PointLight.Color = _colorChangingObject.Color;
                AloneDrawingInfos.OverlayColor = _colorChangingObject.Color;
            }
            else
            {
                PointLight.Color = _forcedColor.Value;
                AloneDrawingInfos.OverlayColor = _forcedColor.Value;
            }

            // Cambiamento di scala
            _scalingObjectForAlone.Update(elapsed);
            _scalingObjectForPenumbraLight.Update(elapsed);

            PointLight.Scale = new Vector2(_scalingObjectForPenumbraLight.Scale);
            AloneDrawingInfos.Scale = _scalingObjectForAlone.Scale;

            // TODO Shake luce
            //var offset = new Vector2(
            //    (float)(Math.Sin(_shakeStartAngle) * _shakeIntensity), 
            //    (float)(Math.Cos(_shakeStartAngle) * _shakeIntensity));
            //_shakeIntensity = Numbers.RandomBetween(0, 10);
            //_shakeStartAngle += (150 + Numbers.RandomBetween(0, 60));
            //PointLight.Position += offset;
        }

        public void DrawAlone(SpriteBatch spriteBatch)
        {
            if (AloneEnabled)
                spriteBatch.Draw(_aloneSprite, AloneDrawingInfos);
        }
    }
}