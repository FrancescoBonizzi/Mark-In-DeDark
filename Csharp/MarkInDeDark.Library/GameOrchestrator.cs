using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.StringsLocalization.Abstractions;
using FbonizziMonoGame.TransformationObjects;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace MarkInDeDark.Library
{
    public class GameOrchestrator
    {
        public enum GameStates
        {
            Menu,
            Playing,
            Levels
        }

        private GameStates _currentState;

        private readonly Func<Level, MarkInDeDarkGame> _gameFactory;
        private MarkInDeDarkGame _game;

        private readonly Func<MainMenuPage> _menuFactory;
        private MainMenuPage _menu;

        private readonly Func<LevelsPage> _levelsPageFactory;
        private LevelsPage _levels;

        private readonly FadeObject _stateTransition;
        private Action _afterTransitionAction;

        private RenderTarget2D _renderTarget;
        private readonly IScreenTransformationMatrixProvider _matrixScaleProvider;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly ILocalizedStringsRepository _localizedStringsRepository;
        private readonly SpriteFont _font;

        public bool ShouldEndApplication { get; private set; }
        public bool IsPaused { get; private set; } = false;

        private readonly TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(200);

        private DrawingInfos _inputTypeDrawingInfos = new DrawingInfos();
        private string _inputTypeText = string.Empty;

        private readonly MarkGamePad _markJoypad;

        public GameOrchestrator(
          Func<Level, MarkInDeDarkGame> gameFactory,
          Func<MainMenuPage> menuFactory,
          Func<LevelsPage> levelsPageFactory,
          GraphicsDevice graphicsDevice,
          IScreenTransformationMatrixProvider matrixScaleProvider,
          ILocalizedStringsRepository localizedStringsRepository,
          SpriteFont font,
          MarkGamePad markJoypad)
        {
            _gameFactory = gameFactory ?? throw new ArgumentNullException(nameof(gameFactory));
            _menuFactory = menuFactory ?? throw new ArgumentNullException(nameof(menuFactory));
            _levelsPageFactory = levelsPageFactory ?? throw new ArgumentNullException(nameof(levelsPageFactory));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _localizedStringsRepository = localizedStringsRepository ?? throw new ArgumentNullException(nameof(localizedStringsRepository));
            _font = font;
            _markJoypad = markJoypad;

            _matrixScaleProvider = matrixScaleProvider ?? throw new ArgumentNullException(nameof(matrixScaleProvider));
            if (_matrixScaleProvider is DynamicScalingMatrixProvider)
            {
                (_matrixScaleProvider as DynamicScalingMatrixProvider).ScaleMatrixChanged += GameOrchestrator_ScaleMatrixChanged;
            }
            RegenerateRenderTarget();

            ShouldEndApplication = false;

            _stateTransition = new FadeObject(_fadeDuration, Color.White);
            _stateTransition.FadeOutCompleted += _stateTransition_FadeOutCompleted;
        }

        private void GameOrchestrator_ScaleMatrixChanged(object sender, EventArgs e)
            => RegenerateRenderTarget();

        public void RegenerateRenderTarget()
        {
            _renderTarget = new RenderTarget2D(
                _graphicsDevice,
                _matrixScaleProvider.VirtualWidth,
                _matrixScaleProvider.VirtualHeight);
        }

        public void Start()
        {
            _currentState = GameStates.Menu;
            _menu = _menuFactory();
            _stateTransition.FadeIn();
        }

        public void StartWithForcedLevel(Level levelToLoad)
        {
            _currentState = GameStates.Playing;
            _game = _gameFactory(levelToLoad);
            _stateTransition.FadeIn();
        }

        private void _stateTransition_FadeOutCompleted(object sender, EventArgs e)
            => _afterTransitionAction();

        public void SetAfterGameState()
        {
            if (_stateTransition.IsFading)
                return;

            _game.StopMusic();

            ShouldEndApplication = false;
            // TODO Mettere uno slideshow con trucchi e consigli
            SetLevelState();
        }

        public void SetMenuState()
        {
            if (_currentState == GameStates.Menu)
                return;

            if (_stateTransition.IsFading)
                return;

            _stateTransition.FadeOut();
            _afterTransitionAction = new Action(
                () =>
                {
                    _stateTransition.FadeIn();

                    _currentState = GameStates.Menu;
                    _game?.OnLeavingPage();
                    _game = null;
                    _levels = null;
                    _menu = _menuFactory();
                });
        }

        public void SetGameState(Level levelToLoad, bool force = false)
        {
            if (_currentState == GameStates.Playing)
                return;

            if (_stateTransition.IsFading && !force)
                return;

            ShouldEndApplication = false;
            _stateTransition.FadeOut();
            _afterTransitionAction = new Action(
                () =>
                {
                    _stateTransition.FadeIn();
                    _currentState = GameStates.Playing;
                    _game = _gameFactory(levelToLoad);
                    GC.Collect();
                    _levels = null;
                    _menu = null;
                });
        }

        public void SetLevelState()
        {
            if (_currentState == GameStates.Levels)
                return;

            if (_stateTransition.IsFading)
                return;

            _stateTransition.FadeOut();
            _afterTransitionAction = new Action(
                () =>
                {
                    _stateTransition.FadeIn();

                    _currentState = GameStates.Levels;
                    _game?.OnLeavingPage();
                    _game = null;
                    _menu = null;
                    _levels = _levelsPageFactory();
                    GC.Collect();
                });
        }

        public void HandleMouseInput(Vector2? pointerLocation, bool hasClicked)
        {
            if (IsPaused)
                return;

            if (pointerLocation == null)
                return;

            switch (_currentState)
            {
                case GameStates.Menu:
                    _menu.HandleInput(pointerLocation.Value, hasClicked, this);
                    break;

                case GameStates.Playing:
                    break;

                case GameStates.Levels:
                    _levels.HandleInput(pointerLocation.Value, hasClicked, this);
                    break;
            }
        }

        public void HandleKeyboardPressed(CharacterActions action)
        {
            if (IsPaused)
                return;

            switch (_currentState)
            {
                case GameStates.Menu:
                    break;

                case GameStates.Playing:
                    _game.HandleInput(action);
                    break;

                case GameStates.Levels:
                    break;
            }
        }

        private void UpdateInputType()
        {
            if (_markJoypad.IsConnected)
            {
                _inputTypeText = _localizedStringsRepository.Get("InputTypeGamePadString");
            }
            else
            {
                _inputTypeText = _localizedStringsRepository.Get("InputTypeKeyboardString");
            }

            var inputTypeSize = _font.MeasureString(_inputTypeText);
            _inputTypeDrawingInfos = new DrawingInfos()
            {
                Position = new Vector2(_matrixScaleProvider.VirtualWidth / 2, _matrixScaleProvider.VirtualHeight - 70f),
                OverlayColor = Color.White,
                Scale = 0.2f,
                Origin = new Vector2(inputTypeSize.X / 2, 0)
            };
        }

        public void Update(TimeSpan elapsed)
        {
            UpdateInputType();

            if (IsPaused)
                return;

            if (_stateTransition.IsFading)
            {
                _stateTransition.Update(elapsed);
            }

            switch (_currentState)
            {
                case GameStates.Menu:
                    _menu.Update(elapsed);
                    break;

                case GameStates.Playing:
                    _game.Update(elapsed);
                    break;

                case GameStates.Levels:
                    _levels.Update(elapsed);
                    break;
            }

        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            _game?.Resume();
            IsPaused = false;
        }

        public void Pause()
        {
            if (IsPaused)
                return;

            _game?.Pause();
            IsPaused = true;
        }

        public void TogglePause()
        {
            if (IsPaused)
                Resume();
            else Pause();
        }

        public void Back()
        {
            switch (_currentState)
            {
                case GameStates.Menu:
                    ShouldEndApplication = true;
                    _menu.StopMusic();
                    break;

                case GameStates.Playing:
                    _game.StopMusic();
                    SetLevelState();
                    break;

                case GameStates.Levels:
                    SetMenuState();
                    _levels.StopMusic();
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, GameTime gameTime)
        {
            if (IsPaused)
                return;

            // Disegno tutto su un render target...
            graphics.SetRenderTarget(_renderTarget);
            graphics.Clear(Color.Black);

            switch (_currentState)
            {
                case GameStates.Menu:
                    _menu.Draw(spriteBatch);
                    spriteBatch.Begin(transformMatrix: _matrixScaleProvider.ScaleMatrix);
                    spriteBatch.DrawString(_font, _inputTypeText, _inputTypeDrawingInfos);
                    spriteBatch.End();
                    break;

                case GameStates.Playing:
                    _game.Draw(spriteBatch, _renderTarget, gameTime);
                    break;

                case GameStates.Levels:
                    _levels.Draw(spriteBatch);
                    spriteBatch.Begin(transformMatrix: _matrixScaleProvider.ScaleMatrix);
                    spriteBatch.DrawString(_font, _inputTypeText, _inputTypeDrawingInfos);
                    spriteBatch.End();
                    break;
            }

            // ...per poter fare il fade dei vari componenti in modo indipendente
            graphics.SetRenderTarget(null);
            graphics.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(_renderTarget, Vector2.Zero, _stateTransition.OverlayColor);
            spriteBatch.End();
        }
    }
}
