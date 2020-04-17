using MarkInDeDark.Library;
using MarkInDeDark.Library.AI;

using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.Characters
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
            GameConfig gameConfig,
            MarkInDeDarkGame game)
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
                        attackSoundIndex: 0,//soundManager.RegisterBubuAttackSound(),
                        screamSoundIndex: 0,//soundManager.RegisterBubuScreamSound(),
                        nCellsFindingRange: gameConfig.BubuMaxCellsForNearCandle,
                        startingAI: new PatrolAI(patrolPath, gameGrid, candlesManager, player, soundManager, searchesCandles: true)),
                    soundManager,
                    player,
                    assetsLoader,
                    game)
        {
        }
    }
}
