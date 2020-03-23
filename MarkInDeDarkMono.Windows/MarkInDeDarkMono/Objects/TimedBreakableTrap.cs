using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MarkInDeDarkMono.Objects
{
    public class TimedBreakableTrap : TriggerObject
    {
        private enum TrapStates
        {
            Idle,
            GoingToBreak,
            Broken
        }

        private readonly InstantlyDieTrap _instantlyDieTrap;
        private TimeSpan _elapsedSinceTrigger;
        private readonly Sprite _breakableTrapSprite;
        private readonly DrawingInfos _drawingInfos;
        private TrapStates _currentTrapState;

        private readonly TimeSpan _trapTimeToBreak;

        public TimedBreakableTrap(
            AssetsLoader assetsLoader,
            GameConfig gameConfig,
            Rectangle triggerCollisionRectangle,
            IEnumerable<Character> affectedCharacters)
            : base(
                  triggerCollisionRectangle: triggerCollisionRectangle,
                  affectedCharacters: affectedCharacters,
                  onTriggerPressed: (character, elapsed) => { },
                  onTriggerLeft: null,
                  shouldFireOnce: false)
        {
            _trapTimeToBreak = TimeSpan.FromSeconds(gameConfig.BreakableTrapBreakingTimeSeconds);

            SetOnTriggerPressed(
                (character, elapsed) => 
                {
                    switch (_currentTrapState)
                    {
                        case TrapStates.Idle:
                            _currentTrapState = TrapStates.GoingToBreak;
                            break;

                        case TrapStates.GoingToBreak:
                            _elapsedSinceTrigger += elapsed;

                            break;

                        case TrapStates.Broken:
                            break;
                    }
                });

            SetOnTriggerLeft(
                () =>
                {
                    if (_currentTrapState == TrapStates.GoingToBreak)
                    {
                        _elapsedSinceTrigger = TimeSpan.Zero;
                        _currentTrapState = TrapStates.Idle;
                        _hasFired = false;
                    }
                });

            _currentTrapState = TrapStates.Idle;

            // La tengo per quando si romperà la "copertura"
            // di questa trappola
            _instantlyDieTrap = new InstantlyDieTrap(
                assetsLoader,
                affectedCharacters,
                triggerCollisionRectangle);

            _elapsedSinceTrigger = TimeSpan.Zero;
            _breakableTrapSprite = assetsLoader.Sprites["trap_breakable"];
            _drawingInfos = new DrawingInfos()
            {
                Position = triggerCollisionRectangle.Center.ToVector2(),
                Origin = _breakableTrapSprite.SpriteCenter
            };
        }

        public override void Update(TimeSpan elapsed)
        {
            switch (_currentTrapState)
            {
                case TrapStates.Idle:
                    base.Update(elapsed);
                    break;

                case TrapStates.GoingToBreak:

                    base.Update(elapsed);
                    // Il tempo è incrementato nel trigger
                    if (_elapsedSinceTrigger >= _trapTimeToBreak)
                    {
                        _currentTrapState = TrapStates.Broken;
                    }
                  
                    break;

                case TrapStates.Broken:
                    _instantlyDieTrap.Update(elapsed);
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_currentTrapState)
            {
                case TrapStates.Idle:
                case TrapStates.GoingToBreak:
                    spriteBatch.Draw(_breakableTrapSprite, _drawingInfos);
                    break;

                case TrapStates.Broken:
                    _instantlyDieTrap.Draw(spriteBatch);
                    break;
            }
        }

    }
}
