using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.StringsLocalization.Abstractions;
using FbonizziMonoGame.TransformationObjects;
using MarkInDeDark.Common;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Lighting;
using MarkInDeDark.Library.Objects;
using MarkInDeDark.Library.Particles.Snowing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Penumbra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace MarkInDeDark.Library
{
    public class MarkInDeDarkGame
    {
        private readonly GameGrid _gameGrid;
        private readonly Exit _exit;
        private readonly Player _player;
        private readonly List<Enemy> _enemies;
        private readonly List<TriggerObject> _triggerObjects;
        private readonly CandlesManager _candlesManager;
        private readonly CollectableObject _key;
        private readonly SnowingManager _snowingManager;

        public bool IsPaused { get; private set; }

        // Tempo per tornare al menù dopo che è tutto finito
        private readonly TimeSpan _gameFinishTimer = TimeSpan.FromSeconds(5);

        // Tempo prima di fare il fade di tutto il livello a parte il player
        private readonly TimeSpan _gameFinishGameOverSceneTimer = TimeSpan.FromSeconds(1);

        // Tempo prima di dire a tutto il gioco "è finita!" (fa anche terminare l'animazione dell'attacco ai mostri)
        private readonly TimeSpan _attackTimer = TimeSpan.FromSeconds(1.5);

        private TimeSpan _elapsedFinish;
        private FadeObject _gameOverFadeObject;

        private readonly GameHUD _gameHud;
        private readonly MarkInDeDarkBootstrap _markInDeDarkBootstrap;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly RenderTarget2D _gameOverBackgroundRenderTarget;
        private readonly RenderTarget2D _gameOverForegroundRenderTarget;

        private readonly RenderTarget2D _groundLevel0RenderTarget;
        private readonly RenderTarget2D _groundLevelNormalMapRenderTarget;
        private readonly RenderTarget2D _groundLevelSpecularMapRenderTarget;
        private readonly RenderTarget2D _groundLevelSpecularMapLightMaskRenderTarget;
        private readonly RenderTarget2D _groundLevelSpecularMapWithAppliedLightmaskRenderTarget;
        private readonly RenderTarget2D _groundLevelAdditionalLightMaskRenderTarget;
        private readonly RenderTarget2D _groundLevelAppliedNormalMapRenderTarget;
        private readonly RenderTarget2D _groundLevelResultRenderTarget;

        private readonly RenderTarget2D _groundLevelRenderTarget;
        private readonly RenderTarget2D _normalMapRenderTarget;
        private readonly RenderTarget2D _baseSceneRenderTarget;
        private readonly RenderTarget2D _baseSceneItemsWithNormalMapRenderTarget;

        private readonly Color _normalMapNeutralColor = new Color(128, 128, 255);

        private readonly Effect _circularNormalMapShader;
        private readonly Effect _specularMapShader;

        private readonly IScreenTransformationMatrixProvider _matrixScaleProvider;
        private readonly GameOrchestrator _gameOrchestrator;
        private readonly SoundManager _soundManager;
        private readonly GameConfig _gameConfig;
        private readonly Level _currentLevel;
        private readonly MarkGamePad _markGamePad;

        private readonly Camera2D _camera;
        private readonly List<CandlePack> _candlePacks;
        private Vector2 _playerCameraOffset;

        private bool? _runGameOverScene;
        private bool _shakeViewport;
        public static LevelCustomization LevelCustomization { get; private set; }
        private Vector3 _normalMapAmbienceColor;

        private Matrix _worldMatrix = Matrix.Identity;
        private Matrix _projectionMatrix = Matrix.Identity;

        public TimeSpan ElapsedGameTime { get; private set; }

        private enum GameStates
        {
            Playing,
            Finishing,
            Finished
        }

        private GameStates _currentGameState;
        private double _shakeStartAngle;
        private double _shakeIntensity;
        private TimeSpan _shakeStart;
        private TimeSpan _shakeDuration;
        private readonly ILocalizedStringsRepository _localizedStringsRepository;
        private readonly PenumbraComponent _penumbra;
        private readonly List<MarkLight> _normalMapLights;

        private readonly Timer _normalMapRegulationTimer;

        public MarkInDeDarkGame(
            GraphicsDevice graphicsDevice,
            GameOrchestrator gameOrchestrator,
            AssetsLoader assetsLoader,
            Level selectedLevel,
            GameConfig gameConfig,
            IScreenTransformationMatrixProvider matrixScaleProvider,
            SoundManager soundManager,
            ILocalizedStringsRepository localizedStringsRepository,
            MarkGamePad markGamePad,
            MarkInDeDarkBootstrap markInDeDarkBootstrap)
        {
            _markInDeDarkBootstrap = markInDeDarkBootstrap;
            _graphicsDevice = graphicsDevice;
            _currentGameState = GameStates.Playing;
            _localizedStringsRepository = localizedStringsRepository;

            _circularNormalMapShader = assetsLoader.CircularNormalMapShader;
            _specularMapShader = assetsLoader.SpecularMapShader;

            _elapsedFinish = TimeSpan.Zero;

            _matrixScaleProvider = matrixScaleProvider;

            _gameOrchestrator = gameOrchestrator;
            _soundManager = soundManager;
            _gameConfig = gameConfig;
            _playerCameraOffset = gameConfig.CameraOffsetFromPlayer.ToXnaVector2();
            _normalMapLights = new List<MarkLight>();

            if (_markInDeDarkBootstrap.NormalMapRegulationMode)
            {
                _normalMapRegulationTimer = new Timer(500);
                _normalMapRegulationTimer.Elapsed += _normalMapRegulationTimer_Elapsed;
                _normalMapRegulationTimer.Start();
            }

            _currentLevel = selectedLevel;
            LevelCustomization = LevelCustomizations.CustomizeForLevel(_currentLevel.LevelEditorDeserializedGrid.EnvironmentTexturePackId);

            var presentationParameters = graphicsDevice.PresentationParameters;
            _gameOverBackgroundRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _gameOverForegroundRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevelRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _baseSceneRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _normalMapRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _baseSceneItemsWithNormalMapRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevel0RenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevelNormalMapRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevelSpecularMapRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevelSpecularMapLightMaskRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevelSpecularMapWithAppliedLightmaskRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevelResultRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevelAppliedNormalMapRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _groundLevelAdditionalLightMaskRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);

            // _gameConfig.GodModeDebugMode = true;

            _penumbra = markInDeDarkBootstrap.CreatePenumbra();
            _penumbra.Initialize();
            _penumbra.Debug = _gameConfig.GodModeDebugMode;

            _markGamePad = markGamePad;

            _camera = new Camera2D(_matrixScaleProvider)
            {
                Zoom = 0.8f
            };

            _gameGrid = new GameGrid(_currentLevel, assetsLoader, _camera, _soundManager, gameConfig, this);
            _candlesManager = new CandlesManager(0, assetsLoader, _soundManager, _gameGrid, gameConfig, _normalMapLights, _penumbra);
            _player = new Player(assetsLoader, _gameGrid, gameConfig, _candlesManager, _soundManager, this);
            _exit = new Exit(_gameGrid.ExitPosition, new Vector2(_gameGrid.CellWidth, _gameGrid.CellHeight), _gameGrid.ExitDoor);
            _key = new CollectableObject(assetsLoader.Sprites["Objects/key"], _gameGrid.KeyPosition, _soundManager);

            _enemies = new List<Enemy>();
            _triggerObjects = new List<TriggerObject>();

            _candlePacks = _gameGrid.CandlePacks;
            ElapsedGameTime = TimeSpan.Zero;
            _gameHud = new GameHUD(assetsLoader, matrixScaleProvider, _candlesManager, selectedLevel.LevelTitleToShow);

            // Se c'è il trigger, il goblin spawna solo quando viene azionato
            if (_gameGrid.GoblinTriggerRectangle != null && _gameGrid.GoblinStartPosition != null)
            {
                _triggerObjects.Add(new GoblinTriggerTrap(
                    assetsLoader: assetsLoader,
                    triggerCollisionRectangle: _gameGrid.GoblinTriggerRectangle.Value,
                    affectedCharacters: new Character[] { _player },
                    onTriggerPressed: (character, elapsed) =>
                    {
                        var newGoblin = new Goblin(
                            assetsLoader,
                            _gameGrid.GoblinStartPosition.Value,
                            _gameGrid,
                            _candlesManager,
                            _soundManager,
                            _player,
                            gameConfig,
                            this);
                        _enemies.Add(newGoblin);
                        _normalMapLights.Add(newGoblin.SurroundingLight);
                        _penumbra.Lights.Add(newGoblin.SurroundingLight.PenumbraLight);
                        //_soundManager.PlayBreakWallSound();
                        StartCameraShake(4);

                        // Aggiungo il goblin al controllo delle trappole
                        foreach (var trap in _triggerObjects.Where(t => t is InstantlyDieTrap || t is TimedBreakableTrap))
                        {
                            trap.AddAffectedCharacter(newGoblin);
                        }
                    }));
            }
            // Se non metto il trigger, lo spawno subito
            else if (_gameGrid.GoblinStartPosition != null)
            {
                _enemies.Add(new Goblin(assetsLoader, _gameGrid.GoblinStartPosition.Value, _gameGrid, _candlesManager, _soundManager, _player, gameConfig, this));
            }

            if (_gameGrid.ZombiesPath != null)
            {
                foreach (var zombiePath in _gameGrid.ZombiesPath)
                {
                    _enemies.Add(new Zombie(assetsLoader, zombiePath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig, this));
                }
            }

            if (_gameGrid.GhostsPath != null)
            {
                foreach (var ghostPath in _gameGrid.GhostsPath)
                {
                    _enemies.Add(new Ghost(assetsLoader, ghostPath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig, this));
                }
            }

            if (_gameGrid.BubusPath != null)
            {
                foreach (var bubuPath in _gameGrid.BubusPath)
                {
                    _enemies.Add(new Bubu(assetsLoader, bubuPath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig, this));
                }
            }

            if (_gameGrid.SkeletronsPath != null)
            {
                foreach (var skeletronPath in _gameGrid.SkeletronsPath)
                {
                    _enemies.Add(new Skeletron(assetsLoader, skeletronPath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig, this));
                }
            }

            if (_gameGrid.TaurusPath != null)
            {
                foreach (var taurusPath in _gameGrid.TaurusPath)
                {
                    _enemies.Add(new Taurus(assetsLoader, taurusPath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig, _enemies, this));
                }
            }

            if (_gameGrid.InstantlyDieTrapCollisionRectangles != null)
            {
                foreach (var instantlyDieTrapRectangle in _gameGrid.InstantlyDieTrapCollisionRectangles)
                {
                    _triggerObjects.Add(new InstantlyDieTrap(
                        assetsLoader,
                        (new List<Character> { _player }).Concat(_enemies.Where(e => !(e is Ghost))).ToList(),
                        instantlyDieTrapRectangle));
                }
            }

            if (_gameGrid.BrekableTrapCollisionRectangles != null)
            {
                foreach (var breakableTrapRectangle in _gameGrid.BrekableTrapCollisionRectangles)
                {
                    _triggerObjects.Add(new TimedBreakableTrap(
                        assetsLoader,
                        _gameConfig,
                        breakableTrapRectangle,
                        (new List<Character> { _player }).Concat(_enemies.Where(e => !(e is Ghost))).ToList(),
                        () => StartCameraShake(1)));
                }
            }

            if (_gameGrid.CellTriggerText != null)
            {
                foreach (var cellTriggerText in _gameGrid.CellTriggerText)
                {
                    _triggerObjects.Add(new ShowTextTriggerObject(
                        _localizedStringsRepository.Get(cellTriggerText.Value),
                        _gameHud,
                        cellTriggerText.Key,
                        _player));
                }
            }

            _projectionMatrix = GetProjectionMatrixForNormalMap();

            _normalMapLights.Add(_player.SurroundingLight);
            foreach (var e in _enemies)
            {
                if (e.SurroundingLight != null)
                    _normalMapLights.Add(e.SurroundingLight);
            }

            foreach (var l in _gameGrid.BraziersLit)
                _normalMapLights.Add(l.SurroundingLight);

            // Penumbra
            foreach (var normalMapLight in _normalMapLights)
                _penumbra.Lights.Add(normalMapLight.PenumbraLight);

            foreach (var w in _gameGrid.AllWalls)
            {
                _penumbra.Hulls.AddRange(w.Hulls);
            }

            foreach (var p in _gameGrid.AllPillars)
            {
                _penumbra.Hulls.Add(p.Hull);
            }

            _normalMapAmbienceColor = LevelCustomization.AmbienceColor;
            _penumbra.AmbientColor = LevelCustomization.PenumbraAmbientColor;
            if (LevelCustomization.IsSnowing)
            {
                _snowingManager = new SnowingManager(assetsLoader.Sprites["Effects/snowflake"], _camera);
            }
        }

        // TODO Levare questa robba quando avremo finito?
        private void _normalMapRegulationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (File.Exists(NormalMapParameters.FileName))
            {
                var normalMapParametersTemp = JsonConvert.DeserializeObject<NormalMapParameters>(
                    File.ReadAllText(NormalMapParameters.FileName));

                if (normalMapParametersTemp.EditAmbienceLights)
                {
                    _normalMapAmbienceColor = normalMapParametersTemp.AmbienceColor;
                    _penumbra.AmbientColor = normalMapParametersTemp.PenumbraAmbienceColor;
                }

                if (normalMapParametersTemp.EditPlayerLights)
                {
                    _player.SurroundingLight.Z = normalMapParametersTemp.PlayerLightDirectionZ;
                    _player.SurroundingLight.DistanceSquaredForGround = normalMapParametersTemp.PlayerLightDistance;
                    _player.SurroundingLight.Opacity = normalMapParametersTemp.PlayerLightOpacity;
                    _player.SurroundingLight.ColorNormal = normalMapParametersTemp.PlayerLightColor;
                }

                try
                {
                    File.Delete(NormalMapParameters.FileName);
                }
                catch
                {

                }
            }
        }

        public void HandleInput(CharacterActions action)
        {
            if (action == CharacterActions.Candle && _candlesManager.LeftCandles == 0)
            {
                _gameHud.ShowMessage(
                    _localizedStringsRepository.Get("NotEnoughCandlesString"),
                    _gameConfig.NotEnoughCandlesStringColorRGB);
            }

            _player.HandleInput(action);
        }

        private void RepositionCamera()
        {
            var newCameraPosition = _player.Position - _playerCameraOffset;
            _camera.Position = new Vector2(
                MathHelper.Lerp(_camera.Position.X, newCameraPosition.X, 0.1f),
                MathHelper.Lerp(_camera.Position.Y, newCameraPosition.Y, 0.1f));
        }

        public void Update(TimeSpan elapsed)
        {
            if (IsPaused)
                return;

            switch (_currentGameState)
            {
                case GameStates.Playing:
                    ElapsedGameTime += elapsed;

                    RepositionCamera();

                    _snowingManager?.Update(elapsed);
                    if (_player.IsDying)
                    {
                        _gameHud.ShowMessage(
                             string.Format(_localizedStringsRepository.Get("GameOverString")),
                             _gameConfig.GameOverStringColorRGB,
                             true);

                        _markGamePad.Shake(6, TimeSpan.FromSeconds(2));
                        _currentGameState = GameStates.Finishing;
                    }

                    if (!_key.IsCollected)
                    {
                        if (_player.CollisionRectangle.Intersects(_key.CollisionRectangle))
                        {
                            _key.Collect();
                            _gameHud.ShowMessage(
                                string.Format(_localizedStringsRepository.Get("KeyFoundString")),
                                _gameConfig.KeyFoundStringColorRGB);
                            _exit.SetOpen();
                            _gameHud.DrawKey = true;
                        }
                    }

                    if (_player.CollisionRectangle.Intersects(_exit.CollisionRectangle))
                    {
                        if (_key.IsCollected)
                        {
                            _player.Win(_exit);
                            _gameHud.ShowMessage(
                               string.Format(_localizedStringsRepository.Get("LevelCompletedString")),
                               _gameConfig.LevelCompletedStringColorRGB,
                               true);
                            _currentGameState = GameStates.Finishing;
                        }
                        else
                        {
                            _gameHud.ShowMessage(
                                string.Format(_localizedStringsRepository.Get("ClosedDoorString")),
                                _gameConfig.ClosedDoorStringColorRGB);
                        }
                    }

                    for (int c = _candlePacks.Count - 1; c >= 0; --c)
                    {
                        var candlePack = _candlePacks[c];
                        if (candlePack.IsActive)
                        {
                            candlePack.Update(elapsed);
                            if (!candlePack.IsCollected && _player.CollisionRectangle.Intersects(candlePack.CollisionRectangle))
                            {
                                candlePack.Collect();
                                _candlesManager.AddCandlesToInventory(candlePack.NCandlesInThisPack);
                                _gameHud.ShowMessage(
                                    string.Format(_localizedStringsRepository.Get("CandlePackFoundString"), candlePack.NCandlesInThisPack),
                                    _gameConfig.CandlePackFoundStringColorRGB);
                            }
                        }
                        else
                        {
                            _candlePacks.RemoveAt(c);
                        }
                    }

                    break;

                case GameStates.Finishing:

                    _elapsedFinish += elapsed;

                    if (_elapsedFinish >= _gameFinishGameOverSceneTimer)
                    {
                        if (_runGameOverScene == null)
                        {
                            _runGameOverScene = !_player.HasWon;
                            if (_runGameOverScene == true)
                            {
                                _gameOverFadeObject = new FadeObject(TimeSpan.FromSeconds(1.5), Color.White);
                                _gameOverFadeObject.FadeOut();
                            }
                        }
                    }

                    if (_gameOverFadeObject?.IsFading == true)
                    {
                        _gameOverFadeObject.Update(elapsed);
                    }

                    if (_elapsedFinish >= _gameFinishTimer)
                    {
                        if (_player.HasWon)
                        {
                            _currentLevel.SetCompletionParameters(true);
                        }

                        _currentGameState = GameStates.Finished;
                    }

                    if (_elapsedFinish >= _attackTimer)
                    {
                        foreach (var e in _enemies)
                            e.GameFinished();
                    }


                    break;

                case GameStates.Finished:
                    _gameOrchestrator.SetAfterGameState();
                    return;
            }


            // Effetto shake della viewport
            if (_shakeViewport)
            {
                _shakeStart += elapsed;
                var offset = new Vector2((float)(Math.Sin(_shakeStartAngle) * _shakeIntensity), (float)(Math.Cos(_shakeStartAngle) * _shakeIntensity));
                _shakeIntensity -= 0.25f;
                _shakeStartAngle += (150 + Numbers.RandomBetween(0, 60));
                if (elapsed.TotalSeconds - _shakeStart.TotalSeconds > _shakeDuration.TotalSeconds || _shakeIntensity <= 0)
                {
                    _shakeViewport = false;
                }

                _camera.Position += offset;
            }

            if (_triggerObjects != null)
            {
                foreach (var triggerObject in _triggerObjects)
                {
                    triggerObject.Update(elapsed);
                }
            }

            foreach (var brazierLit in _gameGrid.BraziersLit)
            {
                brazierLit.Update(elapsed);
            }

            _gameGrid.Update(elapsed);
            _player.Update(elapsed);
            _candlesManager.Update(elapsed);
            _key.Update(elapsed);
            _gameHud.Update(elapsed);
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsDead)
                    enemy.Update(elapsed);
                else
                    enemy.UpdateAfterDeath(elapsed);
            }

            var viewMatrix = _camera.GetViewMatrix();
            _penumbra.Transform = viewMatrix;

            foreach (var gameLight in _normalMapLights)
            {
                gameLight.IsOutOfScreen = _camera.Contains(gameLight.BoundingRectangle) == ContainmentType.Disjoint;

                if (gameLight.ShouldDraw)
                {
                    gameLight.PositionForWall = new Vector3(
                        WorldToScreenForLights(gameLight.Position + gameLight.DeltaForWall, viewMatrix),
                        gameLight.Z);
                    gameLight.PositionForGround = new Vector3(
                        WorldToScreenForLights(gameLight.Position + gameLight.DeltaForGround, viewMatrix),
                        gameLight.Z);
                }
            }

            //var onScreen = _normalMapLights.Count(c => !c.IsOutOfScreen);
            //var toDraw = _normalMapLights.Count(c => c.ShouldDraw);
            //var enableds = _normalMapLights.Count(c => c.IsEnabled);
            //Console.WriteLine($"onScreen: {onScreen} - enabled: {enableds} - toDraw: {toDraw}");
        }

        public Vector2 WorldToScreenForLights(Vector2 worldPosition, Matrix viewMatrix)
        {
            return Vector2.Transform(worldPosition, viewMatrix);
        }

        public void OnLeavingPage()
        {
            _markInDeDarkBootstrap.DisablePenumbra(_penumbra);
        }

        public void Pause()
        {
            _soundManager.PauseAll();
            IsPaused = true;
        }

        public void Resume()
        {
            _soundManager.ResumeAll();
            IsPaused = false;
        }

        public void StopMusic()
            => _soundManager.StopAll();

        /// <summary>
        /// Scala da 1 a 15
        /// </summary>
        /// <param name="intensity"></param>
        public void StartCameraShake(int intensity, TimeSpan? duration = null)
        {
            if (duration == null)
                _shakeDuration = TimeSpan.FromSeconds(2f);
            else
                _shakeDuration = duration.Value;

            _shakeIntensity = intensity;
            _shakeStartAngle = Math.PI;
            _shakeStart = TimeSpan.Zero;
            _shakeViewport = true;

            _markGamePad.Shake(intensity, _shakeDuration);
        }

        public void Draw(
            SpriteBatch spriteBatch,
            RenderTarget2D virtualDrawTarget,
            GameTime gameTime)
        {
            if (_runGameOverScene == true)
            {
                DrawPlayingScene(spriteBatch, _gameOverBackgroundRenderTarget, gameTime);

                var viewMatrix = _camera.GetViewMatrix();

                _graphicsDevice.SetRenderTarget(_gameOverForegroundRenderTarget);
                _graphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);
                _player.Draw(spriteBatch);
                spriteBatch.End();

                _graphicsDevice.SetRenderTarget(virtualDrawTarget);
                _graphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();
                spriteBatch.Draw(_gameOverBackgroundRenderTarget, Vector2.Zero, _gameOverFadeObject.OverlayColor);
                spriteBatch.Draw(_gameOverForegroundRenderTarget, Vector2.Zero, Color.White.WithAlpha(1 - _gameOverFadeObject.CurrentAlpha));
                spriteBatch.End();

                return;
            }
            else
            {
                DrawPlayingScene(spriteBatch, virtualDrawTarget, gameTime);
            }
        }

        private Matrix GetProjectionMatrixForNormalMap()
        {
            // TODO Correggerlo anche nella libreria FbonizziMonoGame
            var projection = Matrix.CreateOrthographicOffCenter(0, _matrixScaleProvider.RealScreenWidth,
                _matrixScaleProvider.RealScreenHeight, 0, -1, 0);
            Matrix.Multiply(ref _worldMatrix, ref projection, out projection);
            return projection;
        }

        public void DrawPlayingScene(
            SpriteBatch spriteBatch,
            RenderTarget2D finalRenderTarget,
            GameTime gameTime)
        {
            var viewMatrix = _camera.GetViewMatrix();
            var projectionMatrix = _projectionMatrix;

            // 0. Preparo l'effetto NormalMap
            _circularNormalMapShader.Parameters["AmbientColor"].SetValue(_normalMapAmbienceColor);
            _circularNormalMapShader.Parameters["World"].SetValue(Matrix.Identity);
            _circularNormalMapShader.Parameters["ViewProjection"].SetValue(projectionMatrix);
            _circularNormalMapShader.CurrentTechnique.Passes[0].Apply();

            // 1. Disegno il pavimento con già la normal applicata e con le ombre
            _gameGrid.DrawGroundWithNormalMap(
                spriteBatch: spriteBatch,
                viewMatrix: viewMatrix,
                normalMapEffect: _circularNormalMapShader,
                specularMapEffect: _specularMapShader,
                graphicsDevice: _graphicsDevice,
                baseRenderTarget: _groundLevel0RenderTarget,
                normalMapRenderTarget: _groundLevelNormalMapRenderTarget,
                specularMapRenderTarget: _groundLevelSpecularMapRenderTarget,
                specularMapLightmaskRenderTarget: _groundLevelSpecularMapLightMaskRenderTarget,
                specularMapWithAppliedLightmaskRenderTarget: _groundLevelSpecularMapWithAppliedLightmaskRenderTarget,
                resultRenderTarget: _groundLevelResultRenderTarget,
                appliedNormalMapRenderTarget: _groundLevelAppliedNormalMapRenderTarget,
                drawGlowsForSpecular: s =>
                {
                    _player.SurroundingLight?.DrawGlowForSpecular(s);
                    foreach (var e in _enemies)
                        e.SurroundingLight?.DrawGlowForSpecular(s);
                    foreach (var l in _gameGrid.BraziersLit)
                        l.SurroundingLight?.DrawGlowForSpecular(s);
                    foreach (var c in _candlesManager.Candles)
                        c.SurroundingLight?.DrawGlowForSpecular(s);
                },
                drawOtherGroundObjects: s =>
                {
                    if (_triggerObjects != null)
                    {
                        foreach (var triggerObject in _triggerObjects)
                            triggerObject.Draw(spriteBatch);
                    }

                    _player.DrawShadows(s);

                    foreach (Enemy en in _enemies)
                        en.DrawShadows(s);

                    foreach (var brazierLit in _gameGrid.BraziersLit)
                        brazierLit.DrawShadows(spriteBatch);
                },
                lights: _normalMapLights);

            _graphicsDevice.SetRenderTarget(_groundLevelAdditionalLightMaskRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
            foreach (var e in _enemies)
                e.SurroundingLight?.DrawLightMask(spriteBatch);
            foreach (var l in _gameGrid.BraziersLit)
                l.SurroundingLight?.DrawLightMask(spriteBatch);
            foreach (var c in _candlesManager.Candles)
                c.SurroundingLight?.DrawLightMask(spriteBatch);
            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(_groundLevelRenderTarget);
            _penumbra.BeginDraw();
            _graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            spriteBatch.Draw(_groundLevelResultRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
            _penumbra.Draw(gameTime);

            // Disegno altri glow sopra alla lightmap di penumbra
            _graphicsDevice.SetRenderTarget(_penumbra.LightMap);
            spriteBatch.Begin(blendState: BlendState.Additive);
            spriteBatch.Draw(_groundLevelAdditionalLightMaskRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
            _penumbra.BlendAll();

            // 2. Disegno la normal map di muri e pilastri
            _graphicsDevice.SetRenderTarget(_normalMapRenderTarget);
            _graphicsDevice.Clear(_normalMapNeutralColor);
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);
            _gameGrid.DrawAllNormalMapsButGround(spriteBatch);
            _player.DrawNormalMap(spriteBatch);
            foreach (var e in _enemies)
                e.DrawNormalMap(spriteBatch);
            foreach (var littleLight in _gameGrid.BraziersLit)
                littleLight.DrawNormalMap(spriteBatch);
            spriteBatch.End();

            // 2. Disegno il resto
            _graphicsDevice.SetRenderTarget(_baseSceneRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(
                transformMatrix: viewMatrix,
                samplerState: SamplerState.PointClamp,
                sortMode: SpriteSortMode.FrontToBack);

            if (!_gameConfig.GodModeDebugMode)
                _gameGrid.DrawWalls(spriteBatch);
            else _gameGrid.DrawForDebug(spriteBatch);

            _key.Draw(spriteBatch);

            foreach (var candlePack in _candlePacks)
                candlePack.Draw(spriteBatch);

            foreach (var otherObject in _gameGrid.DrawableObjects)
                otherObject.Draw(spriteBatch);

            foreach (var brazierLit in _gameGrid.BraziersLit)
                brazierLit.Draw(spriteBatch);

            _candlesManager.Draw(spriteBatch);

            _player.Draw(spriteBatch);

            foreach (Enemy en in _enemies)
                en.Draw(spriteBatch);

            _gameGrid.DrawRiempitiviGround(spriteBatch);

            spriteBatch.End();

            // 3. Applico la normal map generica
            if (!_gameConfig.GodModeDebugMode)
            {
                _graphicsDevice.SetRenderTarget(_baseSceneItemsWithNormalMapRenderTarget);
                _graphicsDevice.Clear(Color.Transparent);
                _circularNormalMapShader.Parameters["NormalTexture"].SetValue(_normalMapRenderTarget);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, null, null, _circularNormalMapShader);
                foreach (var light in _normalMapLights)
                {
                    if (!light.ShouldDraw)
                        continue;

                    _circularNormalMapShader.Parameters["LightPosition"].SetValue(light.PositionForWall);
                    _circularNormalMapShader.Parameters["LightColor"].SetValue(light.ColorNormal);
                    _circularNormalMapShader.Parameters["LightOpacity"].SetValue(light.Opacity);
                    _circularNormalMapShader.Parameters["LightDistanceSquared"].SetValue(light.DistanceSquaredForWall);
                    spriteBatch.Draw(_baseSceneRenderTarget, Vector2.Zero, Color.White);
                }
                spriteBatch.End();
            }

            // 4. Ricomposizione finale di pavimento ed il resto
            _graphicsDevice.SetRenderTarget(finalRenderTarget);
            _graphicsDevice.Clear(Color.Black);

            if (!_gameConfig.GodModeDebugMode)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
                spriteBatch.Draw(_groundLevelRenderTarget, Vector2.Zero, Color.White);
                spriteBatch.Draw(_baseSceneItemsWithNormalMapRenderTarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.Draw(_groundLevelRenderTarget, Vector2.Zero, Color.White);
                spriteBatch.Draw(_baseSceneRenderTarget, Vector2.Zero, Color.White.WithAlpha(0.2f));
                spriteBatch.End();
            }

            // 5. Effetti sopra la lightmask
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);
            foreach (Enemy en in _enemies)
                en.DrawEffectsOverDark(spriteBatch);
            _candlesManager.DrawEffectsOverDark(spriteBatch);
            _snowingManager?.Draw(spriteBatch);
            spriteBatch.End();

            // 6. HUD
            spriteBatch.Begin();
            _gameHud.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}

