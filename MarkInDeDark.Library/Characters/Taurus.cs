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
    public class Taurus : Enemy
    {
        public float TaurusChargeSpeed { get; }
        public double TaurusTimeToStartSeconds { get; }
        public double TaurusTimeStunnedSeconds { get; }
        public Color TaurusChargeColorRGB { get; }

        private readonly IEnumerable<Enemy> _enemies;
        
        public Taurus(
            AssetsLoader assetsLoader,
            List<GameCell> patrolPath,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            SoundManager soundManager,
            Player player,
            GameConfig gameConfig,
            IEnumerable<Enemy> enemies,
            MarkInDeDarkGame game)
            : base(
                  gameGrid,
                  new CharacterConfiguration(
                      assetsLoader: assetsLoader,
                      characterName: "taurus",
                      startingPosition: patrolPath.First().CenterPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                        firstTorchColor: gameConfig.TaurusLightColorRGB1,
                        secondTorchColor: gameConfig.TaurusLightColorRGB2,
                        torchScale: gameConfig.TaurusLightSize,
                        torchPositionTraslationUp: new Vector2(25, 10),
                        torchPositionTraslationLeft: new Vector2(0, 25),
                        torchPositionTraslationRight: new Vector2(0, 25),
                        torchPositionTraslationDown: new Vector2(-25, 10),
                        assetsLoader: assetsLoader),
                      gameConfig.TaurusSpeed,
                      withDieAnimation: false),
                  new EnemyConfiguration(
                      assetsLoader,
                      "taurus",
                      0,//soundManager.RegisterTaurusAttackSound(),
                      0,//soundManager.RegisterTaurusScreamSound(),
                      gameConfig.TaurusMaxCellsForNearCandle,
                      new PatrolChargeAI(patrolPath, gameGrid, candlesManager, player, soundManager, game)),
                  soundManager,
                  player,
                  assetsLoader,
                  game)
        {
            TaurusChargeSpeed = gameConfig.TaurusChargeSpeed;
            TaurusTimeToStartSeconds = gameConfig.TaurusTimeToStartSeconds;
            TaurusTimeStunnedSeconds = gameConfig.TaurusTimeStunnedSeconds;
            TaurusChargeColorRGB = gameConfig.TaurusChargeColorRGB;
            _enemies = enemies.Where(e => !(e is Taurus));
            _torch = null;

            MainAnimationsAnimationManager.AddAnimation("TaurusRunSide", assetsLoader.GetAnimation("TaurusRunSide"));
            MainAnimationsAnimationManager.AddAnimation("TaurusRunFront", assetsLoader.GetAnimation("TaurusRunFront"));
            MainAnimationsAnimationManager.AddAnimation("TaurusRunBack", assetsLoader.GetAnimation("TaurusRunBack"));
            MainAnimationsAnimationManager.AddAnimation("TaurusChargeSide", assetsLoader.GetAnimation("TaurusChargeSide"));
            MainAnimationsAnimationManager.AddAnimation("TaurusChargeFront", assetsLoader.GetAnimation("TaurusChargeFront"));
            MainAnimationsAnimationManager.AddAnimation("TaurusChargeBack", assetsLoader.GetAnimation("TaurusChargeBack"));
        }

        public override void Update(TimeSpan elapsed)
        {
            base.Update(elapsed);

            // Il Taurus può uccidere gli altri nemici
            foreach(var e in _enemies)
            {
                if (!e.IsDead
                    && CurrentAI is ChargeDirectionAI
                    && CollisionRectangle.Intersects(e.CollisionRectangle))
                {
                    e.Die();
                }
            }
        }

    }
}
