using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;

using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Objects
{
    public class InstantlyDieTrap : TriggerObject
    {
        private readonly SpriteAnimation _animation;
        private readonly DrawingInfos _drawingInfos;

        public InstantlyDieTrap(
            AssetsLoader assetsLoader,
            IEnumerable<Character> affectedCharacters,
            Rectangle collisionRectangle)
            : base(
                  triggerCollisionRectangle: collisionRectangle.WithTolerance(new Rectangle(70, 70, 140, 140)),
                  affectedCharacters: affectedCharacters,
                  onTriggerPressed: (character, elapsed) => { },
                  onTriggerLeft: null,
                  shouldFireOnce: false)
        {
            _animation = assetsLoader.GetAnimation("TrapIstant");
            _drawingInfos = new DrawingInfos()
            {
                Position = collisionRectangle.Center.ToVector2(),
                Origin = _animation.FirstFrameSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForGroundObjects
            };

            _animation.Stop();

            SetOnTriggerPressed(
                (character, elapsed) =>
                {
                    character.DieForTrap(collisionRectangle.Center.ToVector2());
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
    }
}
