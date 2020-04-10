using MarkInDeDarkMono;
using MarkInDeDarkMono.AI;

using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Characters
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
            IEnumerable<Enemy> enemies)
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
                      gameConfig.TaurusSpeed),
                  new EnemyConfiguration(
                      assetsLoader,
                      "taurus",
                      soundManager.RegisterTaurusAttackSound(),
                      soundManager.RegisterTaurusScreamSound(),
                      gameConfig.TaurusMaxCellsForNearCandle,
                      new PatrolChargeAI(patrolPath, gameGrid, candlesManager, player, soundManager)),
                  soundManager,
                  player,
                  assetsLoader)
        {
            TaurusChargeSpeed = gameConfig.TaurusChargeSpeed;
            TaurusTimeToStartSeconds = gameConfig.TaurusTimeToStartSeconds;
            TaurusTimeStunnedSeconds = gameConfig.TaurusTimeStunnedSeconds;
            TaurusChargeColorRGB = gameConfig.TaurusChargeColorRGB;
            _enemies = enemies.Where(e => !(e is Taurus));
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
