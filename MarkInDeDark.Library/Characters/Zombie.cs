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
            GameConfig gameConfig,
            MarkInDeDarkGame game)
            : base(
                  gameGrid,
                  new CharacterConfiguration(
                      assetsLoader: assetsLoader,
                      characterName: "zombie",
                      startingPosition: patrolPath.First().CenterPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                        firstTorchColor: gameConfig.ZombieLightColorRGB1,
                        secondTorchColor: gameConfig.ZombieLightColorRGB2,
                        torchScale: gameConfig.ZombieLightSize,
                        assetsLoader: assetsLoader,
                        torchPositionTraslationUp: new Vector2(0, -10),
                        torchPositionTraslationLeft: new Vector2(0, -20),
                        torchPositionTraslationRight: new Vector2(0, -10),
                        torchPositionTraslationDown: new Vector2(0, -10)),
                      movementSpeed: gameConfig.ZombieSpeed),
                  new EnemyConfiguration(
                      assetsLoader: assetsLoader,
                      enemyName: "zombie",
                      attackSoundIndex: soundManager.RegisterZombieAttackSound(),
                      screamSoundIndex: soundManager.RegisterZombieScreamSound(),
                      nCellsFindingRange: null,
                      startingAI: new PatrolAI(patrolPath, gameGrid, candlesManager, player, soundManager, searchesCandles: false)),
                  soundManager,
                  player,
                  assetsLoader,
                  game)
        {
            _candlesManager = candlesManager;
            _thisAI = CurrentAI as PatrolAI;
            
            // Ha gli occhi, non una cosa in mano, quindi non serve il glow
            _torch = null;
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
