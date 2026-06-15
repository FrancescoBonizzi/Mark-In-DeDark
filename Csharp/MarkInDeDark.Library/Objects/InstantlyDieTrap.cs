using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace MarkInDeDark.Library.Objects
{
    public class InstantlyDieTrap : TriggerObject
    {
        private readonly SpriteAnimation _animation;
        private readonly DrawingInfos _drawingInfos;

        public InstantlyDieTrap(
            AssetsLoader assetsLoader,
            IEnumerable<Character> affectedCharacters,
            Rectangle collisionRectangle,
            GameGrid gameGrid)
            : base(
                  triggerCollisionRectangle: collisionRectangle.WithTolerance(new Rectangle(70, 70, 140, 140)),
                  affectedCharacters: affectedCharacters,
                  onTriggerPressed: (character, elapsed) => { },
                  onTriggerLeft: null,
                  shouldFireOnce: false,
                  gameGrid: gameGrid)
        {
            _animation = assetsLoader.GetAnimation("Grounds/TrapHole");
            _drawingInfos = new DrawingInfos()
            {
                Position = collisionRectangle.Center.ToVector2(),
                Origin = _animation.FirstFrameSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.GroundObjectsLayerDepth
            };

            _animation.Stop();

            SetOnTriggerPressed(
                (character, elapsed) =>
                {
                    character.FallInTrap(collisionRectangle.Center.ToVector2());
                    _animation.Play();
                });
        }

        public override void Update(TimeSpan elapsed)
        {
            _animation.Update(elapsed);
            base.Update(elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_animation.CurrentFrame, _drawingInfos);

        public override void DrawNormal(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_animation.CurrentFrameNormalMap, _drawingInfos);
    }
}
