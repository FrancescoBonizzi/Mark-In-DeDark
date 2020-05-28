using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.TransformationObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Lighting
{
    public class MarkLight
    {
        public float Z { get; set; }
        public float Opacity { get; set; }
        public float DistanceSquared { get; set; }
        public Vector3 PositionForGround { get; set; }
        public Vector3 PositionForWall { get; set; }
        public Vector3 ColorNormal { get; set; } = Vector3.One;

        public bool IsEnabled { get; set; } = true;

        private readonly DrawingInfos _specularGlowDrawingInfos;
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
        private readonly float _originalDistanceSquared;

        public Vector2 Position { get; set; }

        private Color? _forcedColor;

        public MarkLight(
            AssetsLoader assetsLoader,
            Color firstAnimationColor,
            Color secondAnimationColor,
            float z,
            float opacity,
            float distanceSquared)
        {
            Z = z;
            Opacity = opacity;
            DistanceSquared = distanceSquared;
            _originalDistanceSquared = distanceSquared;

            _specularGlowSprite = assetsLoader.Sprites["Effects/glow"];
            _firstAnimationColor = firstAnimationColor;
            _secondAnimationColor = secondAnimationColor;

            _originalMinScale = 0.8f;
            _originalMaxScale = 1f;
            _originalScalingSpeed = 2.5f;
            _scalingObjectForLight = new ScalingObject(_originalMinScale, _originalMaxScale, 2.5f);
            _scalingObjectForSpecular = new ScalingObject(1.4f, 2.4f, _originalScalingSpeed);

            _originalColorChangingSpeed = 2.5;
            _colorChangingObject = new ColorChangingObject(
                firstAnimationColor, secondAnimationColor, _originalColorChangingSpeed);

            _specularGlowDrawingInfos = new DrawingInfos()
            {
                Origin = _specularGlowSprite.SpriteCenter,
                Scale = 2f
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
            }
            else
            {
                ColorNormal = _forcedColor.Value.ToVector3();
            }

            // Cambiamento di scala
            _scalingObjectForLight.Update(elapsed);
            _scalingObjectForSpecular.Update(elapsed);

            DistanceSquared = _scalingObjectForLight.Scale * _originalDistanceSquared;
            ColorNormal = _colorChangingObject.Color.ToVector3();

            // Shake luce
            var shakeIntensity = Numbers.RandomBetween(0, 10);
            var shakeStartAngle = Numbers.RandomBetween(0, 360);
            var offset = new Vector2(
                (float)(Math.Sin(shakeStartAngle) * shakeIntensity),
                (float)(Math.Cos(shakeStartAngle) * shakeIntensity));

            Position = new Vector2(
                MathHelper.Lerp(Position.X, Position.X + offset.X, (float)Math.Sin(elapsed.TotalSeconds * 20)),
                MathHelper.Lerp(Position.Y, Position.Y + offset.Y, (float)Math.Sin(elapsed.TotalSeconds * 20)));

            _specularGlowDrawingInfos.Position = Position + new Vector2(0f, 100f);
            _specularGlowDrawingInfos.Scale = _scalingObjectForSpecular.Scale;
        }

        public void DrawGlow(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_specularGlowSprite, _specularGlowDrawingInfos);
        }
    }
}