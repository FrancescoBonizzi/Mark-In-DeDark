using MarkInDeDark.Library;
using MarkInDeDark.Library.AI;

using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.Characters
{
    public class Ghost : Enemy
    {
        private readonly CandlesManager _candlesManager;

        public Ghost(
            AssetsLoader assetsLoader,
            List<GameCell> patrolPath,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            SoundManager soundManager,
            Player player,
            GameConfig gameConfig,
            MarkInDeDarkGame game)
            : base(
                  gameGrid,
                  new CharacterConfiguration(
                      assetsLoader: assetsLoader,
                      characterName: "ghost",
                      startingPosition: patrolPath.First().CenterPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                        firstTorchColor: gameConfig.GhostLightColorRGB1,
                        secondTorchColor: gameConfig.GhostLightColorRGB2,
                        torchScale: gameConfig.GhostLightSize,
                        torchPositionTraslationUp: Vector2.Zero,
                        torchPositionTraslationLeft: Vector2.Zero,
                        torchPositionTraslationRight: Vector2.Zero,
                        torchPositionTraslationDown: Vector2.Zero,
                        assetsLoader: assetsLoader),
                      movementSpeed: gameConfig.GhostSpeed,
                      withDieAnimation: false),
                  new EnemyConfiguration(
                      assetsLoader: assetsLoader,
                      enemyName: "ghost",
                      attackSoundIndex: soundManager.RegisterGhostAttackSound(),
                      screamSoundIndex: soundManager.RegisterGhostScreamSound(),
                      nCellsFindingRange: null,
                      startingAI: new PatrolGhostAI(patrolPath, gameGrid, player, soundManager)),
                  soundManager,
                  player,
                  assetsLoader,
                  game)
        {
            _candlesManager = candlesManager;
            _canGoOverWalls = true;

            // E' lui stesso il glow
            _torch = null;
            var deltaPosition = new Vector2(0, -15f);
            MainFigureDrawingInfos.Origin -= deltaPosition;
        }

        public override void Die()
        {
            // Il Ghost non può morire
            return;
        }

        public override void Update(TimeSpan elapsed)
        {
            // Se incontra una candela, la spegne all'istante
            var currentCell = _gameGrid.GetCellAtPixel(Position);
            var possibleCandle = _candlesManager.IsThereACandle(currentCell);
            if (possibleCandle != null)
            {
                possibleCandle.DieFast();
            }

            base.Update(elapsed);
        }

        public override bool CustomConditionToAttackPlayer()
        {
            // Attacca il player solo se si muove
            return _player.IsMoving;
        }

    }
}
