using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;

using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MarkInDeDarkMono.Objects
{
    public class InstantlyDieTrap : TriggerObject
    {
        private readonly Sprite _trapSprite;
        private readonly DrawingInfos _drawingInfos;

        public InstantlyDieTrap(
            AssetsLoader assetsLoader,
            IEnumerable<Character> affectedCharacters,
            Rectangle collisionRectangle)
            : base(
                  triggerCollisionRectangle: collisionRectangle.WithTolerance(new Rectangle(70, 70, 140, 140)),
                  affectedCharacters: affectedCharacters,
                  onTriggerPressed: (character, elapsed) => character.DieForTrap(collisionRectangle.Center.ToVector2()),
                  onTriggerLeft: null,
                  shouldFireOnce: false)
        {
            _trapSprite = assetsLoader.Sprites["trap_instantly_die"];
            _drawingInfos = new DrawingInfos()
            {
                Position = collisionRectangle.Center.ToVector2(),
                Origin = _trapSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.Calculate(collisionRectangle.Center.ToVector2() - _trapSprite.SpriteCenter)
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_trapSprite, _drawingInfos);
    }
}
