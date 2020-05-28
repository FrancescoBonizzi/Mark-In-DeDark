using MarkInDeDark.Library.AI;

using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.Characters
{
    public class Ghost : Enemy
    {
        private readonly GhostParticleSystem _primaryGhostParticleSystem;
        private readonly GhostParticleSystem _wallCrossGhostParticleSystem;
        private readonly GhostParticleSystem _lightMapGhostParticleSystem;

        public Ghost(
            AssetsLoader assetsLoader,
            List<GameCell> patrolPath,
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
                      characterName: "ghost",
                      startingPosition: patrolPath.First().CenterPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                        firstTorchColor: gameConfig.GhostLightColorRGB1,
                        secondTorchColor: gameConfig.GhostLightColorRGB2,
                        torchPositionTraslationUp: Vector2.Zero,
                        torchPositionTraslationLeft: Vector2.Zero,
                        torchPositionTraslationRight: Vector2.Zero,
                        torchPositionTraslationDown: Vector2.Zero,
                        z: 46f,
                        opacity: 0.5f,
                        distanceSquared: 4_950f),
                      movementSpeed: gameConfig.GhostSpeed,
                      withDieAnimation: false),
                  enemyConfiguration: new EnemyConfiguration(
                      assetsLoader: assetsLoader,
                      enemyName: "ghost",
                      attackSoundIndex: 0, //soundManager.RegisterGhostAttackSound(),
                      screamSoundIndex: 0, //soundManager.RegisterGhostScreamSound(),
                      nCellsFindingRange: null,
                      startingAI: new PatrolGhostAI(patrolPath, gameGrid, player, soundManager),
                      killsCandlesIfCollidesWithThem: true),
                  soundManager: soundManager,
                  player: player,
                  assetsLoader: assetsLoader,
                  game: game,
                  candlesManager: candlesManager)
        {
            _canGoOverWalls = true;

            var deltaPosition = new Vector2(0, -15f);
            MainFigureDrawingInfos.Origin -= deltaPosition;

            _primaryGhostParticleSystem = new GhostParticleSystem(
               assetsLoader,
               new Color(101, 221, 160),
               new Color(71, 191, 117))
            {
                Density = 3,
                MinNumParticles = 1,
                MaxNumParticles = 4,
                MinInitialSpeed = 160,
                MaxInitialSpeed = 200,
                MinAcceleration = 3.6f,
                MaxAcceleration = 7.5f,
                MinRotationSpeed = -3,
                MaxRotationSpeed = 0,
                MinLifetime = TimeSpan.FromMilliseconds(1028),
                MaxLifetime = TimeSpan.FromMilliseconds(1557),
                MinScale = 1,
                MaxScale = 1,
                MinSpawnAngle = -45,
                MaxSpawnAngle = 235
            };

            _wallCrossGhostParticleSystem = new GhostParticleSystem(
                assetsLoader,
                Color.LimeGreen,
                Color.Lime);

            _lightMapGhostParticleSystem = new GhostParticleSystem(
                assetsLoader,
                Color.White,
                Color.White)
            {
                Density = 3,
                MinNumParticles = 1,
                MaxNumParticles = 4,
                MinInitialSpeed = 160,
                MaxInitialSpeed = 200,
                MinAcceleration = 3.6f,
                MaxAcceleration = 7.5f,
                MinRotationSpeed = -3,
                MaxRotationSpeed = 0,
                MinLifetime = TimeSpan.FromMilliseconds(1028),
                MaxLifetime = TimeSpan.FromMilliseconds(1557),
                MinScale = 1.5f,
                MaxScale = 2f,
                MinSpawnAngle = -45,
                MaxSpawnAngle = 235
            };
        }

        public override void Die()
        {
            // Il Ghost non può morire
            return;
        }

        public override void CustomActionOnPlayerDead()
        {
            _player.ShouldDoExplosionAnimation = true;
        }

        protected override void KillCandleLogic(Candle possibleCandle, TimeSpan elapsed)
        {
            possibleCandle.Die(TimeSpan.FromMilliseconds(250));
        }

        public override bool CustomConditionToAttackPlayer()
        {
            // Attacca il player solo se si muove
            return _player.IsMoving;
        }

        public override void Update(TimeSpan elapsed)
        {
            _primaryGhostParticleSystem.AddParticles(WorldCenter);
            _lightMapGhostParticleSystem.AddParticles(WorldCenter);

            if (_gameGrid.CollideWithAWall(CollisionRectangle))
            {
                _wallCrossGhostParticleSystem.AddParticles(WorldCenter);
            }

            _lightMapGhostParticleSystem.Update(elapsed);
            _primaryGhostParticleSystem.Update(elapsed);
            _wallCrossGhostParticleSystem.Update(elapsed);
            base.Update(elapsed);
        }

        protected override void RecalculateZIndex()
        {
            base.RecalculateZIndex();
            _primaryGhostParticleSystem.LayerDepth = MainFigureDrawingInfos.LayerDepth - 0.00004f;
            _wallCrossGhostParticleSystem.LayerDepth = MainFigureDrawingInfos.LayerDepth - 0.00005f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _primaryGhostParticleSystem.Draw(spriteBatch);
            _wallCrossGhostParticleSystem.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }

        public override void DrawLightMask(SpriteBatch spriteBatch)
        {
            _lightMapGhostParticleSystem.Draw(spriteBatch);
            base.DrawLightMask(spriteBatch);
        }

    }
}
