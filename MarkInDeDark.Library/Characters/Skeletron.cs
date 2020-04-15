using MarkInDeDark.Library;
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
    public class Skeletron : Enemy
    {
        private readonly SkeletonLightParticleSystem _skeletonLightParticleSystem;

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
                    torchPositionTraslationUp: new Vector2(-45, -15),
                    torchPositionTraslationLeft: new Vector2(-30, -5),
                    torchPositionTraslationRight: new Vector2(30, -5),
                    torchPositionTraslationDown: new Vector2(35, 0),
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
            _skeletonLightParticleSystem = new SkeletonLightParticleSystem(assetsLoader);
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

        public override void DrawEffects(SpriteBatch spriteBatch)
        {
            _skeletonLightParticleSystem.Draw(spriteBatch);
            base.DrawEffects(spriteBatch);
        }
    }
}
