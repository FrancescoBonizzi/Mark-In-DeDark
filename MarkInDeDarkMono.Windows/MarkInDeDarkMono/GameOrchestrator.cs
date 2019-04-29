using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.AfterGameManagement;
using FlowsoftGamesMonogame.Drawing.ViewportAdapters;
using FlowsoftGamesMonogame.UI;
using Mark.Engine;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Levels;
using MarkInDeDarkMono.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        private readonly Matrix _scaleMatrix;
        private readonly AfterGameManager _afterGameManager;
        private readonly ISettingsRepository _settingsRepository;

        private FadeObject _stateTransition;
        private Action _afterTransitionAction;

        private readonly RenderTarget2D _renderTarget;
        private readonly ViewportAdapter _viewportAdapter;

        public bool ShouldEndApplication { get; private set; }
        public bool IsPaused { get; private set; } = false;

        private readonly TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(800);
        private bool _thisGameAfterGameManagerCalled = false;

        public event EventHandler ShowAd;

        public GameOrchestrator(
          Func<Level, MarkInDeDarkGame> gameFactory,
          Func<MainMenuPage> menuFactory,
          Func<LevelsPage> levelsPageFactory,
          GraphicsDevice graphicsDevice,
          ISettingsRepository settingsRepository,
          ViewportAdapter viewportAdapter,
          AfterGameManager afterGameManager)
        {
            _gameFactory = gameFactory ?? throw new ArgumentNullException(nameof(gameFactory));
            _menuFactory = menuFactory ?? throw new ArgumentNullException(nameof(menuFactory));
            _levelsPageFactory = levelsPageFactory ?? throw new ArgumentNullException(nameof(levelsPageFactory));
            _scaleMatrix = viewportAdapter.GetScaleMatrix();

            _afterGameManager = afterGameManager ?? throw new ArgumentNullException(nameof(afterGameManager));
            _settingsRepository = settingsRepository;

            _viewportAdapter = viewportAdapter;
            _renderTarget = new RenderTarget2D(
                graphicsDevice,
                viewportAdapter.ViewportWidth,
                viewportAdapter.ViewportHeight);

            ShouldEndApplication = false;

            _stateTransition = new FadeObject(_fadeDuration, Color.White);
            _stateTransition.FadeOutCompleted += _stateTransition_FadeOutCompleted;
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

            if (_thisGameAfterGameManagerCalled)
                return;

            _game.StopMusic();

            ShouldEndApplication = false;
            _thisGameAfterGameManagerCalled = true;
            var newState = _afterGameManager.GetAfterGameState();
            switch (newState)
            {
                case AfterGameManager.AfterGameStates.ShowAd:
                    ShowAd?.Invoke(this, EventArgs.Empty);
                    SetLevelState();
                    break;

                case AfterGameManager.AfterGameStates.ShowProTip:
                    // No protip in this game
                    break;
            }
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

        public void SetGameState(Level levelToLoad)
        {
            if (_currentState == GameStates.Playing)
                return;

            if (_stateTransition.IsFading)
                return;

            ShouldEndApplication = false;
            _thisGameAfterGameManagerCalled = false;
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

        public void HandleTouchInput(Vector2? touchLocation = null)
        {
            if (touchLocation == null)
                return;

            switch (_currentState)
            {
                case GameStates.Menu:
                    _menu.HandleInput(touchLocation.Value, this);
                    break;

                case GameStates.Playing:
                    // TODO trasformare la posizione del touch in una action
                    break;

                case GameStates.Levels:
                    _levels.HandleInput(touchLocation.Value, this);
                    break;
            }
        }

        public void HandleKeyboardPressed(CharacterActions action)
        {
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
            _game?.Resume();
            IsPaused = false;
        }

        public void Pause()
        {
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
