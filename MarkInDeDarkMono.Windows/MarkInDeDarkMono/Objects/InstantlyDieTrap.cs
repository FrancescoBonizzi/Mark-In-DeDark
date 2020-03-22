using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Characters;
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
                  triggerCollisionRectangle: collisionRectangle.WithTolerance(100),
                  affectedCharacters: affectedCharacters,
                  onTriggerPressed: (character, elapsed) => character.DieForTrap(collisionRectangle.Center.ToVector2()),
                  onTriggerLeft: () => { },
                  shouldFireOnce: false)
        {
            _trapSprite = assetsLoader.Sprites["trap_instantly_die"];
            _drawingInfos = new DrawingInfos()
            {
                Position = collisionRectangle.Center.ToVector2(),
                Origin = _trapSprite.SpriteCenter
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_trapSprite, _drawingInfos);
        }
    }
}
