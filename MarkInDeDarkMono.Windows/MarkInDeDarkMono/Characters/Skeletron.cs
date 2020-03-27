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
                assetsLoader: assetsLoader,
                characterName: "skeleton",
                startingPosition: patrolPath.First().TopLeftPosition,
                characterTorchConfiguration: new CharacterTorchConfiguration(
                    firstTorchColor: gameConfig.SkeletronLightColorRGB1,
                    secondTorchColor: gameConfig.SkeletronLightColorRGB2,
                    torchScale: gameConfig.SkeletronLightSize,
                    torchPositionTraslationUp: new Vector2(-20, -15),
                    torchPositionTraslationLeft: new Vector2(-20, -5),
                    torchPositionTraslationRight: new Vector2(20, -5),
                    torchPositionTraslationDown: new Vector2(20, 0),
                    assetsLoader: assetsLoader),
                movementSpeed: gameConfig.SkeletronSpeed),
            new EnemyConfiguration(
                assetsLoader: assetsLoader,
                enemyName: "skeleton",
                attackSoundIndex: soundManager.RegisterSkeletronAttackSound(),
                screamSoundIndex: soundManager.RegisterSkeletronScreamSound(),
                nCellsFindingRange: gameConfig.SkeletronMaxCellsForNearCandle,
                startingAI: new PatrolHunterLineOfSightAI(patrolPath, gameGrid, candlesManager, player, soundManager, gameConfig)),
            soundManager,
            player,
            assetsLoader)
        {

        }
    }
}
