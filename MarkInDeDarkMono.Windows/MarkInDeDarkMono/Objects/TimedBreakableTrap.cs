using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.Drawing;
using FlowsoftGamesMonogame.Extensions;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Characters;
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
                  triggerCollisionRectangle,
                  affectedCharacters,
                  (character, elapsed) => { },
                  false)
        {
            _trapTimeToBreak = TimeSpan.FromSeconds(gameConfig.BreakableTrapBreakingTimeSeconds);

            SetOnTriggerFunction(
                (character, elapsed) => 
                {
                    switch (_currentTrapState)
                    {
                        case TrapStates.Idle:
                            _currentTrapState = TrapStates.GoingToBreak;
                            break;

                        case TrapStates.GoingToBreak:
                            _elapsedSinceTrigger += elapsed;
                            // TODO il problema è far ripartire il timer quando non c'è più su nessuno

                            break;

                        case TrapStates.Broken:
                            break;
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
                Position = new Vector2(
                   triggerCollisionRectangle.X,
                   triggerCollisionRectangle.Y),
                Origin = triggerCollisionRectangle.Center.ToVector2()
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
