using MarkInDeDark.Library;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.AI
{
    public class PatrolChargeAI : PatrolAI
    {
        private readonly MarkInDeDarkGame _game;

        public PatrolChargeAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            MarkInDeDarkGame game)
            : base(patrolWaypoints, gameGrid, candlesManager, player, true)
        {
            _game = game;
        }

        //public override CharacterActions Update(
        //    Enemy enemy,
        //    TimeSpan elapsed)
        //{
        //    // 1. Il patrol identifica il path per caricare il player
        //    AIMovementAction straightDirectionToPosition = PatrolHunterLineOfSightAI.PathToPositionIfFacingAndSameDirection(
        //        enemy, _player.Position, _gameGrid)?.FirstOrDefault();

        //    // ...se il player non è caricabile
        //    if (straightDirectionToPosition == null)
        //    {
        //        // 2. L'hunter cerca candele da caricare
        //        foreach (var candle in _candlesManager.Candles)
        //        {
        //            straightDirectionToPosition = PatrolHunterLineOfSightAI.PathToPositionIfFacingAndSameDirection(
        //                enemy, candle.Position, _gameGrid)?.FirstOrDefault();

        //            if (straightDirectionToPosition != null)
        //            {
        //                break;
        //            }
        //        }
        //    }

        //    if (straightDirectionToPosition != null)
        //    {
        //        return enemy.SetNewAI(
        //            new ChargeDirectionAI(
        //                straightDirectionToPosition.Action,
        //                _gameGrid,
        //                enemy as Taurus,
        //                _soundManager,
        //                _candlesManager,
        //                this,
        //                _game));
        //    }

        //    return base.Update(enemy, elapsed);
        //}

    }
}
