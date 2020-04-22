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
                  gameGrid,
                  new CharacterConfiguration(
                      assetsLoader: assetsLoader,
                      characterName: "ghost",
                      startingPosition: patrolPath.First().CenterPosition,
                      characterTorchConfiguration: new CharacterTorchConfiguration(
                        firstTorchColor: gameConfig.GhostLightColorRGB1,
                        secondTorchColor: gameConfig.GhostLightColorRGB2,
                        torchScale: gameConfig.GhostLightSize,
                        torchPositionTraslationUp: Vector2.Zero,
                        torchPositionTraslationLeft: Vector2.Zero,
                        torchPositionTraslationRight: Vector2.Zero,
                        torchPositionTraslationDown: Vector2.Zero,
                        assetsLoader: assetsLoader),
                      movementSpeed: gameConfig.GhostSpeed,
                      withDieAnimation: false),
                  new EnemyConfiguration(
                      assetsLoader: assetsLoader,
                      enemyName: "ghost",
                      attackSoundIndex: 0, //soundManager.RegisterGhostAttackSound(),
                      screamSoundIndex: 0, //soundManager.RegisterGhostScreamSound(),
                      nCellsFindingRange: null,
                      startingAI: new GhostAI(patrolPath, gameGrid),
                      killsCandlesIfCollidesWithThem: true),
                  soundManager,
                  player,
                  assetsLoader,
                  game,
                  candlesManager)
        {
            _canGoOverWalls = true;

            // E' lui stesso il glow
            _torch = null;
            var deltaPosition = new Vector2(0, -15f);
            MainFigureDrawingInfos.Origin -= deltaPosition;

            _primaryGhostParticleSystem = new GhostParticleSystem(
                assetsLoader,
                new Color(101, 221, 160),
                new Color(71, 191, 117));

            _wallCrossGhostParticleSystem = new GhostParticleSystem(
                assetsLoader,
                Color.Gold,
                Color.Goldenrod);
        }

        public override void Die()
        {
            // Il Ghost non può morire
            return;
        }

        protected override void KillCandleLogic(Candle possibleCandle, TimeSpan elapsed)
        {
            possibleCandle.DieFast();
        }

        public override bool CustomConditionToAttackPlayer()
        {
            // Attacca il player solo se si muove
            return _player.CurrentCharacterState == CharacterState.Moving;
        }

        public override void Update(TimeSpan elapsed)
        {
            _primaryGhostParticleSystem.AddParticles(WorldCenter);

            if (_gameGrid.CollideWithAWall(CollisionRectangleForMovement))
            {
                _wallCrossGhostParticleSystem.AddParticles(WorldCenter);
            }

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

    }
}
