using System;
using System.Collections.Generic;
using System.Linq;
using FbonizziMonoGame.Extensions;
using MarkInDeDark.Library.AI;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDark.Library.Characters
{
    public class Ghost : Enemy
    {
        private readonly GhostParticleSystem _wallCrossGhostParticleSystem;

        public Ghost(
            AssetsLoader assetsLoader,
            List<GameCell> patrolPath,
            GameGrid gameGrid,
            CandlesManager candlesManager,
            SoundManager soundManager,
            Player player,
            GameConfig gameConfig,
            MarkInDeDarkGame game
        )
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
                        opacity: 1f,
                        distanceSquared: 4_950f,
                        penumbraTorchScale: 300f
                    ),
                    movementSpeed: gameConfig.GhostSpeed,
                    withDieAnimation: false
                ),
                enemyConfiguration: new EnemyConfiguration(
                    assetsLoader: assetsLoader,
                    enemyName: "ghost",
                    attackSoundIndex: 0, //soundManager.RegisterGhostAttackSound(),
                    screamSoundIndex: 0, //soundManager.RegisterGhostScreamSound(),
                    nCellsFindingRange: null,
                    startingAI: new PatrolGhostAI(patrolPath, gameGrid, player, soundManager),
                    killsCandlesIfCollidesWithThem: true
                ),
                soundManager: soundManager,
                player: player,
                assetsLoader: assetsLoader,
                game: game,
                candlesManager: candlesManager
            )
        {
            _canGoOverWalls = true;

            var deltaPosition = new Vector2(0, -15f);
            MainFigureDrawingInfos.Origin -= deltaPosition;
            MainFigureDrawingInfos.HitBoxTolerance = new Rectangle(30, 30, 46, 46);

            _wallCrossGhostParticleSystem = new GhostParticleSystem(
                assetsLoader,
                Color.LimeGreen.WithAlpha(0.2f),
                Color.Lime.WithAlpha(0.2f)
            );

            SurroundingLight.DeltaForWall = new Vector2(0f, -30f);
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
            // Tenere per primo altrimenti sovrascrive il LayerDepth
            base.Update(elapsed);

            if (_gameGrid.CollideWithAWall(CollisionRectangle))
            {
                _wallCrossGhostParticleSystem.AddParticles(WorldCenter);
                MainFigureDrawingInfos.LayerDepth -= 0.02f;
            }

            _wallCrossGhostParticleSystem.Update(elapsed);
        }

        protected override void RecalculateZIndex()
        {
            base.RecalculateZIndex();
            _wallCrossGhostParticleSystem.LayerDepth = MainFigureDrawingInfos.LayerDepth - 0.00005f;
        }

        public override void DrawEffectsOverDark(SpriteBatch spriteBatch)
        {
            _wallCrossGhostParticleSystem.Draw(spriteBatch);
            base.DrawEffectsOverDark(spriteBatch);
        }
    }
}
