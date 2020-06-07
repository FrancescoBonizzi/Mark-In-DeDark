using FbonizziMonoGame;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Lighting;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Characters
{
    public class Player : Character
    {
        private readonly PlayerWinAnimation _winAnimation;
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

        public Player(
            AssetsLoader assetsLoader,
            GameGrid gameGrid,
            GameConfig gameConfig,
            CandlesManager candlesManager,
            SoundManager soundManager,
            MarkInDeDarkGame game)
            : base(gameGrid,
                  soundManager: soundManager,
                  assetsLoader: assetsLoader,
                    configuration: new CharacterConfiguration(
                        assetsLoader: assetsLoader,
                        characterName: "player",
                        startingPosition: gameGrid.PlayerStartingPosition,
                        characterTorchConfiguration: new CharacterTorchConfiguration(
                            firstTorchColor: gameConfig.PlayerLightColorRGB1,
                            secondTorchColor: gameConfig.PlayerLightColorRGB2,
                            torchPositionTraslationUp: new Vector2(-42, -38),
                            torchPositionTraslationLeft: new Vector2(-38, -38),
                            torchPositionTraslationRight: new Vector2(38, -38),
                            torchPositionTraslationDown: new Vector2(35, -30),
                            z: 90f,
                            opacity: 1f,
                            distanceSquared: 20_600f,
                            penumbraTorchScale: 1200f),
                        movementSpeed: gameConfig.PlayerSpeed),
                    game: game)
        {
            _soundManager = soundManager;
            _candlesManager = candlesManager;
            _doesntDie = gameConfig.GodModePlayerDoesntDie;

            _currentPlayerState = PlayerStates.Playing;

            _playerCandleFireParticleSystem = new CandleFireParticleSystem(assetsLoader);
            _winAnimation = new PlayerWinAnimation(assetsLoader);

            AreLightsActive = true;
            //_soundManager.PlayPlayerTorch();
            SetLightPosition(0.5f);
            _deadBodyPartLifetime = TimeSpan.FromSeconds(8f);
            _deadBodyEffectWithFading = false;

            MainAnimationsAnimationManager.AddAnimation(
              "Dying",
              assetsLoader.GetAnimation("playerDying"));
            
            SurroundingLight.PenumbraLight.CastsShadows = true;
            SurroundingLight.PenumbraLight.Enabled = true;
        }

        public bool HasWon
            => _currentPlayerState == PlayerStates.Exiting;

        public override void HandleInput(CharacterActions action)
        {
            if (_currentPlayerState != PlayerStates.Playing)
            {
                return;
            }

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
            MainFigureDrawingInfos.Position = exit.Position;
            _currentPlayerState = PlayerStates.Exiting;
            //_soundManager.PlayLadder();
            _winAnimation.SetPosition(Position);
        }

        public override void Update(TimeSpan elapsed)
        {
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

                    base.Update(elapsed);

                    if (!IsDead)
                    {
                        _playerCandleFireParticleSystem.AddParticles(TorchPosition);
                    }

                    break;

                case PlayerStates.Exiting:
                    SetLightPosition(0.2f, _winAnimation.DrawingInfos.Position + _torchPositionTraslation);
                    SurroundingLight?.Update(elapsed);
                    _playerCandleFireParticleSystem.AddParticles(TorchPosition);
                    _winAnimation.Update(elapsed);

                    // Per far sparire la luce con lui
                    SurroundingLight.Opacity = Numbers.MapValueFromIntervalToInterval(
                        _winAnimation.DrawingInfos.OverlayColor.A,
                        0, 255,
                        0, 1);
                    SurroundingLight.ForceColor(Color.White * SurroundingLight.Opacity);

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

        public override void DrawLightMask(SpriteBatch spriteBatch)
        {
            base.DrawLightMask(spriteBatch);
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
