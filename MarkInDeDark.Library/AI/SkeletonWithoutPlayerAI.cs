using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.AI
{
    public class SkeletonWithoutPlayerAI : PatrolAI
    {
        private readonly GameConfig _gameConfig;
        private readonly Player _player;
        private readonly SoundManager _soundManager;

        public SkeletonWithoutPlayerAI(
            List<GameCell> patrolWaypoints,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            Player player,
            SoundManager soundManager,
            GameConfig gameConfig)
            : base(patrolWaypoints, gameGrid, candlesManager, true)
        {
            _gameConfig = gameConfig;
            _player = player;
            _soundManager = soundManager;
        }

        public static List<CharacterActions> PathToPositionIfFacingAndSameDirection(
            Enemy enemy,
            Vector2 playerPosition,
            GameGrid gameGrid)
        {
            var playerCellIndex = gameGrid.GetCellIndexByPixel(playerPosition);
            var enemyCellIndex = gameGrid.GetCellIndexByPixel(enemy.Position);

            // Se il player ha i piedi in due celle bisogna valutarle entrambe
            bool areInTheSameVertical = playerCellIndex.X == enemyCellIndex.X;
            bool areInTheSameHorizontal = playerCellIndex.Y == enemyCellIndex.Y;

            // Prima euristica: devono essere sulla stessa colonna o riga di celle
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

        public override Vector2 GetNextDirection(Enemy character, TimeSpan elapsed)
        {
            var pathToPlayerIfFacingAndSameDirection = PathToPositionIfFacingAndSameDirection(
              character,
              _player.Position,
              _gameGrid);

            if (pathToPlayerIfFacingAndSameDirection == null)
            {
                return base.GetNextDirection(character, elapsed);
            }

            character.SetNewAI(new SkeletonWithPlayerAI(
                character as Skeletron, 
                this, 
                _gameGrid, 
                _candlesManager, 
                _player, 
                _soundManager, 
                _gameConfig));

            return Vector2.Zero;
        }
    }
}
