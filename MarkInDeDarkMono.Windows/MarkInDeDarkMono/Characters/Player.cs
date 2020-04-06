using MarkInDeDarkMono;

using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Characters
{
    public class Player : Character
    {
        private readonly PlayerDescendingStairs _winAnimation;
        private readonly GameStartSparkleParticleSystem _gameStartSparkleEffect;
        private readonly CandleFireParticleSystem _playerCandleFireParticleSystem;

        private readonly SoundManager _soundManager;
        private readonly CandlesManager _candlesManager;
        private readonly bool _doesntDie;

        private enum PlayerStates
        {
            LevelIntro,
            Playing,
            Exiting
        }

        private PlayerStates _currentPlayerState;

        public Player(
            AssetsLoader assetsLoader,
            GameGrid gameGrid,
            GameConfig gameConfig,
            CandlesManager candlesManager,
            SoundManager soundManager)
            : base(gameGrid,
                  soundManager,
                  assetsLoader,
                    new CharacterConfiguration(
                        assetsLoader: assetsLoader,
                        characterName: "player",
                        startingPosition: gameGrid.PlayerStartingPosition,
                        characterTorchConfiguration: new CharacterTorchConfiguration(
                            firstTorchColor: gameConfig.PlayerLightColorRGB1,
                            secondTorchColor: gameConfig.PlayerLightColorRGB2,
                            torchScale: gameConfig.PlayerLightSize,
                            torchPositionTraslationUp: new Vector2(-42, -38),
                            torchPositionTraslationLeft: new Vector2(-38, -38),
                            torchPositionTraslationRight: new Vector2(38, -38),
                            torchPositionTraslationDown: new Vector2(35, -30),
                            assetsLoader: assetsLoader),
                        movementSpeed: gameConfig.PlayerSpeed,
                        withShadowAnimation: true))
        {
            _soundManager = soundManager;
            _candlesManager = candlesManager;
            _doesntDie = gameConfig.GodModePlayerDoesntDie;

            _currentPlayerState = PlayerStates.LevelIntro;

            _gameStartSparkleEffect = new GameStartSparkleParticleSystem(assetsLoader);
            _gameStartSparkleEffect.AddParticles(WorldCenter + new Vector2(16, -20));

            _playerCandleFireParticleSystem = new CandleFireParticleSystem(assetsLoader);
            _winAnimation = new PlayerDescendingStairs(assetsLoader);

            _soundManager.PlayEnlightTorch();
            AreLightsActive = false;
        }

        public bool HasWon
            => _currentPlayerState == PlayerStates.Exiting;

        public override void HandleInput(CharacterActions action)
        {
            if (_currentPlayerState != PlayerStates.Playing)
                return;

            if (action == CharacterActions.DoorSwitch)
            {
                if (CurrentCharacterState != CharacterState.Attacking
                    && CurrentCharacterState != CharacterState.Dead
                    && CurrentCharacterState != CharacterState.Dying)
                {
                    _gameGrid.SwitchDoorIfAny(_gameGrid.GetCellAtPixel(WorldCenter));
                }
            }
            else if (action == CharacterActions.Candle)
            {
                _candlesManager.PlaceCandle(_gameGrid.GetCellAtPixel(WorldCenter).CenterPosition);
            }
            else
            {
                base.HandleInput(action);
            }
        }

        public void Win(Exit exit)
        {
            _mainFigureDrawingInfos.Position = exit.Position;
            _currentPlayerState = PlayerStates.Exiting;
            _soundManager.PlayLadder();
            _winAnimation.SetPosition(Position);
        }

        public override void Update(TimeSpan elapsed)
        {
            switch (_currentPlayerState)
            {
                case PlayerStates.LevelIntro:

                    _gameStartSparkleEffect.Update(elapsed);
                    if (!_gameStartSparkleEffect.HasActiveParticles)
                    {
                        _currentPlayerState = PlayerStates.Playing;
                        AreLightsActive = true;
                        _soundManager.PlayPlayerTorch();
                        SetLightPosition(0.5f);
                    }

                    break;

                case PlayerStates.Playing:

                    if (IsMoving)
                        _soundManager.PlayPlayerWalking();
                    else
                        _soundManager.StopPlayerWalking();

                    _playerCandleFireParticleSystem.Update(elapsed);

                    if (!IsDead)
                    {
                        _playerCandleFireParticleSystem.AddParticles(TorchPosition);
                    }

                    base.Update(elapsed);

                    break;

                case PlayerStates.Exiting:
                    _winAnimation.Update(elapsed);
                    break;
            }
        }

        public override void Die()
        {
            if (_doesntDie)
                return;

            base.Die();
        }

        public override void DrawLightMap(SpriteBatch spriteBatch)
        {
            if (_currentPlayerState == PlayerStates.LevelIntro)
            {
                _gameStartSparkleEffect.Draw(spriteBatch);
            }

            base.DrawLightMap(spriteBatch);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_currentPlayerState)
            {
                case PlayerStates.LevelIntro:
                    _gameStartSparkleEffect.Draw(spriteBatch);
                    break;

                case PlayerStates.Playing:
                    base.Draw(spriteBatch);
                    _playerCandleFireParticleSystem.Draw(spriteBatch);
                    break;

                case PlayerStates.Exiting:
                    _winAnimation.Draw(spriteBatch);
                    break;
            }
        }
    }
}
