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
    public class Skeletron : Enemy
    {
        public Skeletron(
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
                assetsLoader,
                "skeleton",
                patrolPath.First().TopLeftPosition,
                gameConfig.SkeletronLightColorRGB1,
                gameConfig.SkeletronLightColorRGB2,
                gameConfig.SkeletronLightSize,
                gameConfig.SkeletronSpeed,
                torchPositionTraslationUp: new Vector2(-20, -15),
                torchPositionTraslationLeft: new Vector2(-20, -5),
                torchPositionTraslationRight: new Vector2(20, -5),
                torchPositionTraslationDown: new Vector2(20, 0)),
            new EnemyConfiguration(
                assetsLoader,
                "skeleton",
                soundManager.RegisterSkeletronAttackSound(),
                soundManager.RegisterSkeletronScreamSound(),
                gameConfig.SkeletronMaxCellsForNearCandle,
                new PatrolHunterLineOfSightAI(patrolPath, gameGrid, candlesManager, player, soundManager, gameConfig)),
            soundManager,
            player,
            assetsLoader)
        {

        }
    }
}
