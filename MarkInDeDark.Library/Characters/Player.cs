
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Characters
{
    public class Player : Character
    {
        private readonly PlayerDescendingStairsAnimation _winAnimation;
        private readonly CandleFireParticleSystem _playerCandleFireParticleSystem;

        private readonly SoundManager _soundManager;
        private readonly CandlesManager _candlesManager;
        private readonly bool _doesntDie;

        private enum PlayerStates
        {
            Playing,
            Exiting
        }

        private PlayerStates _currentPlayerState;
        public Vector2 CurrentDirection { get; private set; }

        public Player(
            AssetsLoader assetsLoader,
            GameGrid gameGrid,
            GameConfig gameConfig,
            CandlesManager candlesManager,
            SoundManager soundManager,
            MarkInDeDarkGame game)
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
                        movementSpeed: gameConfig.PlayerSpeed),
                    game)
        {
            _soundManager = soundManager;
            _candlesManager = candlesManager;
            _doesntDie = gameConfig.GodModePlayerDoesntDie;

            _currentPlayerState = PlayerStates.Playing;

            _playerCandleFireParticleSystem = new CandleFireParticleSystem(assetsLoader);
            _winAnimation = new PlayerDescendingStairsAnimation(assetsLoader);

            AreLightsActive = true;
            //_soundManager.PlayPlayerTorch();
            SetLightPosition(0.5f);
            _deadBodyPartLifetime = TimeSpan.FromSeconds(8f);
            _dieMarkEffect = null;
            _deadBodyEffectWithFading = false;
        }

        public bool HasWon
            => _currentPlayerState == PlayerStates.Exiting;

        public void HandleInput(HashSet<CharacterActions> actions)
        {
            if (_currentPlayerState != PlayerStates.Playing)
            {
                return;
            }

            if (actions.Contains(CharacterActions.DoorSwitch))
            {
                if (CurrentCharacterState != CharacterState.Attacking
                    && CurrentCharacterState != CharacterState.Dead
                    && CurrentCharacterState != CharacterState.Dying)
                {
                    _gameGrid.SwitchDoorIfAny(_gameGrid.GetCellAtPixel(WorldCenter));
                }
            }
            else if (actions.Contains(CharacterActions.Candle))
            {
                _candlesManager.PlaceCandle(_gameGrid.GetCellAtPixel(WorldCenter).CenterPosition);
            }
        }

        public void Win(Exit exit)
        {
            MainFigureDrawingInfos.Position = exit.Position;
            _currentPlayerState = PlayerStates.Exiting;
            //_soundManager.PlayLadder();
            _winAnimation.SetPosition(Position);
        }

        public void Update(TimeSpan elapsed, HashSet<CharacterActions> inputs)
        {
            var deltaPosition = Vector2.Zero;
            foreach (var i in inputs)
            {
                if (i == CharacterActions.Up)
                    deltaPosition.Y -= 1;
                if (i == CharacterActions.Down)
                    deltaPosition.Y += 1;
                if (i == CharacterActions.Right)
                    deltaPosition.X += 1;
                if (i == CharacterActions.Left)
                    deltaPosition.X -= 1;
            }

            Update(elapsed, deltaPosition);
        }

        public override void Update(TimeSpan elapsed, Vector2 direction)
        {
            CurrentDirection = direction;

            switch (_currentPlayerState)
            {
                case PlayerStates.Playing:

                    //if (IsMoving)
                    //{
                    //    _soundManager.PlayPlayerWalking();
                    //}
                    //else
                    //{
                    //    _soundManager.StopPlayerWalking();
                    //}

                    _playerCandleFireParticleSystem.Update(elapsed);

                    base.Update(elapsed, direction);

                    if (!IsDead)
                    {
                        _playerCandleFireParticleSystem.AddParticles(TorchPosition);
                    }

                    break;

                case PlayerStates.Exiting:
                    _winAnimation.Update(elapsed);
                    break;
            }
        }

        protected override void RecalculateZIndex()
        {
            base.RecalculateZIndex();
            _playerCandleFireParticleSystem.LayerDepth = MainFigureDrawingInfos.LayerDepth + 0.00004f;
        }

        public override void Die()
        {
            if (_doesntDie)
            {
                return;
            }

            base.Die();
        }

        public override void DrawLightMap(SpriteBatch spriteBatch)
        {
            base.DrawLightMap(spriteBatch);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_currentPlayerState)
            {
                case PlayerStates.Playing:
                    base.Draw(spriteBatch);

                    if (!IsDead && !IsDying)
                        _playerCandleFireParticleSystem.Draw(spriteBatch);

                    break;

                case PlayerStates.Exiting:
                    _winAnimation.Draw(spriteBatch);
                    break;
            }
        }
    }
}
