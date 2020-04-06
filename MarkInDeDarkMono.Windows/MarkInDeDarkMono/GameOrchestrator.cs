using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.TransformationObjects;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Levels;
using MarkInDeDarkMono.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono
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

        private FadeObject _stateTransition;
        private Action _afterTransitionAction;

        private RenderTarget2D _renderTarget;
        private readonly IScreenTransformationMatrixProvider _matrixScaleProvider;

        private readonly GraphicsDevice _graphicsDevice;

        public bool ShouldEndApplication { get; private set; }
        public bool IsPaused { get; private set; } = false;

        private readonly TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(800);
        private readonly Uri _aboutUri = new Uri("https://www.fbonizzi.it");

        public GameOrchestrator(
          Func<Level, MarkInDeDarkGame> gameFactory,
          Func<MainMenuPage> menuFactory,
          Func<LevelsPage> levelsPageFactory,
          GraphicsDevice graphicsDevice,
          IScreenTransformationMatrixProvider matrixScaleProvider)
        {
            _gameFactory = gameFactory ?? throw new ArgumentNullException(nameof(gameFactory));
            _menuFactory = menuFactory ?? throw new ArgumentNullException(nameof(menuFactory));
            _levelsPageFactory = levelsPageFactory ?? throw new ArgumentNullException(nameof(levelsPageFactory));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

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

                    //if (_currentState == GameStates.Playing && _game.IsGameOver)
                    //{
                    //    // Significa che sono uscito dal gioco
                    //    // perché ho perso
                    //    GameOver?.Invoke(this, EventArgs.Empty);
                    //}

                    _currentState = GameStates.Menu;
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
                    _game = null;
                    _menu = null;
                    _levels = _levelsPageFactory();
                });
        }

        public void HandleMouseInput(Vector2? touchLocation = null)
        {
            if (IsPaused)
                return;

            if (touchLocation == null)
                return;

            switch (_currentState)
            {
                case GameStates.Menu:
                    _menu.HandleInput(touchLocation.Value, this);
                    break;

                case GameStates.Playing:
                    break;

                case GameStates.Levels:
                    _levels.HandleInput(touchLocation.Value, this);
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

        public void Update(TimeSpan elapsed)
        {
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

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics)
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
                    break;

                case GameStates.Playing:
                    _game.Draw(spriteBatch, _renderTarget);
                    break;

                case GameStates.Levels:
                    _levels.Draw(spriteBatch);
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
