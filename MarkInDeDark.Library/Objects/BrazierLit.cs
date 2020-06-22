using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Objects
{
    public class BrazierLit
    {
        private readonly SpriteAnimation _brazierAnimation;
        private readonly SpriteAnimation _shadowAnimation;
        private readonly Sprite _normal;

        private readonly DrawingInfos _brazierAnimationDrawingInfos;
        private readonly DrawingInfos _shadowLargeAnimationDrawingInfos;
        private readonly DrawingInfos _shadowAnimationDrawingInfos;

        public MarkLight SurroundingLight { get; }

        public BrazierLit(
            AssetsLoader assetsLoader,
            GameConfig gameConfig,
            Vector2 position)
        {
            _brazierAnimation = assetsLoader.GetAnimation("BrazierLit");
            _shadowAnimation = assetsLoader.GetAnimation("GenericCharacterWhileIdleUpShadow");

            _brazierAnimationDrawingInfos = new DrawingInfos()
            {
                Position = position,
                Origin = _brazierAnimation.FirstFrameSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForGenericObject(position)
            };

            _shadowAnimationDrawingInfos = new DrawingInfos()
            {
                Position = position + new Vector2(0, 40),
                Origin = _shadowAnimation.FirstFrameSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForShadow(),
                Scale = 1f,
                OverlayColor = Color.White.WithAlpha(0.5f)
            };

            _shadowLargeAnimationDrawingInfos = new DrawingInfos()
            {
                Position = position + new Vector2(0, -10),
                Origin = _shadowAnimationDrawingInfos.Origin,
                LayerDepth = _shadowAnimationDrawingInfos.LayerDepth,
                Scale = 2f,
                OverlayColor = _shadowAnimationDrawingInfos.OverlayColor
            };

            _normal = assetsLoader.Sprites["Objects/Brazier/brazier_normal"];

            SurroundingLight = new MarkLight(
                assetsLoader,
                gameConfig.CandleColorRGB1,
                gameConfig.CandleColorRGB2,
                z: 67f,
                opacity: 1f,
                distanceSquared: 7_280f,
                castsShadows: false,
                penumbraLightScale: 150f)
            {
                Position = WorldCenter + new Vector2(0, -50f)
            };

        }

        public void Update(TimeSpan elapsed)
        {
            _brazierAnimation.Update(elapsed);
            _shadowAnimation.Update(elapsed);
            SurroundingLight.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_brazierAnimation.CurrentFrame, _brazierAnimationDrawingInfos);
            spriteBatch.Draw(_shadowAnimation.CurrentFrame, _shadowLargeAnimationDrawingInfos);
            spriteBatch.Draw(_shadowAnimation.CurrentFrame, _shadowAnimationDrawingInfos);
        }

        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_normal, _brazierAnimationDrawingInfos);
        }

        public Vector2 WorldCenter
            => _brazierAnimationDrawingInfos.Center(_brazierAnimation.CurrentFrameWidth, _brazierAnimation.CurrentFrameHeight);

        public Vector2 Position
            => _brazierAnimationDrawingInfos.Position;
    }
}
