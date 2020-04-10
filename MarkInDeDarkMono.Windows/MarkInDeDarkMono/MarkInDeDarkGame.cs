using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.StringsLocalization.Abstractions;
using FbonizziMonoGame.TransformationObjects;

using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Extensions;
using MarkInDeDarkMono.Levels;
using MarkInDeDarkMono.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono
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

        private readonly Sprite _playerStartingPointTexture;
        private readonly DrawingInfos _playerStartingPointDrawingInfos;

        public bool IsPaused { get; private set; }

        private readonly TimeSpan _gameFinishTimer = TimeSpan.FromSeconds(4);
        private readonly TimeSpan _gameFinishGameOverSceneTimer = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _attackTimer = TimeSpan.FromSeconds(1);
        private TimeSpan _elapsedFinish;
        private FadeObject _gameOverFadeObject;

        private readonly GameHUD _gameHud;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly RenderTarget2D _lightMaskRenderTarget;
        private readonly RenderTarget2D _gameOverBackgroundRenderTarget;
        private readonly RenderTarget2D _gameOverForegroundRenderTarget;
        private readonly BlendState _blendStateMultiply;
        private readonly IScreenTransformationMatrixProvider _matrixScaleProvider;
        private readonly GameOrchestrator _gameOrchestrator;
        private readonly SoundManager _soundManager;
        private readonly GameConfig _gameConfig;
        private readonly Level _currentLevel;
        private readonly Color _backgroundColorForLightMask;

        private readonly Camera2D _camera;
        private readonly List<CandlePack> _candlePacks;
        private Vector2 _playerCameraOffset;

        private bool? _runGameOverScene;
        private bool _shakeViewport;

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
        private readonly ILocalizedStringsRepository _localizedStringsRepository;

        public MarkInDeDarkGame(
            GraphicsDevice graphicsDevice,
            GameOrchestrator gameOrchestrator,
            AssetsLoader assetsLoader,
            Level selectedLevel,
            GameConfig gameConfig,
            IScreenTransformationMatrixProvider matrixScaleProvider,
            SoundManager soundManager,
            ILocalizedStringsRepository localizedStringsRepository)
        {
            _graphicsDevice = graphicsDevice;
            _currentGameState = GameStates.Playing;
            _localizedStringsRepository = localizedStringsRepository;

            _elapsedFinish = TimeSpan.Zero;

            _matrixScaleProvider = matrixScaleProvider;

            _gameOrchestrator = gameOrchestrator;
            _soundManager = soundManager;
            _gameConfig = gameConfig;
            _playerCameraOffset = gameConfig.CameraOffsetFromPlayer.ToXnaVector2();

            var presentationParameters = graphicsDevice.PresentationParameters;
            _lightMaskRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _gameOverBackgroundRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _gameOverForegroundRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);

            if (gameConfig.GodModePlayerSeesEverything)
                _backgroundColorForLightMask = Color.White;
            else _backgroundColorForLightMask = Color.Black;

            _blendStateMultiply = new BlendState()
            {
                AlphaSourceBlend = Blend.DestinationAlpha,
                AlphaDestinationBlend = Blend.Zero,
                AlphaBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add
            };

            _currentLevel = selectedLevel;

            _camera = new Camera2D(_matrixScaleProvider)
            {
                Zoom = 0.8f
            };
            _gameGrid = new GameGrid(_currentLevel, assetsLoader, _camera, _soundManager, gameConfig, this);
            _candlesManager = new CandlesManager(0, assetsLoader, _soundManager, _gameGrid, gameConfig);
            _player = new Player(assetsLoader, _gameGrid, gameConfig, _candlesManager, _soundManager);

            _exit = new Exit(assetsLoader, gameConfig, _gameGrid.ExitPosition);
            _key = new CollectableObject(assetsLoader.Sprites["key"], _gameGrid.KeyPosition, _soundManager);
            _playerStartingPointTexture = assetsLoader.Sprites["stairs"];
            _playerStartingPointDrawingInfos = new DrawingInfos()
            {
                Position = _gameGrid.PlayerStartingPosition,
                Origin = _playerStartingPointTexture.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForGroundObjects
            };

            _enemies = new List<Enemy>();
            _triggerObjects = new List<TriggerObject>();

            _candlePacks = _gameGrid.CandlePacks;
            ElapsedGameTime = TimeSpan.Zero;
            _gameHud = new GameHUD(assetsLoader, matrixScaleProvider, _candlesManager);

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
                            gameConfig);
                        _enemies.Add(newGoblin);
                        _soundManager.PlayBreakWallSound();
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
                _enemies.Add(new Goblin(assetsLoader, _gameGrid.GoblinStartPosition.Value, _gameGrid, _candlesManager, _soundManager, _player, gameConfig));
            }

            if (_gameGrid.WerewolfStartingPosition != null)
            {
                _enemies.Add(new Werewolf(assetsLoader, _gameGrid.WerewolfStartingPosition.Value, _gameGrid, _candlesManager, _soundManager, _player, gameConfig));
            }

            if (_gameGrid.ZombiesPath != null)
            {
                foreach (var zombiePath in _gameGrid.ZombiesPath)
                {
                    _enemies.Add(new Zombie(assetsLoader, zombiePath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig));
                }
            }

            if (_gameGrid.GhostsPath != null)
            {
                foreach (var ghostPath in _gameGrid.GhostsPath)
                {
                    _enemies.Add(new Ghost(assetsLoader, ghostPath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig));
                }
            }

            if (_gameGrid.BubusPath != null)
            {
                foreach (var bubuPath in _gameGrid.BubusPath)
                {
                    _enemies.Add(new Bubu(assetsLoader, bubuPath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig));
                }
            }

            if (_gameGrid.SkeletronsPath != null)
            {
                foreach (var skeletronPath in _gameGrid.SkeletronsPath)
                {
                    _enemies.Add(new Skeletron(assetsLoader, skeletronPath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig));
                }
            }

            if (_gameGrid.TaurusPath != null)
            {
                foreach (var taurusPath in _gameGrid.TaurusPath)
                {
                    _enemies.Add(new Taurus(assetsLoader, taurusPath, _gameGrid, _candlesManager, _soundManager, _player, gameConfig, _enemies));
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

                    // Effetto shake della viewport
                    if (_shakeViewport)
                    {
                        _shakeStart += elapsed;
                        var offset = new Vector2((float)(Math.Sin(_shakeStartAngle) * _shakeIntensity), (float)(Math.Cos(_shakeStartAngle) * _shakeIntensity));
                        _shakeIntensity -= 0.25f;
                        _shakeStartAngle += (150 + Numbers.RandomBetween(0, 60));
                        if (elapsed.TotalSeconds - _shakeStart.TotalSeconds > 2 || _shakeIntensity <= 0)
                        {
                            _shakeViewport = false;
                        }

                        _camera.Position += offset;
                    }

                    if (_player.IsDying)
                    {
                        _gameHud.ShowMessage(
                             string.Format(_localizedStringsRepository.Get("GameOverString")),
                             _gameConfig.GameOverStringColorRGB,
                             true);

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

                    if (_triggerObjects != null)
                    {
                        foreach (var triggerObject in _triggerObjects)
                        {
                            triggerObject.Update(elapsed);
                        }
                    }

                    foreach (var littleLight in _gameGrid.LittleLights)
                    {
                        littleLight.Update(elapsed);
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
                                _gameOverFadeObject = new FadeObject(TimeSpan.FromSeconds(2), Color.White);
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
                            _currentLevel.SetCompletionParameters(
                                true,
                                _candlesManager.PlayerUsedCandles,
                                _gameHud.GameElapsed);
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

            _gameGrid.Update(elapsed);
            _player.Update(elapsed);
            _candlesManager.Update(elapsed);
            _key.Update(elapsed);
            _exit.Update(elapsed);
            _gameHud.Update(elapsed);
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsDead)
                {
                    enemy.Update(elapsed);
                }
            }
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
            => _soundManager.StopSounds();

        public void StartCameraShake(int intensity)
        {
            if (_shakeViewport)
                return;

            _shakeIntensity = intensity;
            _shakeStartAngle = Math.PI;
            _shakeStart = TimeSpan.Zero;
            _shakeViewport = true;
        }

        private void DrawLightMap(
            SpriteBatch spriteBatch,
            RenderTarget2D virtualDrawTarget,
            Matrix viewMatrix)
        {
            _graphicsDevice.SetRenderTarget(_lightMaskRenderTarget);
            _graphicsDevice.Clear(_backgroundColorForLightMask);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, transformMatrix: viewMatrix);

            _player.DrawLightMap(spriteBatch);

            foreach (var enemy in _enemies)
            {
                enemy.DrawLightMap(spriteBatch);
            }

            _exit.LightMap.Draw(spriteBatch);
            foreach (var littleLight in _gameGrid.LittleLights)
            {
                littleLight.LightMap.Draw(spriteBatch);
            }
            _candlesManager.DrawLightmap(spriteBatch);


            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(virtualDrawTarget);
        }

        public void Draw(
            SpriteBatch spriteBatch,
            RenderTarget2D virtualDrawTarget)
        {
            if (_runGameOverScene == true)
            {
                DrawPlayingScene(spriteBatch, _gameOverBackgroundRenderTarget);

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
                DrawPlayingScene(spriteBatch, virtualDrawTarget);
            }
        }

        public void DrawPlayingScene(
            SpriteBatch spriteBatch,
            RenderTarget2D virtualDrawTarget)
        {
            var viewMatrix = _camera.GetViewMatrix();
            DrawLightMap(spriteBatch, virtualDrawTarget, viewMatrix);
            
            // Avvio il disegno con l'opzione per lo Z-Index
            spriteBatch.Begin(
                transformMatrix: viewMatrix, 
                samplerState: SamplerState.PointClamp, 
                sortMode: SpriteSortMode.FrontToBack);

            // Disegno tutto il pavimento (che non ha ZIndex perché sta sempre sotto)
            _gameGrid.Draw(spriteBatch);

            // Disegno di tutti gli oggetti
            _exit.Draw(spriteBatch);
            spriteBatch.Draw(_playerStartingPointTexture, _playerStartingPointDrawingInfos);

            if (_triggerObjects != null)
            {
                foreach (var triggerObject in _triggerObjects)
                    triggerObject.Draw(spriteBatch);
            }

            _key.Draw(spriteBatch);

            foreach (var candlePack in _candlePacks)
                candlePack.Draw(spriteBatch);
            
            foreach (var otherObject in _gameGrid.DrawableObjects)
                otherObject.Draw(spriteBatch);
            
            foreach (var littleLight in _gameGrid.LittleLights)
                littleLight.Draw(spriteBatch);

            _gameGrid.Draw(spriteBatch);

            _candlesManager.Draw(spriteBatch);
            _player.Draw(spriteBatch);

            foreach (Enemy en in _enemies)
                en.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, _blendStateMultiply);
            spriteBatch.Draw(_lightMaskRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();


            // Disegno gli effetti in un layer separato sopra a tutto per semplicità
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);
            _candlesManager.DrawEffects(spriteBatch);
            _player.DrawEffects(spriteBatch);
            foreach (Enemy en in _enemies)
                en.DrawEffects(spriteBatch);
            foreach (var littleLight in _gameGrid.LittleLights)
                littleLight.DrawEffects(spriteBatch);
            _gameGrid.DrawEffects(spriteBatch);
            spriteBatch.End();
                

            spriteBatch.Begin();
            _gameHud.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
