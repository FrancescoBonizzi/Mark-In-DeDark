using FbonizziMonoGame.Assets;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Input;
using FbonizziMonoGame.Input.Abstractions;
using FbonizziMonoGame.PlatformAbstractions;
using FbonizziMonoGame.StringsLocalization;
using FbonizziMonoGame.StringsLocalization.Abstractions;
using MarkInDeDark.Common;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Penumbra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace MarkInDeDark.Library
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

        private readonly ILocalizedStringsRepository _localizedStringsRepository;

        private GameOrchestrator _orchestrator;
        private AssetsLoader _assetsLoader;
        private IScreenTransformationMatrixProvider _matrixScaleProvider;

        private readonly IEnumerable<string> _levelsNames;

        // private Sprite _mousePointer;
        private string _forcedLevelPath;

        private SoundManager _soundManager;

        private MarkKeyboard _markKeyboard;
        private MarkGamePad _markGamePad;

        public bool NormalMapRegulationMode { get; set; }

        public MarkInDeDarkBootstrap(
            ITextFileLoader textFileAssetsLoader,
            ISettingsRepository settingsRepository,
            CultureInfo gameCulture,
            IEnumerable<string> levelPaths,
            int windowWidth,
            int windowHeight)
        {
            IsMouseVisible = true;

            _textFileAssetsLoader = textFileAssetsLoader;
            _settingsRepository = settingsRepository;
            _levelsNames = levelPaths;
            _gameCulture = gameCulture;
            _gameConfig = GameConfigSerializerV2.Deserialize(textFileAssetsLoader.LoadFile(@"Assets/GameConfig.json"));

            GraphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight,
                IsFullScreen = _gameConfig.IsFullScreen
                //SynchronizeWithVerticalRetrace = false
            };

            _localizedStringsRepository = new InMemoryLocalizedStringsRepository(new Dictionary<string, string>());

            if (GraphicsDeviceManager.IsFullScreen)
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = 1920;
                GraphicsDeviceManager.PreferredBackBufferHeight = 1080;
            }
            else
            {
                GraphicsDeviceManager.PreferredBackBufferWidth = windowWidth;
                GraphicsDeviceManager.PreferredBackBufferHeight = windowHeight;
            }
        }

        public PenumbraComponent CreatePenumbra()
        {
            var penumbra = new PenumbraComponent(this);
            Services.AddService(penumbra);
            return penumbra;
        }

        public void DisablePenumbra(PenumbraComponent penumbra)
        {
            Services.RemoveService(typeof(PenumbraComponent));
            penumbra.Dispose();
        }

        public void ForceStartLevel(string levelFilePath)
        {
            _forcedLevelPath = levelFilePath;
        }

        protected override void Initialize()
        {
            _matrixScaleProvider = new DynamicScalingMatrixProvider(
                window: new GameWindowScreenSizeChangedNotifier(Window),
                graphicsDevice: GraphicsDeviceManager.GraphicsDevice,
                virtualWidth: 1920, virtualHeight: 1080,
                mantainProportions: true);

            base.Initialize();
        }

        private void LoadGameAssets()
        {
            new GameStringsLoader(
                _localizedStringsRepository,
                _gameCulture,
                new TextFileStringsManager(_textFileAssetsLoader.LoadFile(@"Assets/Translations.json")));

            _inputListeners = new List<IInputListener>();

            // Mouse
            var mouseListener = new MouseListener(_matrixScaleProvider);
            mouseListener.MouseDown += MouseListener_MouseClicked;
            _inputListeners.Add(mouseListener);

            // Tastiera
            _markKeyboard = new MarkKeyboard();

            // Gamepad
            _markGamePad = new MarkGamePad();

            _assetsLoader = new AssetsLoader(
                Content,
                _textFileAssetsLoader,
                _levelsNames,
                _settingsRepository,
                _gameConfig);
            _soundManager = new SoundManager(_assetsLoader);

            // _mousePointer = AssetsLoader.Sprites["manina"];

            var gameFactory = new Func<Level, MarkInDeDarkGame>(
                level => new MarkInDeDarkGame(
                    GraphicsDevice,
                    _orchestrator,
                    _assetsLoader,
                    level,
                    _gameConfig,
                    _matrixScaleProvider,
                    _soundManager,
                    _localizedStringsRepository,
                    _markGamePad,
                    this));

            var menuFactory = new Func<MainMenuPage>(
                () => new MainMenuPage(
                    _assetsLoader,
                    _soundManager,
                    _matrixScaleProvider,
                    _localizedStringsRepository));

            var levelsPageFactory = new Func<LevelsPage>(
                () => new LevelsPage(
                    _matrixScaleProvider,
                    _localizedStringsRepository,
                    _assetsLoader));

            _orchestrator = new GameOrchestrator(
                gameFactory,
                menuFactory,
                levelsPageFactory,
                _spriteBatch.GraphicsDevice,
                _matrixScaleProvider,
                _localizedStringsRepository,
                _assetsLoader.Font,
                _markGamePad);
        }

        public void Back()
        {
            _orchestrator.Back();
            if (_orchestrator.ShouldEndApplication)
            {
                Exit();
            }
        }

        private void MouseListener_MouseClicked(object sender, MouseEventArgs e)
            => _orchestrator.HandleMouseInput(e.Position);

        protected override void LoadContent()
        {
            Content.RootDirectory = "Assets";
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _splashScreenAssetsLoader = new SplashScreenLoader(
                LoadGameAssets,
                Content,
                "splashscreen")
            {
                MininumSplashScreenDuration = TimeSpan.FromSeconds(2)
            };
            _splashScreenAssetsLoader.Load();

            _splashScreenAssetsLoader.Completed += SplashScreenAssetsLoader_Completed;
        }

        private void SplashScreenAssetsLoader_Completed(object sender, EventArgs e)
        {
            _splashScreenAssetsLoader = null;
            if (_forcedLevelPath != null)
            {
                _assetsLoader.ForceLoadLevel(_forcedLevelPath);
                _orchestrator.StartWithForcedLevel(_assetsLoader.Levels[0]);
            }
            else
            {
                _orchestrator.Start();
            }
        }

        public void Pause()
        {
            if (NormalMapRegulationMode)
                return;

            _orchestrator?.Pause();
        }

        public void Resume()
        {
            if (NormalMapRegulationMode)
                return;

            _orchestrator?.Resume();
        }

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

            if (_markGamePad.IsConnected)
            {
                _markGamePad.Update(elapsed);
                var currentInput = _markGamePad.CurrentInputAction;
                if (currentInput == Characters.CharacterActions.GameBack)
                    Back();
                else
                    _orchestrator.HandleKeyboardPressed(currentInput);
            }
            else
            {
                _markKeyboard.Update();
                var currentInput = _markKeyboard.CurrentInputAction;
                if (currentInput == Characters.CharacterActions.GameBack)
                    Back();
                else
                    _orchestrator.HandleKeyboardPressed(currentInput);
            }

            _orchestrator.Update(elapsed);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_splashScreenAssetsLoader != null)
            {
                GraphicsDevice.Clear(Color.Black);
                _spriteBatch.Begin(transformMatrix: _matrixScaleProvider.ScaleMatrix);
                _splashScreenAssetsLoader.Draw(_spriteBatch);
                _spriteBatch.End();
                return;
            }

            _orchestrator.Draw(_spriteBatch, GraphicsDevice, gameTime);

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
