using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.AI
{
    public class TaurusPatrolAI : PatrolAI
    {
        private readonly MarkInDeDarkGame _game;
        private readonly SoundManager _soundManager;
        private readonly Player _player;

        public TaurusPatrolAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            MarkInDeDarkGame game)
            : base(patrolWaypoints, gameGrid, candlesManager, true)
        {
            _game = game;
            _soundManager = soundManager;
            _player = player;
        }

        public override Vector2 GetNextDirection(Enemy enemy, TimeSpan elapsed)
        {
            // Se c'è un path diretto per caricare il player...
            CharacterActions? straightDirectionToPosition = null;

            // Il taurus dev'essere al centro di una cella
            var taurusCell = _gameGrid.GetCellAtPixel(enemy.Position);

            if (Vector2.Distance(taurusCell.CenterPosition, enemy.WorldCenter) <= 5f)
            {
                // Il player dev'essere nel centro della cella
                straightDirectionToPosition = SkeletonPatrolAI.PathToPositionIfFacingAndSameDirection(
                    enemy,
                    _player.Position,
                    _gameGrid)?.FirstOrDefault();
            }

            // ...se il player non è caricabile...
            if (straightDirectionToPosition == null)
            {
                // ...cerco candele da caricare
                foreach (var candle in _candlesManager.Candles)
                {
                    straightDirectionToPosition = SkeletonPatrolAI.PathToPositionIfFacingAndSameDirection(
                        enemy,
                        candle.Position,
                        _gameGrid)?.FirstOrDefault();

                    if (straightDirectionToPosition != null)
                    {
                        break;
                    }
                }
            }

            if (straightDirectionToPosition != null)
            {
                enemy.SetNewAI(
                    new TaurusChargingAI(
                        straightDirectionToPosition.Value,
                        _gameGrid,
                        enemy as Taurus,
                        _soundManager,
                        _candlesManager,
                        this,
                        _game));
                return Vector2.Zero;
            }

            return base.GetNextDirection(enemy, elapsed);
        }
    }
}
