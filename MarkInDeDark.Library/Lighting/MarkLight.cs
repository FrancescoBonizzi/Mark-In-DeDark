using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.TransformationObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;

namespace MarkInDeDark.Library.Lighting
{
    public class MarkLight
    {
        public Vector2 Position { get; set; }
        public Rectangle BoundingRectangle => new Rectangle(
            (int)Position.X - 150,
            (int)Position.Y - 150,
            300,
            300);

        public float Z { get; set; }
        public float Opacity { get; set; }
        public float DistanceSquared { get; set; }
        public Vector3 PositionForGround { get; set; }
        public Vector3 PositionForWall { get; set; }
        public Vector3 ColorNormal { get; set; } = Vector3.One;

        public Vector2 DeltaForWall { get; set; } = new Vector2(0, -60f);
        public Vector2 DeltaForGround { get; set; } = new Vector2(0, 100f);

        public PointLight PenumbraLight { get; }

        public bool IsEnabled { get; set; } = true;
        public bool IsGlowEnabled { get; set; } = true;
        public bool IsOutOfScreen { get; set; }

        public bool ShouldDraw => IsEnabled && !IsOutOfScreen;
        

        private readonly DrawingInfos _specularGlowDrawingInfos;
        private readonly DrawingInfos _lightMaskDrawingInfos;
        private readonly Sprite _specularGlowSprite;

        private ScalingObject _scalingObjectForSpecular;
        private ScalingObject _scalingObjectForLight;
        private readonly float _originalMinScale;
        private readonly float _originalMaxScale;
        private readonly float _originalScalingSpeed;

        protected ColorChangingObject _colorChangingObject;
        private readonly Color _firstAnimationColor;
        private readonly Color _secondAnimationColor;
        private readonly double _originalColorChangingSpeed;
        //    private readonly float _originalDistanceSquared;

        private Color? _forcedColor;

        private readonly ScalingObject _scalingObjectForPenumbraLight;

        public MarkLight(
            AssetsLoader assetsLoader,
            Color firstAnimationColor,
            Color secondAnimationColor,
            float z,
            float opacity,
            float distanceSquared,
            bool castsShadows,
            float penumbraLightScale)
        {
            Z = z;
            Opacity = opacity;
            DistanceSquared = distanceSquared;
            //       _originalDistanceSquared = distanceSquared;

            _specularGlowSprite = assetsLoader.Sprites["Effects/glow"];
            _firstAnimationColor = firstAnimationColor;
            _secondAnimationColor = secondAnimationColor;

            _originalMinScale = 0.8f;
            _originalMaxScale = 1f;
            _originalScalingSpeed = 2.5f;
            //      _scalingObjectForLight = new ScalingObject(_originalMinScale, _originalMaxScale, 2.5f);
            //          _scalingObjectForSpecular = new ScalingObject(1.4f, 2.4f, _originalScalingSpeed);

            _originalColorChangingSpeed = 2.5;
            _colorChangingObject = new ColorChangingObject(
                firstAnimationColor, secondAnimationColor, _originalColorChangingSpeed);

            _specularGlowDrawingInfos = new DrawingInfos()
            {
                Origin = _specularGlowSprite.SpriteCenter,
                Scale = MarkInDeDarkGame.LevelCustomization.GlowScale
            };

            _lightMaskDrawingInfos = new DrawingInfos()
            {
                Origin = _specularGlowSprite.SpriteCenter,
                Scale = 2f
            };

            //_scalingObjectForPenumbraLight = new ScalingObject(
            //    penumbraLightScale * 0.8f,
            //    penumbraLightScale * 1.5f, 2.5f);

            PenumbraLight = new PointLight()
            {
                CastsShadows = castsShadows,
                Scale = new Vector2(penumbraLightScale),
                Color = firstAnimationColor,
                Enabled = false,
                Intensity = 1,
                ShadowType = ShadowType.Solid
            };
        }

        public void SetTemporaryColorForAnimation(Color color)
        {
            _colorChangingObject = new ColorChangingObject(
                _firstAnimationColor,
                color,
                _originalColorChangingSpeed * 4);
        }

        public void ForceColor(Color color)
        {
            _forcedColor = color;
        }

        public void ResetColor()
        {
            _colorChangingObject = new ColorChangingObject(
                _firstAnimationColor,
                _secondAnimationColor,
                _originalColorChangingSpeed);
        }

        public virtual void Update(TimeSpan elapsed)
        {
            if (_forcedColor == null)
            {
                _colorChangingObject.Update(elapsed);
                PenumbraLight.Color = _colorChangingObject.Color;
                ColorNormal = _colorChangingObject.Color.ToVector3();
            }
            else
            {
                ColorNormal = _forcedColor.Value.ToVector3();
                PenumbraLight.Color = _forcedColor.Value;
            }

            // Cambiamento di scala
            //           _scalingObjectForLight.Update(elapsed);
            //         _scalingObjectForSpecular.Update(elapsed);
            //         _scalingObjectForPenumbraLight.Update(elapsed);

            //         DistanceSquared = _scalingObjectForLight.Scale * _originalDistanceSquared;
            
            // Shake luce
#warning RIMETTERE. Ma non può basarsi su Position, altrimenti si continua a spostare!
            //var shakeIntensity = Numbers.RandomBetween(0, 10);
            //var shakeStartAngle = Numbers.RandomBetween(0, 360);
            //var offset = new Vector2(
            //    (float)(Math.Sin(shakeStartAngle) * shakeIntensity),
            //    (float)(Math.Cos(shakeStartAngle) * shakeIntensity));

            //Position = new Vector2(
            //    MathHelper.Lerp(Position.X, Position.X + offset.X, (float)Math.Sin(elapsed.TotalSeconds * 20)),
            //    MathHelper.Lerp(Position.Y, Position.Y + offset.Y, (float)Math.Sin(elapsed.TotalSeconds * 20)));

            _specularGlowDrawingInfos.Position = Position + new Vector2(0f, 100f);
            //        _specularGlowDrawingInfos.Scale = _scalingObjectForSpecular.Scale;
            _lightMaskDrawingInfos.Position = _specularGlowDrawingInfos.Position;

            //          PenumbraLight.Scale = new Vector2(_scalingObjectForPenumbraLight.Scale);
            PenumbraLight.Position = Position + new Vector2(-10f, 75f);
        }

        public void DrawGlowForSpecular(SpriteBatch spriteBatch)
        {
            if (!ShouldDraw)
                return;

            if (!IsGlowEnabled)
                return;

            spriteBatch.Draw(_specularGlowSprite, _specularGlowDrawingInfos);
        }

        public void DrawLightMask(SpriteBatch spriteBatch)
        {
            if (!ShouldDraw)
                return;

            if (!IsGlowEnabled)
                return;

            spriteBatch.Draw(_specularGlowSprite, _lightMaskDrawingInfos);
        }
    }
}