using MarkInDeDark.Library;
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
        private readonly GoblinLightParticleSystem _goblinLightParticleSystem;

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
                      soundManager.RegisterGoblinAttackSound(),
                      soundManager.RegisterGoblinScreamSound(),
                      gameConfig.GoblinMaxCellsForNearCandle,
                      new PlayerHunterAI(player, gameGrid, candlesManager)),
                  soundManager,
                  player,
                  assetsLoader,
                  game)
        {
            // E' più veloce quando hai quasi finito le candele
            candlesManager.PlayerUsedACandle +=
                (sender, args) => Speed += gameConfig.SpeedIncrementForEachCandleTaken;

            _goblinLightParticleSystem = new GoblinLightParticleSystem(assetsLoader);
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

        public override void DrawEffects(SpriteBatch spriteBatch)
        {
            _goblinLightParticleSystem.Draw(spriteBatch);
            base.DrawEffects(spriteBatch);
        }
    }
}
