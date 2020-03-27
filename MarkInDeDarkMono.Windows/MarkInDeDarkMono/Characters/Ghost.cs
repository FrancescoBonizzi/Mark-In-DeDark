using Mark.Engine;
using MarkInDeDarkMono.AI;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkInDeDarkMono.Characters
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
            GameConfig gameConfig)
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
                      startingAI: new PatrolAI(patrolPath, gameGrid, candlesManager, player, soundManager, false)),
                  soundManager,
                  player,
                  assetsLoader)
        {
            _candlesManager = candlesManager;
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
