using Mark.Engine;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Levels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.AI
{
    public class PatrolChargeAI : PatrolAI
    {
        public PatrolChargeAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager)
            : base(patrolWaypoints, gameGrid, candlesManager, player, soundManager)
        {

        }

        public override CharacterActions Update(
            Enemy character,
            TimeSpan elapsed)
        {
            // 1. Il patrol identifica il path per caricare il player
            CharacterActions? straightDirectionToPosition = PatrolHunterLineOfSightAI.PathToPositionIfFacingAndSameDirection(
                character, _player.Position, _gameGrid)?.FirstOrDefault();

            // ...se il player non è caricabile
            if (straightDirectionToPosition == null)
            {
                // 2. L'hunter cerca candele da caricare
                foreach (var candle in _candlesManager.Candles)
                {
                    straightDirectionToPosition = PatrolHunterLineOfSightAI.PathToPositionIfFacingAndSameDirection(
                        character, candle.Position, _gameGrid)?.FirstOrDefault();

                    if (straightDirectionToPosition != null)
                    {
                        break;
                    }
                }
            }

            if (straightDirectionToPosition != null)
            {
                return character.SetNewAI(
                    new ChargeDirectionAI(straightDirectionToPosition.Value, _gameGrid, _player, character as Taurus, _soundManager, _candlesManager, this));
            }

            return base.Update(character, elapsed);
        }

    }
}
