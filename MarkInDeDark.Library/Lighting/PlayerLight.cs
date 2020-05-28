using FbonizziMonoGame.TransformationObjects;
using Microsoft.Xna.Framework;
using Penumbra;
using System;

namespace MarkInDeDark.Library.Lighting
{
    public class PlayerLight : MarkLight
    {
        public PointLight PenumbraLight { get; }

        private const float _penumbraLightScale = 1200f;
        private readonly ScalingObject _scalingObjectForPenumbraLight;

        public PlayerLight(
            AssetsLoader assetsLoader,
            Color firstAnimationColor,
            Color secondAnimationColor) 
            : base(assetsLoader: assetsLoader,
                  firstAnimationColor: firstAnimationColor,
                  secondAnimationColor: secondAnimationColor,
                  z: 90f,
                  opacity: 1f,
                  distanceSquared: 20_600f)
        {
            _scalingObjectForPenumbraLight = new ScalingObject(
                _penumbraLightScale * 0.8f, 
                _penumbraLightScale * 1.5f, 2.5f);

            PenumbraLight = new PointLight()
            {
                CastsShadows = true,
                Scale = new Vector2(_penumbraLightScale),
                Color = firstAnimationColor,
                Enabled = true,
                Intensity = 1,
                ShadowType = ShadowType.Solid
            };

        }

        public override void Update(TimeSpan elapsed)
        {
            base.Update(elapsed);

            _scalingObjectForPenumbraLight.Update(elapsed);
            PenumbraLight.Scale = new Vector2(_scalingObjectForPenumbraLight.Scale);
            PenumbraLight.Position = Position;
            PenumbraLight.Color = _colorChangingObject.Color;
        }
    }
}
