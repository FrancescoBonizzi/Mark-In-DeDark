using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Lighting;
using MarkInDeDark.Library.Maths;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace MarkInDeDark.Library.Characters
{
    public abstract class Character
    {
        public enum CharacterState
        {
            GoingUp,
            GoingDown,
            GoingLeft,
            GoingRight,

            IdleUp,
            IdleDown,
            IdleLeft,
            IdleRight,

            Attacking,
            Dying,
            Dead
        }

        private enum AnimationsNames
        {
            WalkUp,
            WalkDown,
            WalkLeft,
            WalkRight,

            IdleUp,
            IdleDown,
            IdleLeft,
            IdleRight
        }

        private enum ShadowsAnimationsNames
        {
            WhileWalkingUp,
            WhileWalkingDown,
            WhileWalkingLeft,
            WhileWalkingRight,

            WhileIdleUp,
            WhileIdleDown,
            WhileIdleLeft,
            WhileIdleRight,

            WhileDead
        }

        public enum DieAnimationType
        {
            Falling,
            StandardAnimation,
            Explosion
        }

        public CharacterState CurrentCharacterState { get; set; }
        public DieAnimationType CurrentDieAnimationType { get; set; }

        protected bool _canGoOverWalls = false;

        public bool IsDead => CurrentCharacterState == CharacterState.Dead;
        public bool IsDying => CurrentCharacterState == CharacterState.Dying;
        public MarkLight SurroundingLight { get; protected set; }

        public bool AreLightsActive { get; set; }

        public bool IsMoving => _movementLerpAmount < 1f && !IsDead && !IsDying;

        protected bool _stopMovement = false;

        public DrawingInfos MainFigureDrawingInfos { get; set; }
        protected DrawingInfos _shadowsDrawingInfos;

        protected readonly GameGrid _gameGrid;
        private readonly SoundManager _soundManager;
        private float _movementLerpAmount;
        private readonly MarkInDeDarkGame _game;
        private Vector2 _nextLocation;
        private Vector2 _startingLocation;

        protected Vector2 _torchPositionTraslation;
        protected Vector2 _torchPositionTraslationUp;
        protected Vector2 _torchPositionTraslationLeft;
        protected Vector2 _torchPositionTraslationRight;
        protected Vector2 _torchPositionTraslationDown;

        private Vector2 _shadowPositionTraslation;
        private Vector2 _shadowPositionTraslationUp;
        private Vector2 _shadowPositionTraslationLeft;
        private Vector2 _shadowPositionTraslationRight;
        private Vector2 _shadowPositionTraslationDown;
        private Vector2 _shadowPositionTraslationDownIdle;

        private TimeSpan _currentMovementElapsed = TimeSpan.Zero;
        public float CellMovementSeconds { get; set; }

        private CharacterActions? _nextActionToProcess;

        public AnimationsManager MainAnimationsAnimationManager { get; }
        public AnimationsManager ShadowsAnimationManager { get; }

        private CharacterFallingInTrapAnimation _characterFallingATrapAnimation;

        public float CurrentZIndex => MainFigureDrawingInfos.LayerDepth;

        public bool DontPlayDefaultAnimations { get; set; } = false;

        protected TimeSpan _deadBodyPartLifetime = TimeSpan.FromSeconds(3);
        protected bool _deadBodyEffectWithFading = true;

        protected readonly CharacterExplosionParticleSystem _characterDieExplosionParticleSystem;

        public bool ShouldDoExplosionAnimation { get; set; } = false;

        public Character(
            GameGrid grid,
            SoundManager soundManager,
            AssetsLoader assetsLoader,
            CharacterConfiguration configuration,
            MarkInDeDarkGame game)
        {
            _game = game;
            _movementLerpAmount = 1.0f;

            _gameGrid = grid;
            _soundManager = soundManager;

            MainFigureDrawingInfos = new DrawingInfos()
            {
                Position = configuration.StartingPosition,
                HitBoxTolerance = new Rectangle(16, 16, 32, 32),
                Origin = configuration.WalkUpAnimation.FirstFrameSprite.SpriteCenter
            };

            _nextLocation = Position;
            CellMovementSeconds = configuration.MovementSpeed;

            if (configuration.BodyParts != null)
            {
                _characterDieExplosionParticleSystem = new CharacterExplosionParticleSystem(configuration, assetsLoader);
            }

            MainAnimationsAnimationManager = new AnimationsManager()
                .AddAnimation(AnimationsNames.WalkUp.ToString(), configuration.WalkUpAnimation)
                .AddAnimation(AnimationsNames.WalkDown.ToString(), configuration.WalkDownAnimation)
                .AddAnimation(AnimationsNames.WalkRight.ToString(), configuration.WalkSideAnimation)
                .AddAnimation(AnimationsNames.WalkLeft.ToString(), configuration.WalkSideAnimation)
                .AddAnimation(AnimationsNames.IdleUp.ToString(), configuration.IdleUpAnimation)
                .AddAnimation(AnimationsNames.IdleDown.ToString(), configuration.IdleDownAnimation)
                .AddAnimation(AnimationsNames.IdleLeft.ToString(), configuration.IdleSideAnimation)
                .AddAnimation(AnimationsNames.IdleRight.ToString(), configuration.IdleSideAnimation);

            CurrentCharacterState = CharacterState.IdleDown;
            MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleDown.ToString());

            if (configuration.CharacterShadowConfiguration != null)
            {
                ShadowsAnimationManager = new AnimationsManager()
                    .AddAnimation(ShadowsAnimationsNames.WhileWalkingUp.ToString(), configuration.CharacterShadowConfiguration.ShadowWhileWalkingUpAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileWalkingDown.ToString(), configuration.CharacterShadowConfiguration.ShadowWhileWalkingDownAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileWalkingRight.ToString(), configuration.CharacterShadowConfiguration.ShadowWhileWalkingSideAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileWalkingLeft.ToString(), configuration.CharacterShadowConfiguration.ShadowWhileWalkingSideAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileIdleUp.ToString(), configuration.CharacterShadowConfiguration.ShadowWhileIdleUpAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileIdleDown.ToString(), configuration.CharacterShadowConfiguration.ShadowWhileIdleDownAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileIdleLeft.ToString(), configuration.CharacterShadowConfiguration.ShadowWhileIdleSideAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileIdleRight.ToString(), configuration.CharacterShadowConfiguration.ShadowWhileIdleSideAnimation);

                _shadowPositionTraslationDown = configuration.CharacterShadowConfiguration.ShadowPositionTraslationDown;
                _shadowPositionTraslationLeft = configuration.CharacterShadowConfiguration.ShadowPositionTraslationLeft;
                _shadowPositionTraslationRight = configuration.CharacterShadowConfiguration.ShadowPositionTraslationRight;
                _shadowPositionTraslationUp = configuration.CharacterShadowConfiguration.ShadowPositionTraslationUp;
                _shadowPositionTraslationDownIdle = configuration.CharacterShadowConfiguration.ShadowPositionTraslationDownIdle;

                _shadowsDrawingInfos = new DrawingInfos()
                {
                    OverlayColor = Color.Black.WithAlpha(0.9f)
                };
                PlayShadowFor(ShadowsAnimationsNames.WhileIdleDown);
            }

            if (configuration.CharacterTorchConfiguration != null)
            {
                AreLightsActive = true;
                _torchPositionTraslationUp = configuration.CharacterTorchConfiguration.TorchPositionTraslationUp;
                _torchPositionTraslationLeft = configuration.CharacterTorchConfiguration.TorchPositionTraslationLeft;
                _torchPositionTraslationRight = configuration.CharacterTorchConfiguration.TorchPositionTraslationRight;
                _torchPositionTraslationDown = configuration.CharacterTorchConfiguration.TorchPositionTraslationDown;
                SurroundingLight = new MarkLight(
                    assetsLoader: assetsLoader,
                    firstAnimationColor: configuration.CharacterTorchConfiguration.FirstTorchColor,
                    secondAnimationColor: configuration.CharacterTorchConfiguration.SecondTorchColor,
                    z: configuration.CharacterTorchConfiguration.Z,
                    opacity: configuration.CharacterTorchConfiguration.Opacity,
                    distanceSquared: configuration.CharacterTorchConfiguration.DistanceSquared,
                    castsShadows: false,
                    penumbraLightScale: configuration.CharacterTorchConfiguration.PenumbraTorchScale);
                _torchPositionTraslation = _torchPositionTraslationDown;
                SetLightPosition();
            }
        }

        public void SetTemporaryColor(Color color)
        {
            SurroundingLight?.SetTemporaryColorForAnimation(color);
        }

        protected virtual void OnChangeDirection(CharacterState characterState)
        {

        }

        public void ResetNormalColor()
        {
            SurroundingLight?.ResetColor();
        }

        public Vector2 TorchPosition
            => SurroundingLight.Position;

        public Vector2 WorldCenter
            => MainFigureDrawingInfos.Center(
                MainAnimationsAnimationManager.CurrentAnimation.CurrentFrameWidth,
                MainAnimationsAnimationManager.CurrentAnimation.CurrentFrameHeight);

        public Vector2 Position
            => MainFigureDrawingInfos.Position;

        public Rectangle CollisionRectangle
            => MainFigureDrawingInfos.HitBox(
                MainAnimationsAnimationManager.CurrentAnimation.CurrentFrameWidth,
                MainAnimationsAnimationManager.CurrentAnimation.CurrentFrameWidth);

        public float Height
            => MainAnimationsAnimationManager.CurrentAnimation.CurrentFrameHeight;

        public float Width
            => MainAnimationsAnimationManager.CurrentAnimation.CurrentFrameWidth;

        public virtual void Die()
        {
            _gameGrid.ApplyBloodDecal(WorldCenter);
            //_soundManager.PlayBlood();
            CurrentCharacterState = CharacterState.Dying;

            if (ShouldDoExplosionAnimation)
            {
                CurrentDieAnimationType = DieAnimationType.Explosion;

                _characterDieExplosionParticleSystem.Explode(
                    WorldCenter,
                    ZIndexCalculator.OverAllLayerDepth,
                    _deadBodyPartLifetime,
                    _deadBodyEffectWithFading);
            }
            else
            {
                CurrentDieAnimationType = DieAnimationType.StandardAnimation;
                MainAnimationsAnimationManager.PlayAnimation("Dying");
            }
        }

        public virtual void FallInTrap(Vector2 trapCenter)
        {
            _characterFallingATrapAnimation = new CharacterFallingInTrapAnimation(
                MainAnimationsAnimationManager.GetAnimation(AnimationsNames.IdleDown.ToString()).FirstFrameSprite,
                trapCenter,
                this,
                _game);

            CurrentDieAnimationType = DieAnimationType.Falling;
            CurrentCharacterState = CharacterState.Dying;

            _torchPositionTraslation = new Vector2(0);
            if (AreLightsActive)
                SetLightPosition();

            if (SurroundingLight != null)
            {
                SurroundingLight.IsGlowEnabled = false;
                if (SurroundingLight.PenumbraLight != null)
                    SurroundingLight.PenumbraLight.Enabled = false;
            }
        }

        public void SetDirectionAnimation(CharacterActions direction)
        {
            switch (direction)
            {
                case CharacterActions.Up:
                    MainFigureDrawingInfos.IsFlipped = false;
                    if (!DontPlayDefaultAnimations)
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleUp.ToString());
                    ShadowsAnimationManager?.PlayAnimation(ShadowsAnimationsNames.WhileIdleUp.ToString());
                    _torchPositionTraslation = _torchPositionTraslationUp;
                    OnChangeDirection(CharacterState.IdleUp);
                    break;

                case CharacterActions.Down:
                    MainFigureDrawingInfos.IsFlipped = false;
                    if (!DontPlayDefaultAnimations)
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleDown.ToString());
                    ShadowsAnimationManager?.PlayAnimation(ShadowsAnimationsNames.WhileIdleDown.ToString());
                    _torchPositionTraslation = _torchPositionTraslationDown;
                    OnChangeDirection(CharacterState.IdleDown);
                    break;

                case CharacterActions.Right:
                    MainFigureDrawingInfos.IsFlipped = false;
                    if (!DontPlayDefaultAnimations)
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleRight.ToString());
                    ShadowsAnimationManager?.PlayAnimation(ShadowsAnimationsNames.WhileIdleRight.ToString());
                    _torchPositionTraslation = _torchPositionTraslationRight;
                    OnChangeDirection(CharacterState.IdleRight);
                    break;

                case CharacterActions.Left:
                    MainFigureDrawingInfos.IsFlipped = true;
                    if (!DontPlayDefaultAnimations)
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleLeft.ToString());
                    ShadowsAnimationManager?.PlayAnimation(ShadowsAnimationsNames.WhileIdleLeft.ToString());
                    _torchPositionTraslation = _torchPositionTraslationLeft;
                    OnChangeDirection(CharacterState.IdleLeft);

                    break;
            }

        }

        private void PlayShadowFor(ShadowsAnimationsNames shadowsAnimationName)
        {
            if (ShadowsAnimationManager == null)
                return;

            ShadowsAnimationManager.PlayAnimation(shadowsAnimationName.ToString());
            _shadowsDrawingInfos.Origin = ShadowsAnimationManager.CurrentAnimation.FirstFrameSprite.SpriteCenter;

            switch (shadowsAnimationName)
            {
                case ShadowsAnimationsNames.WhileWalkingUp:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = _shadowPositionTraslationUp;
                    break;

                case ShadowsAnimationsNames.WhileWalkingDown:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = _shadowPositionTraslationDown;
                    break;

                case ShadowsAnimationsNames.WhileWalkingLeft:
                    _shadowsDrawingInfos.IsFlipped = true;
                    _shadowPositionTraslation = _shadowPositionTraslationLeft;
                    break;

                case ShadowsAnimationsNames.WhileWalkingRight:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = _shadowPositionTraslationRight;
                    break;

                case ShadowsAnimationsNames.WhileIdleUp:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = _shadowPositionTraslationUp;
                    break;

                case ShadowsAnimationsNames.WhileIdleDown:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = _shadowPositionTraslationDownIdle;
                    break;

                case ShadowsAnimationsNames.WhileIdleLeft:
                    _shadowsDrawingInfos.IsFlipped = true;
                    _shadowPositionTraslation = _shadowPositionTraslationLeft;
                    break;

                case ShadowsAnimationsNames.WhileIdleRight:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = _shadowPositionTraslationRight;
                    break;

                case ShadowsAnimationsNames.WhileDead:
                    break;
            }
        }

        private void GoNextCellUp(GameCell currentCell)
        {
            if (_canGoOverWalls || (currentCell.IsNorthPassable && currentCell.IsNorthDoorPassable && !currentCell.NorthCell.IsUnwalkable))
            {
                GameCell nextCell;
                if (_canGoOverWalls)
                    nextCell = currentCell.NorthCellWithoutLinks;
                else nextCell = currentCell.NorthCell;

                if (nextCell == null)
                    return;

                CurrentCharacterState = CharacterState.GoingUp;

                if (!DontPlayDefaultAnimations)
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkUp.ToString());

                _startingLocation = MainFigureDrawingInfos.Position;
                MainFigureDrawingInfos.IsFlipped = false;
                _nextLocation = nextCell.CenterPosition;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationUp;

                PlayShadowFor(ShadowsAnimationsNames.WhileWalkingUp);
                OnChangeDirection(CurrentCharacterState);
            }
        }

        private void GoNextCellDown(GameCell currentCell)
        {
            if (_canGoOverWalls || (currentCell.IsSouthPassable && currentCell.IsSouthDoorPassable && !currentCell.SouthCell.IsUnwalkable))
            {
                GameCell nextCell;
                if (_canGoOverWalls)
                    nextCell = currentCell.SouthCellWithoutLinks;
                else nextCell = currentCell.SouthCell;

                if (nextCell == null)
                    return;

                CurrentCharacterState = CharacterState.GoingDown;

                if (!DontPlayDefaultAnimations)
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkDown.ToString());

                _startingLocation = MainFigureDrawingInfos.Position;
                MainFigureDrawingInfos.IsFlipped = false;
                _nextLocation = nextCell.CenterPosition;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationDown;

                PlayShadowFor(ShadowsAnimationsNames.WhileWalkingDown);
                OnChangeDirection(CurrentCharacterState);
            }
        }

        private void GoNextCellRight(GameCell currentCell)
        {
            if (_canGoOverWalls || (currentCell.IsEastPassable && currentCell.IsEastDoorPassable && !currentCell.EastCell.IsUnwalkable))
            {
                GameCell nextCell;
                if (_canGoOverWalls)
                    nextCell = currentCell.EastCellWithoutLinks;
                else nextCell = currentCell.EastCell;

                if (nextCell == null)
                    return;

                CurrentCharacterState = CharacterState.GoingRight;

                if (!DontPlayDefaultAnimations)
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkRight.ToString());

                MainFigureDrawingInfos.IsFlipped = false;
                _startingLocation = MainFigureDrawingInfos.Position;
                _nextLocation = nextCell.CenterPosition;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationRight;

                PlayShadowFor(ShadowsAnimationsNames.WhileWalkingRight);
                OnChangeDirection(CurrentCharacterState);
            }
        }

        private void GoNextCellLeft(GameCell currentCell)
        {
            if (_canGoOverWalls || (currentCell.IsWestPassable && currentCell.IsWestDoorPassable && !currentCell.WestCell.IsUnwalkable))
            {
                GameCell nextCell;
                if (_canGoOverWalls)
                    nextCell = currentCell.WestCellWithoutLinks;
                else nextCell = currentCell.WestCell;

                if (nextCell == null)
                    return;

                CurrentCharacterState = CharacterState.GoingLeft;

                if (!DontPlayDefaultAnimations)
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkLeft.ToString());

                MainFigureDrawingInfos.IsFlipped = true;
                _startingLocation = MainFigureDrawingInfos.Position;
                _nextLocation = nextCell.CenterPosition;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationLeft;

                PlayShadowFor(ShadowsAnimationsNames.WhileWalkingLeft);
                OnChangeDirection(CurrentCharacterState);
            }
        }

        private void HandleBufferedInput()
        {
            var currentCell = _gameGrid.GetCellAtPixel(MainFigureDrawingInfos.Position);

            if (!IsMoving)
            {
                switch (_nextActionToProcess)
                {
                    case CharacterActions.IdleLeft:
                        SetDirectionAnimation(CharacterActions.Left);
                        break;

                    case CharacterActions.IdleRight:
                        SetDirectionAnimation(CharacterActions.Right);
                        break;

                    case CharacterActions.IdleUp:
                        SetDirectionAnimation(CharacterActions.Up);
                        break;

                    case CharacterActions.IdleDown:
                        SetDirectionAnimation(CharacterActions.Down);
                        break;

                    case CharacterActions.Up:
                        GoNextCellUp(currentCell);
                        break;

                    case CharacterActions.Down:
                        GoNextCellDown(currentCell);
                        break;

                    case CharacterActions.Left:
                        GoNextCellLeft(currentCell);
                        break;

                    case CharacterActions.Right:
                        GoNextCellRight(currentCell);
                        break;

                    default:

                        switch (CurrentCharacterState)
                        {
                            case CharacterState.Attacking:
                                // Semplicemente impedisco di cambiare lo stato perché sta attaccando
                                break;

                            case CharacterState.GoingUp:
                                CurrentCharacterState = CharacterState.IdleUp;
                                if (!DontPlayDefaultAnimations)
                                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleUp.ToString());
                                PlayShadowFor(ShadowsAnimationsNames.WhileIdleUp);
                                break;

                            case CharacterState.GoingDown:
                                CurrentCharacterState = CharacterState.IdleDown;
                                if (!DontPlayDefaultAnimations)
                                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleDown.ToString());
                                ShadowsAnimationManager?.PlayAnimation(ShadowsAnimationsNames.WhileIdleDown.ToString());
                                PlayShadowFor(ShadowsAnimationsNames.WhileIdleDown);
                                break;

                            case CharacterState.GoingLeft:
                                CurrentCharacterState = CharacterState.IdleLeft;
                                if (!DontPlayDefaultAnimations)
                                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleLeft.ToString());
                                PlayShadowFor(ShadowsAnimationsNames.WhileIdleLeft);
                                break;

                            case CharacterState.GoingRight:
                                CurrentCharacterState = CharacterState.IdleRight;
                                if (!DontPlayDefaultAnimations)
                                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleRight.ToString());
                                PlayShadowFor(ShadowsAnimationsNames.WhileIdleRight);
                                break;
                        }
                        break;
                }
            }
            else // Reverse del movimento
            {
                switch (CurrentCharacterState)
                {
                    case CharacterState.GoingUp:
                        if (_nextActionToProcess == CharacterActions.Down)
                        {
                            CurrentCharacterState = CharacterState.GoingDown;
                            MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkDown.ToString());
                            Vector2 tmp = _nextLocation;
                            _nextLocation = _startingLocation;
                            _startingLocation = tmp;
                            _currentMovementElapsed = TimeSpan.FromSeconds(CellMovementSeconds - (float)_currentMovementElapsed.TotalSeconds);
                            _torchPositionTraslation = _torchPositionTraslationDown;

                            PlayShadowFor(ShadowsAnimationsNames.WhileWalkingDown);
                        }
                        break;

                    case CharacterState.GoingDown:
                        if (_nextActionToProcess == CharacterActions.Up)
                        {
                            CurrentCharacterState = CharacterState.GoingUp;
                            MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkUp.ToString());
                            Vector2 tmp = _nextLocation;
                            _nextLocation = _startingLocation;
                            _startingLocation = tmp;
                            _currentMovementElapsed = TimeSpan.FromSeconds(CellMovementSeconds - (float)_currentMovementElapsed.TotalSeconds);
                            _torchPositionTraslation = _torchPositionTraslationUp;

                            PlayShadowFor(ShadowsAnimationsNames.WhileWalkingUp);
                        }
                        break;

                    case CharacterState.GoingLeft:
                        if (_nextActionToProcess == CharacterActions.Right)
                        {
                            CurrentCharacterState = CharacterState.GoingRight;
                            MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkRight.ToString());
                            MainFigureDrawingInfos.IsFlipped = false;
                            Vector2 tmp = _nextLocation;
                            _nextLocation = _startingLocation;
                            _startingLocation = tmp;
                            _currentMovementElapsed = TimeSpan.FromSeconds(CellMovementSeconds - (float)_currentMovementElapsed.TotalSeconds);
                            _torchPositionTraslation = _torchPositionTraslationRight;

                            PlayShadowFor(ShadowsAnimationsNames.WhileWalkingRight);
                        }
                        break;

                    case CharacterState.GoingRight:
                        if (_nextActionToProcess == CharacterActions.Left)
                        {
                            CurrentCharacterState = CharacterState.GoingLeft;
                            MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkLeft.ToString());
                            MainFigureDrawingInfos.IsFlipped = true;
                            Vector2 tmp = _nextLocation;
                            _nextLocation = _startingLocation;
                            _startingLocation = tmp;
                            _currentMovementElapsed = TimeSpan.FromSeconds(CellMovementSeconds - (float)_currentMovementElapsed.TotalSeconds);
                            _torchPositionTraslation = _torchPositionTraslationLeft;

                            PlayShadowFor(ShadowsAnimationsNames.WhileWalkingLeft);
                        }
                        break;

                }
            }
        }

        public virtual void HandleInput(CharacterActions action)
        {
            _nextActionToProcess = action;
        }


        public virtual void Update(TimeSpan elapsed)
        {
            if (!IsDying && !IsDead)
            {
                HandleBufferedInput();

                if (IsMoving && !_stopMovement)
                {
                    _currentMovementElapsed += elapsed;
                    // 1 secondo per raggiugnere la cella
                    _movementLerpAmount = (float)(_currentMovementElapsed.TotalSeconds / CellMovementSeconds);

                    //_movementLerpAmount = MathHelper.Clamp(
                    //    _movementLerpAmount + Speed * (float)elapsed.TotalSeconds,
                    //    _startingLerpAmount,
                    //    1f);

                    MainFigureDrawingInfos.Position = Vector2.Lerp(
                        _startingLocation,
                        _nextLocation,
                        _movementLerpAmount);

                    if (_movementLerpAmount >= 1f)
                    {
                        //MainFigureDrawingInfos.Position = _nextLocation;
                        _currentMovementElapsed = TimeSpan.Zero;
                    }
                }

                SetShadowPosition();
                RecalculateZIndex();
                MainAnimationsAnimationManager.Update(elapsed);
                ShadowsAnimationManager?.Update(elapsed);
            }

            if (IsDying)
            {
                switch (CurrentDieAnimationType)
                {
                    case DieAnimationType.Falling:
                        _characterFallingATrapAnimation.Update(elapsed);

                        break;

                    case DieAnimationType.StandardAnimation:
                        if (MainAnimationsAnimationManager.CurrentAnimation.HasFinishedPlaying)
                        {
                            CurrentCharacterState = CharacterState.Dead;
                            SurroundingLight.IsEnabled = false;
                        }
                        else
                        {
                            MainAnimationsAnimationManager.Update(elapsed);
                        }
                        RecalculateZIndex();
                        break;

                    case DieAnimationType.Explosion:
                        _characterDieExplosionParticleSystem.Update(elapsed);
                        if (_characterDieExplosionParticleSystem.HasFinished)
                        {
                            CurrentCharacterState = CharacterState.Dead;
                            if (SurroundingLight != null)
                                SurroundingLight.IsEnabled = false;
                        }
                        RecalculateZIndex();
                        break;
                }
            }

            if (AreLightsActive)
            {
                SetLightPosition();
                // _lightMap.Update(elapsed);
                SurroundingLight?.Update(elapsed);
            }
        }

        public virtual void UpdateAfterDeath(TimeSpan elapsed)
        {
        }

        protected virtual void RecalculateZIndex()
        {
            MainFigureDrawingInfos.LayerDepth = ZIndexCalculator.ForCharacter(
                this,
                Position);
        }

        protected void SetLightPosition(float lerpAmount = 0.2f)
            => SetLightPosition(lerpAmount, WorldCenter + _torchPositionTraslation);

        protected void SetLightPosition(float lerpAmount, Vector2 position)
        {
            if (SurroundingLight == null)
                return;

            var newLightPosition = position;
            SurroundingLight.Position = Vector2.Lerp(SurroundingLight.Position, newLightPosition, lerpAmount);
        }

        protected void SetShadowPosition()
        {
            if (_shadowsDrawingInfos != null)
            {
                _shadowsDrawingInfos.Position = WorldCenter + _shadowPositionTraslation;
            }
        }

        public void DrawShadows(SpriteBatch spriteBatch)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.GoingUp:
                case CharacterState.GoingDown:
                case CharacterState.GoingLeft:
                case CharacterState.GoingRight:
                case CharacterState.IdleUp:
                case CharacterState.IdleDown:
                case CharacterState.IdleLeft:
                case CharacterState.IdleRight:
                case CharacterState.Attacking:
                    ShadowsAnimationManager?.Draw(spriteBatch, _shadowsDrawingInfos);
                    break;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.GoingUp:
                case CharacterState.GoingDown:
                case CharacterState.GoingLeft:
                case CharacterState.GoingRight:
                case CharacterState.IdleUp:
                case CharacterState.IdleDown:
                case CharacterState.IdleLeft:
                case CharacterState.IdleRight:
                case CharacterState.Attacking:
                    MainAnimationsAnimationManager.Draw(spriteBatch, MainFigureDrawingInfos);
                    break;

                case CharacterState.Dying:
                    switch (CurrentDieAnimationType)
                    {
                        case DieAnimationType.Falling:
                            _characterFallingATrapAnimation.Draw(spriteBatch);
                            break;

                        case DieAnimationType.StandardAnimation:
                            MainAnimationsAnimationManager.Draw(spriteBatch, MainFigureDrawingInfos);
                            break;

                        case DieAnimationType.Explosion:
                            _characterDieExplosionParticleSystem.Draw(spriteBatch);
                            break;
                    }
                    break;

                case CharacterState.Dead:

                    if (CurrentDieAnimationType == DieAnimationType.StandardAnimation)
                        if (this is Player)
                        {
                            MainAnimationsAnimationManager.Draw(spriteBatch, MainFigureDrawingInfos);
                        }

                    break;
            }
        }

        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.GoingUp:
                case CharacterState.GoingDown:
                case CharacterState.GoingLeft:
                case CharacterState.GoingRight:
                case CharacterState.IdleUp:
                case CharacterState.IdleDown:
                case CharacterState.IdleLeft:
                case CharacterState.IdleRight:
                case CharacterState.Attacking:
                    MainAnimationsAnimationManager.DrawNormalMap(spriteBatch, MainFigureDrawingInfos);
                    break;
            }
        }

        public virtual void DrawEffectsOverDark(SpriteBatch spriteBatch)
        {

        }

        public virtual void DrawLightMask(SpriteBatch spriteBatch)
        {
            if (SurroundingLight == null)
                return;
        }
    }
}
