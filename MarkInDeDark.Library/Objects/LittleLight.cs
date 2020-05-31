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
    public class LittleLight
    {
        private readonly SpriteAnimation _animation;
        private readonly DrawingInfos _drawingInfos;
 //       private readonly LittleLightParticleSystem _littleLightParticle;
        private readonly Vector2 _fireEffectGenerationCenter;

        public MarkLight SurroundingLight { get; }

        public LittleLight(
            AssetsLoader assetsLoader,
            GameConfig gameConfig,
            Vector2 position)
        {
            _animation = assetsLoader.GetAnimation("LittleLight");

            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                Origin = _animation.FirstFrameSprite.SpriteCenter,
                Scale = 3.0f,
                LayerDepth = ZIndexCalculator.ForGenericObject(position)
            };

            // Rimosso momentaneamente per performance. Comunque mettere poche particelle. 
            // Faceva 1% di CPU per luce
            //_littleLightParticle = new LittleLightParticleSystem(assetsLoader)
            //{
            //    LayerDepth = _drawingInfos.LayerDepth + 0.00001f
            //};

            SurroundingLight = new MarkLight(
                assetsLoader,
                gameConfig.LittleLightColorRGB1,
                gameConfig.LittleLightColorRGB2,
                z: 67f,
                opacity: 1f,
                distanceSquared: 7_280f,
                castsShadows: false,
                penumbraLightScale: 150f,
                lightingMode: LightingMode.ThreePerThree)
            {
                Position = WorldCenter
            };

            _fireEffectGenerationCenter = _drawingInfos.Center(_animation.FirstFrameSprite.Width, _animation.FirstFrameSprite.Height) - new Vector2(0, 10f);
        }

        public void Update(TimeSpan elapsed)
        {
            // Rimosso momentaneamente per performance. Comunque mettere poche particelle
            //_littleLightParticle.AddParticles(_fireEffectGenerationCenter);
            //_littleLightParticle.Update(elapsed);
            _animation.Update(elapsed);
            SurroundingLight.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation.CurrentFrame, _drawingInfos);
            // Rimosso momentaneamente per performance. Comunque mettere poche particelle
            //_littleLightParticle.Draw(spriteBatch);
        }

        public Vector2 WorldCenter
            => _drawingInfos.Center(_animation.CurrentFrameWidth, _animation.CurrentFrameHeight);

        public Vector2 Position
            => _drawingInfos.Position;
    }
}
