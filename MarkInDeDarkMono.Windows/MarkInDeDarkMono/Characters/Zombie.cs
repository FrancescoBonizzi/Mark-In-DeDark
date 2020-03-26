using Mark.Engine;
using MarkInDeDarkMono.AI;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Characters
{
    public class Zombie : Enemy
    {
        private readonly CandlesManager _candlesManager;
        private readonly PatrolAI _thisAI;

        public Zombie(
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
                      characterName: "zombie",
                      startingPosition: patrolPath.First().CenterPosition,
                      firstTorchColor: gameConfig.ZombieLightColorRGB1,
                      secondTorchColor: gameConfig.ZombieLightColorRGB2,
                      torchScale: gameConfig.ZombieLightSize,
                      movementSpeed: gameConfig.ZombieSpeed,
                      torchPositionTraslationUp: new Vector2(0, -10),
                      torchPositionTraslationLeft: new Vector2(0, -20),
                      torchPositionTraslationRight: new Vector2(0, -10),
                      torchPositionTraslationDown: new Vector2(0, -10)),
                  new EnemyConfiguration(
                      assetsLoader: assetsLoader,
                      enemyName: "zombie",
                      attackSoundIndex: soundManager.RegisterZombieAttackSound(),
                      screamSoundIndex: soundManager.RegisterZombieScreamSound(),
                      nCellsFindingRange: null,
                      startingAI: new PatrolAI(patrolPath, gameGrid, candlesManager, player, soundManager, searchesCandles: false)),
                  soundManager,
                  player,
                  assetsLoader)
        {
            _candlesManager = candlesManager;
            _thisAI = CurrentAI as PatrolAI;
        }

        public override void Update(TimeSpan elapsed)
        {
            // Se incontra una candela, la spegne all'istante
            var currentCell = _gameGrid.GetCellAtPixel(Position);
            var possibleCandle = _candlesManager.IsThereACandle(currentCell);
            if (possibleCandle != null)
            {
                SetNewAI(new KillCandleAI(possibleCandle, Array.Empty<CharacterActions>(), _thisAI));
                CurrentAI.Update(this, elapsed);
            }

            base.Update(elapsed);
        }

    }
}
