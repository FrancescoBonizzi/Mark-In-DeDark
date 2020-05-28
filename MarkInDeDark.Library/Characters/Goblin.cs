using MarkInDeDark.Library.AI;

using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Characters
{
    public class Goblin : Enemy
    {
        private readonly CharacterTorchParticleSystem _goblinLightParticleSystem;

        public Goblin(
            AssetsLoader assetsLoader,
            Vector2 startingPosition,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            SoundManager soundManager,
            Player player,
            GameConfig gameConfig,
            MarkInDeDarkGame game)
            : base(
                  grid: gameGrid,
                  characterBasicConfiguration: new CharacterConfiguration(
                      assetsLoader: assetsLoader,
                      characterName: "goblin",
                      startingPosition: startingPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                          firstTorchColor: gameConfig.GoblinLightColorRGB1,
                          secondTorchColor: gameConfig.GoblinLightColorRGB2,
                          torchPositionTraslationUp: new Vector2(-35, -10),
                          torchPositionTraslationLeft: new Vector2(5, 5),
                          torchPositionTraslationRight: new Vector2(-5, 5),
                          torchPositionTraslationDown: new Vector2(30, -15),
                          z: 46f,
                          opacity: 0.5f,
                          distanceSquared: 4_950f),
                      movementSpeed: gameConfig.GoblinSpeed),
                  enemyConfiguration: new EnemyConfiguration(
                      assetsLoader: assetsLoader,
                      enemyName: "goblin",
                      attackSoundIndex: 0,//soundManager.RegisterGoblinAttackSound(),
                      screamSoundIndex: 0,//soundManager.RegisterGoblinScreamSound(),
                      nCellsFindingRange: gameConfig.GoblinMaxCellsForNearCandle,
                      startingAI: new PlayerHunterAI(player, gameGrid, candlesManager),
                      killsCandlesIfCollidesWithThem: true),
                  soundManager: soundManager,
                  player: player,
                  assetsLoader: assetsLoader,
                  game: game,
                  candlesManager: candlesManager)
        {
            // E' più veloce quando hai quasi finito le candele
            candlesManager.PlayerUsedACandle +=
                (sender, args) => Speed += gameConfig.GoblinSpeedIncrementForEachCandleTaken;

            _goblinLightParticleSystem = new CharacterTorchParticleSystem(
                assetsLoader, 
                gameConfig.GoblinLightColorRGB1,
                gameConfig.GoblinLightColorRGB2);
        }

        protected override void RecalculateZIndex()
        {
            base.RecalculateZIndex();
            _goblinLightParticleSystem.LayerDepth = MainFigureDrawingInfos.LayerDepth + 0.00004f;
        }

        public override void Update(TimeSpan elapsed)
        {
            _goblinLightParticleSystem.Update(elapsed);

            if (!IsDead)
            {
                _goblinLightParticleSystem.AddParticles(TorchPosition);
            }

            base.Update(elapsed);
        }

        public override void UpdateAfterDeath(TimeSpan elapsed)
        {
            _goblinLightParticleSystem.Update(elapsed);
            base.UpdateAfterDeath(elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _goblinLightParticleSystem.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }
    }
}
