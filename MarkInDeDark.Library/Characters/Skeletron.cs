using MarkInDeDark.Library;
using MarkInDeDark.Library.AI;

using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MarkInDeDark.Library.Characters
{
    public class Skeletron : Enemy
    {
        private readonly CharacterTorchParticleSystem _skeletonLightParticleSystem;

        public Skeletron(
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
                characterName: "skeleton",
                startingPosition: patrolPath.First().CenterPosition,
                characterTorchConfiguration: new CharacterTorchConfiguration(
                    firstTorchColor: gameConfig.SkeletronLightColorRGB1,
                    secondTorchColor: gameConfig.SkeletronLightColorRGB2,
                    torchScale: gameConfig.SkeletronLightSize,
                    torchScaleMultiplierForPenumbra: 600,
                    torchPositionTraslationUp: new Vector2(-45, -15),
                    torchPositionTraslationLeft: new Vector2(-30, -5),
                    torchPositionTraslationRight: new Vector2(30, -5),
                    torchPositionTraslationDown: new Vector2(35, 0)),
                movementSpeed: gameConfig.SkeletronSpeed),
            enemyConfiguration: new EnemyConfiguration(
                assetsLoader: assetsLoader,
                enemyName: "skeleton",
                attackSoundIndex: 0,//soundManager.RegisterSkeletronAttackSound(),
                screamSoundIndex: 0,//soundManager.RegisterSkeletronScreamSound(),
                nCellsFindingRange: gameConfig.SkeletronMaxCellsForNearCandle,
                startingAI: new PatrolHunterLineOfSightAI(patrolPath, gameGrid, candlesManager, player, soundManager, gameConfig),
                true),
            soundManager: soundManager,
            player: player,
            assetsLoader: assetsLoader,
            game: game,
            candlesManager: candlesManager)
        {
            SurroundingLight.AloneEnabled = true;
            _skeletonLightParticleSystem = new CharacterTorchParticleSystem(
                assetsLoader, 
                gameConfig.SkeletronLightColorRGB1,
                gameConfig.SkeletronLightColorRGB2);
        }

        protected override void RecalculateZIndex()
        {
            base.RecalculateZIndex();
            _skeletonLightParticleSystem.LayerDepth = MainFigureDrawingInfos.LayerDepth + 0.00004f;
        }

        public override void Update(TimeSpan elapsed)
        {
            _skeletonLightParticleSystem.Update(elapsed);

            if (!IsDead)
            {
                _skeletonLightParticleSystem.AddParticles(TorchPosition);
            }

            base.Update(elapsed);
        }

        public override void UpdateAfterDeath(TimeSpan elapsed)
        {
            _skeletonLightParticleSystem.Update(elapsed);
            base.UpdateAfterDeath(elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _skeletonLightParticleSystem.Draw(spriteBatch);
        }
    }
}
