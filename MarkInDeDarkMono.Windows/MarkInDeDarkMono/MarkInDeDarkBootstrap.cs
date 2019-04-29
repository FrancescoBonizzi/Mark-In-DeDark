using FlowsoftGamesMonogame;
using FlowsoftGamesMonogame.AfterGameManagement;
using FlowsoftGamesMonogame.Assets;
using FlowsoftGamesMonogame.Drawing.ViewportAdapters;
using FlowsoftGamesMonogame.InputListeners;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Levels;
using MarkInDeDarkMono.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MarkInDeDarkMono
{
    public class MarkInDeDarkBootstrap : Game
    {
        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        private SpriteBatch _spriteBatch;

        private SplashScreenLoader _splashScreenAssetsLoader;

        private readonly ITextFileLoader _textFileAssetsLoader;
        private readonly ISettingsRepository _settingsRepository;
        private List<IInputListener> _inputListeners;

        private readonly CultureInfo _gameCulture;
        private readonly GameConfig _gameConfig;

        public event EventHandler ExitGameRequested;
        public event EventHandler ShowAd;

        private GameOrchestrator _orchestrator;
        private AfterGameManager _afterGameManager;
        private AssetsLoader _assetsLoader;
        private ScalingViewportAdapter _viewportAdapter;
        private Matrix _scaleMatrix;

        private readonly IEnumerable<string> _levelsNames;
        private readonly bool _isPc;

        // private Sprite _mousePointer;

        private SoundManager _soundManager;
        private MarkKeyboard _markKeyboard;

        public MarkInDeDarkBootstrap(
            ITextFileLoader textFileAssetsLoader,
            ISettingsRepository settingsRepository,
            CultureInfo gameCulture,
            bool isPc,
            IEnumerable<string> levelNames)
        {
            if (isPc)
            {
                IsMouseVisible = true;
                _isPc = isPc;
            }

            _textFileAssetsLoader = textFileAssetsLoader;
            _settingsRepository = settingsRepository;
            _levelsNames = levelNames;
            _gameCulture = gameCulture;
            _gameConfig = new GameConfig(textFileAssetsLoader.LoadFile(@"Content/GameConfig.txt"));

            GraphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight,
                IsFullScreen = _gameConfig.IsFullScreen
            };
        }

#warning Questo lo metto nel costruttore così non va più settato fuori
        public void SetGraphicsDeviceViewportSize(int width, int height)
        {
            if (GraphicsDeviceManager.IsFullScreen)
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = 1920;
                GraphicsDeviceManager.PreferredBackBufferHeight = 1080;
            }
            else
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = width;
                GraphicsDeviceManager.PreferredBackBufferHeight = height;
            }
        }

        protected override void Initialize()
        {
            _viewportAdapter = new ScalingViewportAdapter(
                  GraphicsDeviceManager.GraphicsDevice,
                  800, 480); // Questo diventerà 1920x1080
            _scaleMatrix = _viewportAdapter.GetScaleMatrix();

            base.Initialize();
        }

        private void LoadGameAssets()
        {
            new GameStrings(_gameCulture);

            _afterGameManager = new AfterGameManager(false, 1);

            _assetsLoader = new AssetsLoader(
                Content, 
                _textFileAssetsLoader, 
                _levelsNames,
                _settingsRepository);
            _soundManager = new SoundManager(_assetsLoader);

            // _mousePointer = AssetsLoader.Sprites["manina"];

            var gameFactory = new Func<Level, MarkInDeDarkGame>(
                level => new MarkInDeDarkGame(
                    GraphicsDevice,
                    _orchestrator,
                    _assetsLoader,
                    level,
                    _gameConfig,
                    _viewportAdapter,
                    _settingsRepository,
                    _soundManager));

            var menuFactory = new Func<MainMenuPage>(
                () => new MainMenuPage(
                    _assetsLoader,
                    _soundManager,
                    _viewportAdapter));

            var levelsPageFactory = new Func<LevelsPage>(
                () => new LevelsPage(_viewportAdapter, _assetsLoader));

            _orchestrator = new GameOrchestrator(
                gameFactory,
                menuFactory,
                levelsPageFactory,
                _spriteBatch.GraphicsDevice,
                _settingsRepository,
                _viewportAdapter,
                _afterGameManager);

            _orchestrator.ShowAd += _orchestrator_ShowAd;

            _inputListeners = new List<IInputListener>();
            if (_isPc)
            {
                var mouseListener = new MouseListener(_viewportAdapter);
                mouseListener.MouseDown += MouseListener_MouseClicked;

                var keyboardListener = new KeyboardListener();
                keyboardListener.KeyPressed += KeyboardListener_KeyPressed;

                _inputListeners.Add(mouseListener);
                _inputListeners.Add(keyboardListener);

                _markKeyboard = new MarkKeyboard();
            }
            else
            {
                var touchListener = new TouchListener(_viewportAdapter);
                touchListener.TouchStarted += TouchListener_TouchEnded;

                var gamepadListener = new GamePadListener();
                gamepadListener.ButtonDown += GamepadListener_ButtonDown;

                _inputListeners.Add(touchListener);
                _inputListeners.Add(gamepadListener);
            }
        }

        private void _orchestrator_ShowAd(object sender, EventArgs e)
            => ShowAd?.Invoke(this, EventArgs.Empty);

        private void GamepadListener_ButtonDown(object sender, GamePadEventArgs e)
        {
            if (e.Button == Buttons.Back)
            {
                _orchestrator.Back();
                if (_orchestrator.ShouldEndApplication)
                {
                    if (ExitGameRequested != null)
                        ExitGameRequested(this, EventArgs.Empty); // Se ho un handler specifico, uso quello
                    else
                        Exit(); // Devono ancora fixare il problema dell'uscita da Android
                }
            }
        }

        private void KeyboardListener_KeyPressed(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Keys.P)
            {
                _orchestrator.TogglePause();
            }
            else if (e.Key == Keys.Escape)
            {
                _orchestrator.Back();
                if (_orchestrator.ShouldEndApplication)
                {
                    if (ExitGameRequested != null)
                        ExitGameRequested(this, EventArgs.Empty); // Se ho un handler specifico, uso quello
                    else
                        Exit(); // Devono ancora fixare il problema dell'uscita da Android
                }
            }
            else if (e.Key == Keys.Back) // L'indietro di Android viene triggerato qui!
            {
                _orchestrator.Back();
                if (_orchestrator.ShouldEndApplication)
                {
                    if (ExitGameRequested != null)
                        ExitGameRequested(this, EventArgs.Empty); // Se ho un handler specifico, uso quello
                    else
                        Exit(); // Devono ancora fixare il problema dell'uscita da Android
                }
            }
            //else
            //{
            //    _orchestrator.HandleKeyboardPressed(e.Key);
            //}
        }

        private void TouchListener_TouchEnded(object sender, TouchEventArgs e)
           => _orchestrator.HandleTouchInput(e.Position);

        private void MouseListener_MouseClicked(object sender, MouseEventArgs e)
            => _orchestrator.HandleTouchInput(e.Position);

        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _splashScreenAssetsLoader = new SplashScreenLoader(
                LoadGameAssets,
                Content,
                "Backgrounds/splashscreen");
            _splashScreenAssetsLoader.Load();

            _splashScreenAssetsLoader.Completed += _splashScreenAssetsLoader_Completed;
        }

        private void _splashScreenAssetsLoader_Completed(object sender, EventArgs e)
        {
            _splashScreenAssetsLoader = null;
            _orchestrator.Start();
        }

        public void Pause()
            => _orchestrator?.Pause();

        public void Resume()
            => _orchestrator?.Resume();

        protected override void Update(GameTime gameTime)
        {
            var elapsed = gameTime.ElapsedGameTime;

            if (_splashScreenAssetsLoader != null)
            {
                _splashScreenAssetsLoader.Update(elapsed);
                return;
            }

            foreach (var listener in _inputListeners)
                listener.Update(gameTime);

            if (_isPc)
            {
                _markKeyboard.Update();
                _orchestrator.HandleKeyboardPressed(_markKeyboard.CurrentInputAction);
            }

            _orchestrator.Update(elapsed);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_splashScreenAssetsLoader != null)
            {
                GraphicsDevice.Clear(Color.Black);
                _spriteBatch.Begin(transformMatrix: _scaleMatrix);
                _splashScreenAssetsLoader.Draw(_spriteBatch);
                _spriteBatch.End();
                return;
            }

            _orchestrator.Draw(_spriteBatch, GraphicsDevice);

            //if (!_orchestrator.IsPaused)
            //{
            //    var mouseState = Mouse.GetState();
            //    var mousePosition = new Vector2(mouseState.X - 32, mouseState.Y);
            //    if (mouseState.X != 0 && mouseState.Y != 0)
            //    {
            //        _spriteBatch.Begin();
            //        _spriteBatch.Draw(_mousePointer.Sheet, mousePosition, _mousePointer.SourceRectangle, Color.White);
            //        _spriteBatch.End();
            //    }
            //}

            base.Draw(gameTime);
        }

    }
}
