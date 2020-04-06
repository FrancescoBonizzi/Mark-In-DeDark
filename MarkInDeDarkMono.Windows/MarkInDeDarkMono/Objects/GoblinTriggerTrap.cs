using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;

using MarkInDeDarkMono.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MarkInDeDarkMono.Objects
{
    public class GoblinTriggerTrap : TriggerObject
    {
        private readonly Sprite _trapSprite;
        private readonly DrawingInfos _drawingInfos;

        public GoblinTriggerTrap(
            AssetsLoader assetsLoader,
            IEnumerable<Character> affectedCharacters,
            Rectangle triggerCollisionRectangle,
            Action<Character, TimeSpan> onTriggerPressed)
            : base(
                triggerCollisionRectangle: triggerCollisionRectangle,
                affectedCharacters: affectedCharacters,
                onTriggerPressed: onTriggerPressed,
                onTriggerLeft: null,
                shouldFireOnce: true)
        {
            _trapSprite = assetsLoader.Sprites["goblin_trigger"];
            _drawingInfos = new DrawingInfos()
            {
                Position = triggerCollisionRectangle.Center.ToVector2(),
                Origin = _trapSprite.SpriteCenter
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_trapSprite, _drawingInfos);
        }
    }
}
