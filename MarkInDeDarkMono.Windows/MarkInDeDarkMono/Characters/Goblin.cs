using MarkInDeDarkMono;
using MarkInDeDarkMono.AI;

using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;

namespace MarkInDeDarkMono.Characters
{
    public class Goblin : Enemy
    {
        public Goblin(
            AssetsLoader assetsLoader,
            Vector2 startingPosition,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            SoundManager soundManager,
            Player player,
            GameConfig gameConfig)
            : base(
                  gameGrid,
                  new CharacterConfiguration(
                      assetsLoader,
                      "goblin",
                      startingPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                          gameConfig.GoblinLightColorRGB1,
                          gameConfig.GoblinLightColorRGB2,
                          gameConfig.GoblinLightSize,
                          torchPositionTraslationUp: new Vector2(20, -15),
                          torchPositionTraslationLeft: new Vector2(5, 5),
                          torchPositionTraslationRight: new Vector2(-5, 5),
                          torchPositionTraslationDown: new Vector2(-20, 5),
                          assetsLoader: assetsLoader),
                      gameConfig.GoblinSpeed),
                  new EnemyConfiguration(
                      assetsLoader,
                      "goblin",
                      soundManager.RegisterGoblinAttackSound(),
                      soundManager.RegisterGoblinScreamSound(),
                      gameConfig.GoblinMaxCellsForNearCandle,
                      new PlayerHunterAI(player, gameGrid, candlesManager)),
                  soundManager,
                  player,
                  assetsLoader)
        {
            // E' più veloce quando hai quasi finito le candele
            candlesManager.PlayerUsedACandle +=
                (sender, args) => Speed += gameConfig.SpeedIncrementForEachCandleTaken;

            _zIndexDelta = 0.0051f;
        }
    }
}
