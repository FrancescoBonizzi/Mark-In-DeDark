using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.StringsLocalization.Abstractions;
using FbonizziMonoGame.TransformationObjects;

using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public bool IsPaused { get; private set; }

        private readonly TimeSpan _gameFinishTimer = TimeSpan.FromSeconds(4);
        private readonly TimeSpan _gameFinishGameOverSceneTimer = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _attackTimer = TimeSpan.FromSeconds(1);
        private TimeSpan _elapsedFinish;
        private FadeObject _gameOverFadeObject;

        private readonly GameHUD _gameHud;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly RenderTarget2D _baseSceneRenderTarget;
        private readonly RenderTarget2D _baseSceneWithNormalMapRenderTarget;
        private readonly RenderTarget2D _lightMaskRenderTarget;
        private readonly RenderTarget2D _normalMapRenderTarget;

        private readonly RenderTarget2D _gameOverBackgroundRenderTarget;
        private readonly RenderTarget2D _gameOverForegroundRenderTarget;
     
        private readonly IScreenTransformationMatrixProvider _matrixScaleProvider;
        private readonly GameOrchestrator _gameOrchestrator;
        private readonly SoundManager _soundManager;
        private readonly GameConfig _gameConfig;
        private readonly Level _currentLevel;
        private readonly MarkGamePad _markGamePad;
        private readonly Color _backgroundColorForLightMask;

        private readonly Camera2D _camera;
        private readonly List<CandlePack> _candlePacks;
        private Vector2 _playerCameraOffset;

        private bool? _runGameOverScene;
        private bool _shakeViewport;

        private readonly Effect _normalMapShader;
        private readonly Effect _lightMaskShader;

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

        public MarkInDeDarkGame(
            GraphicsDevice graphicsDevice,
            GameOrchestrator gameOrchestrator,
            AssetsLoader assetsLoader,
            Level selectedLevel,
            GameConfig gameConfig,
            IScreenTransformationMatrixProvider matrixScaleProvider,
            SoundManager soundManager,
            ILocalizedStringsRepository localizedStringsRepository,
            MarkGamePad markGamePad)
        {
            _graphicsDevice = graphicsDevice;
            _currentGameState = GameStates.Playing;
            _localizedStringsRepository = localizedStringsRepository;

            _normalMapShader = assetsLoader.NormalMapShader;
            _lightMaskShader = assetsLoader.LightMaskShader;

            _elapsedFinish = TimeSpan.Zero;

            _matrixScaleProvider = matrixScaleProvider;

            _gameOrchestrator = gameOrchestrator;
            _soundManager = soundManager;
            _gameConfig = gameConfig;
            _playerCameraOffset = gameConfig.CameraOffsetFromPlayer.ToXnaVector2();

            var presentationParameters = graphicsDevice.PresentationParameters;
            _lightMaskRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _normalMapRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _baseSceneRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _baseSceneWithNormalMapRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);

            _gameOverBackgroundRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            _gameOverForegroundRenderTarget = new RenderTarget2D(graphicsDevice, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
            

            if (gameConfig.GodModePlayerSeesEverything)
                _backgroundColorForLightMask = Color.White;
            else _backgroundColorForLightMask = Color.Black;

            _currentLevel = selectedLevel;
            _markGamePad = markGamePad;

            _camera = new Camera2D(_matrixScaleProvider)
            {
                Zoom = 0.8f
            };
            _gameGrid = new GameGrid(_currentLevel, assetsLoader, _camera, _soundManager, gameConfig, this);
            _candlesManager = new CandlesManager(0, assetsLoader, _soundManager, _gameGrid, gameConfig);
            _player = new Player(assetsLoader, _gameGrid, gameConfig, _candlesManager, _soundManager, this);

            _exit = new Exit(assetsLoader, gameConfig, _gameGrid.ExitPosition);
            _key = new CollectableObject(assetsLoader.Sprites["Objects/key"], _gameGrid.KeyPosition, _soundManager);

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
                            gameConfig,
                            this);
                        _enemies.Add(newGoblin);
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
        }

        public void HandleActionsInput(HashSet<CharacterActions> actions)
        {
            if (actions.Contains(CharacterActions.Candle) && _candlesManager.LeftCandles == 0)
            {
                _gameHud.ShowMessage(
                    _localizedStringsRepository.Get("NotEnoughCandlesString"),
                    _gameConfig.NotEnoughCandlesStringColorRGB);
            }

            _player.HandleInput(actions);
        }

        private void RepositionCamera()
        {
            var newCameraPosition = _player.Position - _playerCameraOffset;
            _camera.Position = new Vector2(
                MathHelper.Lerp(_camera.Position.X, newCameraPosition.X, 0.1f),
                MathHelper.Lerp(_camera.Position.Y, newCameraPosition.Y, 0.1f));
        }

        public void Update(TimeSpan elapsed, HashSet<CharacterActions> inputs)
        {
            if (IsPaused)
                return;

            HandleActionsInput(inputs);

            switch (_currentGameState)
            {
                case GameStates.Playing:
                    ElapsedGameTime += elapsed;

                    RepositionCamera();

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
                        if (_player.CollisionRectangleForAttack.Intersects(_key.CollisionRectangle))
                        {
                            _key.Collect();
                            _gameHud.ShowMessage(
                                string.Format(_localizedStringsRepository.Get("KeyFoundString")),
                                _gameConfig.KeyFoundStringColorRGB);
                            _exit.SetOpen();
                        }
                    }

                    if (_player.CollisionRectangleForAttack.Intersects(_exit.CollisionRectangle))
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
                            if (!candlePack.IsCollected && _player.CollisionRectangleForAttack.Intersects(candlePack.CollisionRectangle))
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

            foreach (var littleLight in _gameGrid.LittleLights)
            {
                littleLight.Update(elapsed);
            }

            _gameGrid.Update(elapsed);
            _player.Update(elapsed, inputs);
            _candlesManager.Update(elapsed);
            _key.Update(elapsed);
            _exit.Update(elapsed);
            _gameHud.Update(elapsed);
            foreach (var enemy in _enemies)
            {
                if (!enemy.IsDead)
                    enemy.Update(elapsed);
                else
                    enemy.UpdateAfterDeath(elapsed);
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

        private void DrawLightMask(
            SpriteBatch spriteBatch,
            Matrix viewMatrix)
        {
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
            var v = new Vector3(_player.LightDirection * 200f, 200f);
            v.Normalize();
            _normalMapShader.Parameters["LightDirection"].SetValue(v);

            // Disegno lo strato di luci
            _graphicsDevice.SetRenderTarget(_lightMaskRenderTarget);
            _graphicsDevice.Clear(_backgroundColorForLightMask);
            DrawLightMask(spriteBatch, viewMatrix);

            // Avvio il disegno con l'opzione per lo Z-Index
            _graphicsDevice.SetRenderTarget(_baseSceneRenderTarget);
            _graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(
                transformMatrix: viewMatrix, 
                samplerState: SamplerState.PointClamp, 
                sortMode: SpriteSortMode.FrontToBack);

            // Disegno il pavimento
            _gameGrid.Draw(spriteBatch);

            // Disegno di tutti gli oggetti
            _exit.Draw(spriteBatch);

            if (_triggerObjects != null)
            {
                foreach (var triggerObject in _triggerObjects)
                    triggerObject.Draw(spriteBatch);
            }

            _key.Draw(spriteBatch);

            foreach (var candlePack in _candlePacks)
                candlePack.Draw(spriteBatch);
            
            foreach (var littleLight in _gameGrid.LittleLights)
                littleLight.Draw(spriteBatch);

            _gameGrid.Draw(spriteBatch);
            _candlesManager.Draw(spriteBatch);
            _player.Draw(spriteBatch);

            foreach (Enemy en in _enemies)
                en.Draw(spriteBatch);

            spriteBatch.End();

            // Disegno la normal map
            //_graphicsDevice.SetRenderTarget(_normalMapRenderTarget);
            //_graphicsDevice.Clear(Color.Black);
            //spriteBatch.Begin(
            //    transformMatrix: viewMatrix,
            //    samplerState: SamplerState.PointClamp,
            //    sortMode: SpriteSortMode.FrontToBack);
            //_gameGrid.DrawNormalMap(spriteBatch);
            //spriteBatch.End();


            // Applico gli effetti

            // Scena con NormalMap
            //_graphicsDevice.SetRenderTarget(_baseSceneWithNormalMapRenderTarget);
            //_normalMapShader.Parameters["NormalTexture"].SetValue((Texture2D)_normalMapRenderTarget);
            //_normalMapShader.CurrentTechnique.Passes[0].Apply();
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _normalMapShader);
            //spriteBatch.Draw((Texture2D)_baseSceneRenderTarget, Vector2.Zero, Color.White);
            //spriteBatch.End();

            // Scena LightMask
            _graphicsDevice.SetRenderTarget(virtualDrawTarget);
            _lightMaskShader.Parameters["lightMask"].SetValue((Texture2D)_lightMaskRenderTarget);
            _lightMaskShader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _lightMaskShader);
            spriteBatch.Draw((Texture2D)_baseSceneRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Effects over the dark
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);
            foreach (Enemy en in _enemies)
                en.DrawEffectsOverDark(spriteBatch);
            spriteBatch.End();

            // Debug
            //spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);
            //_gameGrid.DrawForDebug(spriteBatch);
            //_player.DrawForDebug(spriteBatch);
            //foreach (Enemy en in _enemies)
            //    en.DrawForDebug(spriteBatch);
            //spriteBatch.End();

            // HUD
            spriteBatch.Begin();
            _gameHud.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
