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
        private readonly Sprite _glow;

        private readonly DrawingInfos _brazierAnimationDrawingInfos;
        private readonly DrawingInfos _shadowAnimationDrawingInfos;
        private readonly DrawingInfos _glowDrawingInfos;

        public MarkLight SurroundingLight { get; }

        public BrazierLit(
            AssetsLoader assetsLoader,
            GameConfig gameConfig,
            Vector2 position)
        {
            _brazierAnimation = assetsLoader.GetAnimation("BrazierLit");
            _shadowAnimation = assetsLoader.GetAnimation("BrazierShadow");

            _normal = assetsLoader.Sprites["Objects/Brazier/brazier_normal"];
            _glow = assetsLoader.Sprites["Effects/glow"];

            _brazierAnimationDrawingInfos = new DrawingInfos()
            {
                Position = position,
                Origin = _brazierAnimation.FirstFrameSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForGenericObject(position)
            };

            _shadowAnimationDrawingInfos = new DrawingInfos()
            {
                Position = position,
                Origin = _shadowAnimation.FirstFrameSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForShadow(),
                OverlayColor = Color.Black.WithAlpha(0.8f)
            };

            _glowDrawingInfos = new DrawingInfos()
            {
                Position = position + new Vector2(0f, -20f),
                Origin = _glow.SpriteCenter,
                LayerDepth = ZIndexCalculator.OverAllLayerDepth,
                Scale = 0.7f//, // Parte da 256pixel, per avere ~50pixel
                //OverlayColor = Color.White.WithAlpha(0.5f)
            };

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
                Position = WorldCenter + new Vector2(0, -50f),
                DeltaForWall = new Vector2(0, 4),
                DistanceSquaredForWall = 17_000f
            };

            SurroundingLight.SetGlowOpacity(0.25f);
        }

        public void Update(TimeSpan elapsed)
        {
            _brazierAnimation.Update(elapsed);
            _shadowAnimation.Update(elapsed);
            SurroundingLight.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_brazierAnimation.CurrentFrame, _brazierAnimationDrawingInfos);

        public void DrawOverAllGlow(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_glow, _glowDrawingInfos);

        public void DrawNormalMap(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_normal, _brazierAnimationDrawingInfos);

        public void DrawShadows(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_shadowAnimation.CurrentFrame, _shadowAnimationDrawingInfos);

        public Vector2 WorldCenter
            => _brazierAnimationDrawingInfos.Center(_brazierAnimation.CurrentFrameWidth, _brazierAnimation.CurrentFrameHeight);

        public Vector2 Position
            => _brazierAnimationDrawingInfos.Position;
    }
}
