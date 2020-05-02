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
    public class Bubu : Enemy
    {
        private readonly EnemyEyeParticleSystem _leftEye;
        private readonly EnemyEyeParticleSystem _rightEye;

        private Vector2? _leftEyeTraslation;
        private Vector2? _rightEyeTraslation;

        public Bubu(
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
                        characterName: "bubu",
                        startingPosition: patrolPath.First().CenterPosition,
                        movementSpeed: gameConfig.BubuSpeed,
                        characterTorchConfiguration: null),
                    new EnemyConfiguration(
                        assetsLoader: assetsLoader,
                        enemyName: "bubu",
                        attackSoundIndex: 0,//soundManager.RegisterBubuAttackSound(),
                        screamSoundIndex: 0,//soundManager.RegisterBubuScreamSound(),
                        nCellsFindingRange: gameConfig.BubuMaxCellsForNearCandle,
                        startingAI: new PatrolAI(patrolPath, gameGrid, candlesManager, player, soundManager, searchesCandles: true),
                        true),
                    soundManager,
                    player,
                    assetsLoader,
                    game,
                    candlesManager)
        {
            _leftEye = new EnemyEyeParticleSystem(
                assetsLoader,
                Color.White,
                new Color(225, 206, 172));
            _rightEye = new EnemyEyeParticleSystem(
                assetsLoader,
                Color.White,
                new Color(225, 206, 172));

            _deadBodyParticleEffect = new CharacterTorchParticleSystem(
                assetsLoader,
                Color.White,
                new Color(225, 206, 172));
        }

        protected override void OnChangeDirection(CharacterState characterState)
        {
            switch (characterState)
            {
                case CharacterState.GoingUp:
                case CharacterState.IdleUp:
                    _leftEyeTraslation = null; //new Vector2(-10f, -33f);
                    _rightEyeTraslation = null;//new Vector2(6f, -33f);
                    break;

                case CharacterState.GoingDown:
                case CharacterState.IdleDown:
                    _leftEyeTraslation = new Vector2(-10f, -33f);
                    _rightEyeTraslation = new Vector2(6f, -33f);
                    break;

                case CharacterState.GoingLeft:
                case CharacterState.IdleLeft:
                    _leftEyeTraslation = new Vector2(-20f, -41f);
                    _rightEyeTraslation = new Vector2(-15f, -41f);
                    break;

                case CharacterState.GoingRight:
                case CharacterState.IdleRight:
                    _leftEyeTraslation = new Vector2(15f, -41f);
                    _rightEyeTraslation = new Vector2(20f, -41f);
                    break;
            }

            base.OnChangeDirection(characterState);
        }

        public override void CustomActionOnPlayerDead()
        {
            _player.ShouldDoExplosionAnimation = true;
        }

        public override void Update(TimeSpan elapsed)
        {
            base.Update(elapsed);

            if (!IsDead)
            {
                if (_leftEyeTraslation != null)
                    _leftEye.AddParticles(MainFigureDrawingInfos.Position + _leftEyeTraslation.Value);
                if (_rightEyeTraslation != null)
                    _rightEye.AddParticles(MainFigureDrawingInfos.Position + _rightEyeTraslation.Value);

                _leftEye.Update(elapsed);
                _rightEye.Update(elapsed);
            }
        }

        public override void DrawEffectsOverDark(SpriteBatch spriteBatch)
        {
            base.DrawEffectsOverDark(spriteBatch);

            if (!IsDead)
            {
                _leftEye.Draw(spriteBatch);
                _rightEye.Draw(spriteBatch);
            }
        }
    }
}
