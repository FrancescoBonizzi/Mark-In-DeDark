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

namespace MarkInDeDark.Library.Objects
{
    public class GoblinTriggerTrap : TriggerObject
    {
        private readonly Sprite _trapSprite;
        private readonly DrawingInfos _drawingInfos;

        public GoblinTriggerTrap(
            AssetsLoader assetsLoader,
            IEnumerable<Character> affectedCharacters,
            Rectangle triggerCollisionRectangle,
            Action<Character, TimeSpan> onTriggerPressed,
            GameGrid gameGrid)
            : base(
                triggerCollisionRectangle: triggerCollisionRectangle,
                affectedCharacters: affectedCharacters,
                onTriggerPressed: onTriggerPressed,
                onTriggerLeft: null,
                shouldFireOnce: true,
                gameGrid: gameGrid)
        {
            _trapSprite = assetsLoader.Sprites["Grounds/goblin_trigger"];
            _drawingInfos = new DrawingInfos()
            {
                Position = triggerCollisionRectangle.Center.ToVector2(),
                Origin = _trapSprite.SpriteCenter,
                LayerDepth = ZIndexCalculator.GroundObjectsLayerDepth
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_trapSprite, _drawingInfos);
        }
    }
}
