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
                      attackSoundIndex: 0,//soundManager.RegisterZombieAttackSound(),
                      screamSoundIndex: 0,//soundManager.RegisterZombieScreamSound(),
                      nCellsFindingRange: null,
                      startingAI: new PatrolAI(patrolPath, gameGrid, candlesManager, player, searchesCandles: false),
                      true),
                  soundManager,
                  player,
                  assetsLoader,
                  game,
                  candlesManager)
        {
            // Ha gli occhi, non una cosa in mano, quindi non serve il glow
            _torch = null;
        }
    }
}
