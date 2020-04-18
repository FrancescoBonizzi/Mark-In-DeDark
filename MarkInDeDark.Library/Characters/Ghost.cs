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
                      attackSoundIndex: 0, //soundManager.RegisterGhostAttackSound(),
                      screamSoundIndex: 0, //soundManager.RegisterGhostScreamSound(),
                      nCellsFindingRange: null,
                      startingAI: new PatrolGhostAI(patrolPath, gameGrid, player, soundManager),
                      killsCandlesIfCollidesWithThem: true),
                  soundManager,
                  player,
                  assetsLoader,
                  game,
                  candlesManager)
        {
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

        protected override void KillCandleLogic(Candle possibleCandle, TimeSpan elapsed)
        {
            possibleCandle.DieFast();
        }

        public override bool CustomConditionToAttackPlayer()
        {
            // Attacca il player solo se si muove
            return _player.IsMoving;
        }

    }
}
