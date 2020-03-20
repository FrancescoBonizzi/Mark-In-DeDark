using Mark.Engine;
using MarkInDeDarkMono.AI;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;

namespace MarkInDeDarkMono.Characters
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
            GameConfig gameConfig)
            : base(
                  gameGrid,
                  new CharacterConfiguration(
                      assetsLoader,
                      "werewolf",
                      startingPosition,
                      gameConfig.WerewolfLightColorRGB1,
                      gameConfig.WerewolfLightColorRGB2,
                      gameConfig.WerewolfLightSize,
                      gameConfig.WerewolfSpeed,
                      torchPositionTraslationUp: new Vector2(25, 10),
                      torchPositionTraslationLeft: new Vector2(0, 25),
                      torchPositionTraslationRight: new Vector2(0, 25),
                      torchPositionTraslationDown: new Vector2(-25, 10)),
                  new EnemyConfiguration(
                      assetsLoader,
                      "werewolf",
                      soundManager.RegisterWerewolfAttackSound(),
                      soundManager.RegisterWerewolfScreamSound(),
                      gameConfig.WerewolfMaxCellsForNearCandle,
                      new PlayerHunterAI(player, gameGrid, candlesManager)),
                  soundManager,
                  player,
                  assetsLoader)
        {

        }
    }
}
