using Mark.Engine;
using MarkInDeDarkMono.AI;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Characters
{
    public class Zombie : Enemy
    {
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
                      nCellsFindingRange: gameConfig.ZombieMaxCellsForNearCandle,
                      startingAI: new PatrolAI(patrolPath, gameGrid, candlesManager, player, soundManager)),
                  soundManager,
                  player,
                  assetsLoader)
        {

        }

    }
}
