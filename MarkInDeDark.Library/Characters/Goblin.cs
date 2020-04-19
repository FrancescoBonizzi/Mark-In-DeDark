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
                  gameGrid,
                  new CharacterConfiguration(
                      assetsLoader,
                      "goblin",
                      startingPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                          gameConfig.GoblinLightColorRGB1,
                          gameConfig.GoblinLightColorRGB2,
                          gameConfig.GoblinLightSize,
                          torchPositionTraslationUp: new Vector2(30, -15),
                          torchPositionTraslationLeft: new Vector2(5, 5),
                          torchPositionTraslationRight: new Vector2(-5, 5),
                          torchPositionTraslationDown: new Vector2(-33, 5),
                          assetsLoader: assetsLoader),
                      gameConfig.GoblinSpeed),
                  new EnemyConfiguration(
                      assetsLoader,
                      "goblin",
                      0,//soundManager.RegisterGoblinAttackSound(),
                      0,//soundManager.RegisterGoblinScreamSound(),
                      gameConfig.GoblinMaxCellsForNearCandle,
                      new GoblinAI(gameGrid, player),
                      true),
                  soundManager,
                  player,
                  assetsLoader,
                  game,
                  candlesManager)
        {
            // E' più veloce quando hai quasi finito le candele
            candlesManager.PlayerUsedACandle +=
                (sender, args) => Speed += gameConfig.SpeedIncrementForEachCandleTaken;

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
            base.Draw(spriteBatch);
            _goblinLightParticleSystem.Draw(spriteBatch);
        }
    }
}
