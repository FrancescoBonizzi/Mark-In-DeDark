using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Drawing.Abstractions;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.PlatformAbstractions;
using FbonizziMonoGame.StringsLocalization.Abstractions;
using Mark.Engine;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Common;
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
        private readonly GameGrid _grid;
        private readonly Exit _exit;
        private readonly Player _player;
        private readonly List<Enemy> _enemies;
        private readonly List<TriggerObject> _triggerObjects;
        private readonly CandlesManager _candlesManager;
        private readonly CollectableObject _key;

        public bool IsPaused { get; private set; }

        private readonly TimeSpan _gameFinishTimer = TimeSpan.FromSeconds(4);
        private readonly TimeSpan _attackTimer = TimeSpan.FromSeconds(1);
        private TimeSpan _elapsedFinish;

        private readonly GameHUD _gameHud;

        private GraphicsDevice _graphicsDevice;
        private RenderTarget2D _lightMaskRenderTarget;
        private BlendState _blendStateMultiply;
        private readonly IScreenTransformationMatrixProvider _matrixScaleProvider;
        private readonly GameOrchestrator _gameOrchestrator;
        private readonly SoundManager _soundManager;
        private readonly GameConfig _gameConfig;
        private readonly Level _currentLevel;
        private readonly Color _backgroundColorForLightMask;

        private Camera2D _camera;
        private List<CandlePack> _candlePacks;
        private Vector2 _playerCameraOffset;

        public TimeSpan ElapsedGameTime { get; private set; }

        private enum GameStates
        {
            Playing,
            Finishing,
            Finished
        }

        private GameStates _currentGameState;
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

            _camera = new Camera2D(_matrixScaleProvider);
            _grid = new GameGrid(_currentLevel, assetsLoader, _camera, _soundManager);
            _candlesManager = new CandlesManager(0, assetsLoader, _soundManager, _grid, gameConfig);
            _player = new Player(assetsLoader, _grid, gameConfig, _candlesManager, _soundManager);

            _exit = new Exit(assetsLoader, gameConfig, _grid.ExitPosition);
            _key = new CollectableObject(assetsLoader.Sprites["key"], _grid.KeyPosition, _soundManager);

            _enemies = new List<Enemy>();
            _triggerObjects = new List<TriggerObject>();

            // Se c'è il trigger, il goblin spawna solo quando viene azionato
            if (_grid.GoblinTriggerRectangle != null && _grid.GoblinStartPosition != null)
            {
                _triggerObjects.Add(new TriggerObject(
                    triggerCollisionRectangle: _grid.GoblinTriggerRectangle.Value,
                    affectedCharacters: new Character[] { _player },
                    onTriggerPressed: (character, elapsed) =>
                    {
                        var newGoblin = new Goblin(
                            assetsLoader,
                            _grid.GoblinStartPosition.Value,
                            _grid,
                            _candlesManager,
                            _soundManager,
                            _player,
                            gameConfig);
                        _enemies.Add(newGoblin);
                        _soundManager.PlayBreakWallSound();

                        // Aggiungo il goblin al controllo delle trappole
                        foreach (var trap in _triggerObjects.Where(t => t is InstantlyDieTrap || t is TimedBreakableTrap))
                        {
                            trap.AddAffectedCharacter(newGoblin);
                        }
                    },
                    onTriggerLeft: null,
                    shouldFireOnce: true));
            }
            // Se non metto il trigger, lo spawno subito
            else if (_grid.GoblinStartPosition != null)
            {
                _enemies.Add(new Goblin(assetsLoader, _grid.GoblinStartPosition.Value, _grid, _candlesManager, _soundManager, _player, gameConfig));
            }

            if (_grid.WerewolfStartingPosition != null)
            {
                _enemies.Add(new Werewolf(assetsLoader, _grid.WerewolfStartingPosition.Value, _grid, _candlesManager, _soundManager, _player, gameConfig));
            }

            if (_grid.ZombiesPath != null)
            {
                foreach (var zombiePath in _grid.ZombiesPath)
                {
                    _enemies.Add(new Zombie(assetsLoader, zombiePath, _grid, _candlesManager, _soundManager, _player, gameConfig));
                }
            }

            if (_grid.SkeletronsPath != null)
            {
                foreach (var skeletronPath in _grid.SkeletronsPath)
                {
                    _enemies.Add(new Skeletron(assetsLoader, skeletronPath, _grid, _candlesManager, _soundManager, _player, gameConfig));
                }
            }

            if (_grid.TaurusPath != null)
            {
                foreach (var taurusPath in _grid.TaurusPath)
                {
                    _enemies.Add(new Taurus(assetsLoader, taurusPath, _grid, _candlesManager, _soundManager, _player, gameConfig, _enemies));
                }
            }

            if (_grid.InstantlyDieTrapCollisionRectangles != null)
            {
                foreach (var instantlyDieTrapRectangle in _grid.InstantlyDieTrapCollisionRectangles)
                {
                    _triggerObjects.Add(new InstantlyDieTrap(
                        assetsLoader,
                        (new List<Character> { _player }).Concat(_enemies),
                        instantlyDieTrapRectangle));
                }
            }

            if (_grid.BrekableTrapCollisionRectangles != null)
            {
                foreach (var breakableTrapRectangle in _grid.BrekableTrapCollisionRectangles)
                {
                    _triggerObjects.Add(new TimedBreakableTrap(
                        assetsLoader,
                        _gameConfig,
                        breakableTrapRectangle,
                        (new List<Character> { _player }).Concat(_enemies)));
                }
            }

            _candlePacks = _grid.CandlePacks;
            ElapsedGameTime = TimeSpan.Zero;
            _gameHud = new GameHUD(assetsLoader, matrixScaleProvider, _candlesManager);
        }

        public void HandleInput(CharacterActions action)
        {
            if (action == CharacterActions.Candle && _candlesManager.LeftCandles == 0)
            {
                _gameHud.ShowMessage(
                    _localizedStringsRepository.Get(GameStringsLoader.NotEnoughCandlesString),
                    _gameConfig.NotEnoughCandlesStringColorRGB);
            }

            _player.HandleInput(action);
        }

        private void RepositionCamera()
            => _camera.Position = _player.Position - _playerCameraOffset;

        public void Update(TimeSpan elapsed)
        {
            if (IsPaused)
                return;

            switch (_currentGameState)
            {
                case GameStates.Playing:
                    ElapsedGameTime += elapsed;
                    RepositionCamera();

                    if (_player.IsDying)
                    {
                        _gameHud.ShowMessage(
                             string.Format(_localizedStringsRepository.Get(GameStringsLoader.GameOverString)),
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
                                string.Format(_localizedStringsRepository.Get(GameStringsLoader.KeyFoundString)),
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
                               string.Format(_localizedStringsRepository.Get(GameStringsLoader.LevelCompletedString)),
                               _gameConfig.LevelCompletedStringColorRGB,
                               true);
                            _currentGameState = GameStates.Finishing;
                        }
                        else
                        {
                            _gameHud.ShowMessage(
                                string.Format(_localizedStringsRepository.Get(GameStringsLoader.ClosedDoorString)),
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
                                    string.Format(_localizedStringsRepository.Get(GameStringsLoader.CandlePackFoundString), candlePack.NCandlesInThisPack),
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

            _grid.Update(elapsed);
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
            _candlesManager.DrawLightmap(spriteBatch);

            spriteBatch.End();

            _graphicsDevice.SetRenderTarget(virtualDrawTarget);
        }

        public void Draw(
            SpriteBatch spriteBatch,
            RenderTarget2D virtualDrawTarget)
        {
            var viewMatrix = _camera.GetViewMatrix();
            DrawLightMap(spriteBatch, virtualDrawTarget, viewMatrix);

            spriteBatch.Begin(transformMatrix: viewMatrix);

            _grid.DrawCells(spriteBatch);
            _grid.DrawDirtys(spriteBatch);
            _exit.Draw(spriteBatch);

            if (_triggerObjects != null)
            {
                foreach (var triggerObject in _triggerObjects)
                    triggerObject.Draw(spriteBatch);
            }

            _grid.DrawWalls(spriteBatch);
            _grid.DrawAngles(spriteBatch);

            _key.Draw(spriteBatch);
            foreach (var candlePack in _candlePacks)
                candlePack.Draw(spriteBatch);

            _candlesManager.DrawSmoke(spriteBatch);

            // I nemici morti vengono disegnati sotto il player
            foreach (Enemy en in _enemies.Where(enemy => enemy.IsDead))
            {
                en.Draw(spriteBatch);
            }

            _candlesManager.Draw(spriteBatch);
            _player.Draw(spriteBatch);

            // I nemici vivi vengono disegnati sopra al player
            foreach (Enemy en in _enemies.Where(enemy => !enemy.IsDead))
                en.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, _blendStateMultiply);
            spriteBatch.Draw(_lightMaskRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin();
            _gameHud.Draw(spriteBatch);
            spriteBatch.End();
        }

    }
}
