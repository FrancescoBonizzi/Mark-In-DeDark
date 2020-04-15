using MarkInDeDark.Library;
using MarkInDeDark.Library.AI;

using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Characters
{
    public class Werewolf : Enemy
    {
        public Werewolf(
            AssetsLoader assetsLoader,
            Vector2 startingPosition,
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
                      characterName: "werewolf",
                      startingPosition: startingPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                        firstTorchColor: gameConfig.WerewolfLightColorRGB1,
                        secondTorchColor: gameConfig.WerewolfLightColorRGB2,
                        torchScale: gameConfig.WerewolfLightSize,
                        torchPositionTraslationUp: new Vector2(25, 10),
                        torchPositionTraslationLeft: new Vector2(0, 25),
                        torchPositionTraslationRight: new Vector2(0, 25),
                        torchPositionTraslationDown: new Vector2(-25, 10),
                        assetsLoader: assetsLoader),
                      movementSpeed: gameConfig.WerewolfSpeed),
                  new EnemyConfiguration(
                      assetsLoader,
                      "werewolf",
                      soundManager.RegisterWerewolfAttackSound(),
                      soundManager.RegisterWerewolfScreamSound(),
                      gameConfig.WerewolfMaxCellsForNearCandle,
                      new PlayerHunterAI(player, gameGrid, candlesManager)),
                  soundManager,
                  player,
                  assetsLoader,
                  game)
        {
        }
    }
}
