using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.AI
{
    public class PatrolHunterLineOfSightAI : PatrolAI
    {
        private readonly GameConfig _gameConfig;

        public PatrolHunterLineOfSightAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            GameConfig gameConfig)
            : base(patrolWaypoints, gameGrid, candlesManager, player, soundManager, true)
        {
            _gameConfig = gameConfig;
        }

        public static IEnumerable<AIMovementAction> PathToPositionIfFacingAndSameDirection(
            Enemy enemy,
            Vector2 playerPosition,
            GameGrid gameGrid)
        {
            var playerCellIndex = gameGrid.GetCellIndexByPixel(playerPosition);
            var enemyCellIndex = gameGrid.GetCellIndexByPixel(enemy.Position);

            // Se il player ha i piedi in due celle bisogna valutarle entrambe
            bool areInTheSameVertical = playerCellIndex.X == enemyCellIndex.X;
            bool areInTheSameHorizontal = playerCellIndex.Y == enemyCellIndex.Y;

            // Prima euristica: devono essere sulla stessa retta
            if (!areInTheSameVertical && !areInTheSameHorizontal)
            {
                return null;
            }

            // Seconda euristica: il nemico non dev'essere di schiena rispetto al player
            if (areInTheSameVertical)
            {
                if (
                        (enemy.CurrentAction == CharacterActions.Up
                        && enemyCellIndex.Y < playerCellIndex.Y)

                        || (enemy.CurrentAction == CharacterActions.Down
                        && enemyCellIndex.Y > playerCellIndex.Y)
                   )
                {
                    return null;
                }
            }
            else if (areInTheSameHorizontal)
            {
                if (
                        (enemy.CurrentAction == CharacterActions.Left
                        && enemyCellIndex.X < playerCellIndex.X)

                        || (enemy.CurrentAction == CharacterActions.Right
                        && enemyCellIndex.X > playerCellIndex.X)
                    )
                {
                    return null;
                }
            }

            // Calcolo le azioni per raggiungersi,
            // così posso vedere se ci sono muri di mezzo (ovvero se deve fare più azioni per raggiungerlo)
            var actionsToPlayer = gameGrid.FindActionsToPath(
                enemy.Position,
                playerPosition);

            if (actionsToPlayer == null || !actionsToPlayer.Any()) // Può succedere se il player si sposta al volo
            {
                return null;
            }

            var firstAction = actionsToPlayer.First();

            foreach (var action in actionsToPlayer.Skip(1))
            {
                // Se il path per il player non è sempre la stessa direzione, non lo seguo
                if (action != firstAction)
                {
                    return null;
                }
            }

            return actionsToPlayer;
        }

        public override CharacterActions Update(
            Enemy character,
            TimeSpan elapsed)
        {
            var pathToPlayerIfFacingAndSameDirection = PathToPositionIfFacingAndSameDirection(
                character,
                _player.Position,
                _gameGrid);

            if (pathToPlayerIfFacingAndSameDirection == null)
            {
                return base.Update(character, elapsed);
            }

            return character.SetNewAI(new PlayerHunterLineOfSightAI(
                character, this, _gameGrid, _candlesManager, _player, _soundManager, _gameConfig, pathToPlayerIfFacingAndSameDirection));
        }

    }
}
