using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.Drawing;
using FlowsoftGamesMonogame.Extensions;
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
                  collisionRectangle.WithTolerance(60),
                  affectedCharacters,
                  (character, elapsed) => character.DieForTrap(
                      new Vector2(collisionRectangle.X, collisionRectangle.Y)),
                  false)
        {
            _trapSprite = assetsLoader.Sprites["trap_instantly_die"];
            _drawingInfos = new DrawingInfos()
            {
                Position = new Vector2(
                    collisionRectangle.X, 
                    collisionRectangle.Y),
                Origin = collisionRectangle.Center.ToVector2()
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_trapSprite, _drawingInfos);
        }
    }
}
