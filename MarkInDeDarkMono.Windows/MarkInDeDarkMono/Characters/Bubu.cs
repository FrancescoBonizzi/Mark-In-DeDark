using Mark.Engine;
using MarkInDeDarkMono.AI;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Characters
{
    public class Bubu : Enemy
    {
        public Bubu(
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
                        characterName: "bubu",
                        startingPosition: patrolPath.First().CenterPosition,
                        movementSpeed: gameConfig.BubuSpeed,
                        characterTorchConfiguration: null),
                    new EnemyConfiguration(
                        assetsLoader: assetsLoader,
                        enemyName: "bubu",
                        attackSoundIndex: soundManager.RegisterBubuAttackSound(),
                        screamSoundIndex: soundManager.RegisterBubuScreamSound(),
                        nCellsFindingRange: gameConfig.BubuMaxCellsForNearCandle,
                        startingAI: new PatrolAI(patrolPath, gameGrid, candlesManager, player, soundManager, searchesCandles: true)),
                    soundManager,
                    player,
                    assetsLoader)
        {

        }
    }
}
