using Mark.Engine;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Levels;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Characters
{
    public class Player : Character
    {
        private PlayerDescendingStairs _winAnimation;
        private SparkleParticleSystem _sparkleEffect;

        private readonly SoundManager _soundManager;
        private readonly CandlesManager _candlesManager;
        private readonly GameGrid _gameGrid;

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
                        assetsLoader,
                        "player",
                        gameGrid.PlayerStartingPosition,
                        gameConfig.PlayerLightColorRGB1,
                        gameConfig.PlayerLightColorRGB2,
                        gameConfig.PlayerLightSize,
                        gameConfig.PlayerSpeed,
                        torchPositionTraslationUp: new Vector2(-16, -20),
                        torchPositionTraslationLeft: new Vector2(-20, -20),
                        torchPositionTraslationRight: new Vector2(20, -20),
                        torchPositionTraslationDown: new Vector2(16, -20)))
        {
            _soundManager = soundManager;
            _candlesManager = candlesManager;
            _gameGrid = gameGrid;

            _currentPlayerState = PlayerStates.LevelIntro;

            _sparkleEffect = new SparkleParticleSystem(assetsLoader);
            _sparkleEffect.AddParticles(WorldCenter + new Vector2(16, -20));

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

            if (action == CharacterActions.Candle)
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
            _drawingInfos.Position = exit.Position;
            _currentPlayerState = PlayerStates.Exiting;
            _soundManager.PlayLadder();
            _winAnimation.SetPosition(Position);
        }

        public override void Update(TimeSpan elapsed)
        {
            switch (_currentPlayerState)
            {
                case PlayerStates.LevelIntro:

                    _sparkleEffect.Update(elapsed);
                    if (!_sparkleEffect.HasActiveParticles)
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

                    base.Update(elapsed);

                    break;

                case PlayerStates.Exiting:
                    _winAnimation.Update(elapsed);
                    break;
            }
        }

        public override void DrawLightMap(SpriteBatch spriteBatch)
        {
            if (_currentPlayerState == PlayerStates.LevelIntro)
            {
                _sparkleEffect.Draw(spriteBatch);
            }

            base.DrawLightMap(spriteBatch);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_currentPlayerState)
            {
                case PlayerStates.LevelIntro:
                    _sparkleEffect.Draw(spriteBatch);
                    break;

                case PlayerStates.Playing:
                    base.Draw(spriteBatch);
                    break;

                case PlayerStates.Exiting:
                    _winAnimation.Draw(spriteBatch);
                    break;
            }
        }
    }
}
