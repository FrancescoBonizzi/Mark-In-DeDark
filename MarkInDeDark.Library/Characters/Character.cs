using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
            IdleRight,

            Die
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

        public CharacterState CurrentCharacterState { get; set; }

        protected bool _canGoOverWalls = false;

        public bool IsDead => CurrentCharacterState == CharacterState.Dead;
        public bool IsDying => CurrentCharacterState == CharacterState.Dying;
        private readonly Light _lightMap;
        protected Light _torch;

        public bool AreLightsActive { get; set; }

        public bool IsMoving => _movementLerpAmount < 1f && !IsDead && !IsDying;
        public float Speed { get; set; }

        protected bool _stopMovement = false;

        public DrawingInfos MainFigureDrawingInfos { get; set; }
        protected DrawingInfos _shadowsDrawingInfos;

        protected readonly GameGrid _gameGrid;
        private readonly SoundManager _soundManager;
        private float _movementLerpAmount;
        private readonly MarkInDeDarkGame _game;
        private float _startingLerpAmount;
        private Vector2 _nextLocation;
        private Vector2 _startingLocation;

        private Vector2 _torchPositionTraslation;
        private Vector2 _torchPositionTraslationUp;
        private Vector2 _torchPositionTraslationLeft;
        private Vector2 _torchPositionTraslationRight;
        private Vector2 _torchPositionTraslationDown;

        private Vector2 _shadowPositionTraslation;

        private CharacterActions? _nextActionToProcess;
        private readonly BloodParticleSystem _bloodEffect;

        public AnimationsManager MainAnimationsAnimationManager { get; }
        public AnimationsManager ShadowsAnimationManager { get; }

        private CharacterFallingInTrapAnimation _characterFallingATrapAnimation;

        public float CurrentZIndex => MainFigureDrawingInfos.LayerDepth;

        public bool DontPlayDefaultAnimations { get; set; } = false;

        public Character(
            GameGrid grid,
            SoundManager soundManager,
            AssetsLoader assetsLoader,
            CharacterConfiguration configuration,
            MarkInDeDarkGame game)
        {
            _game = game;
            _startingLerpAmount = 0f;
            _movementLerpAmount = 1.0f;

            _gameGrid = grid;
            _soundManager = soundManager;

            MainFigureDrawingInfos = new DrawingInfos()
            {
                Position = configuration.StartingPosition,
                HitBoxTolerance = new Rectangle(50, 50, 100, 100),
                Origin = configuration.WalkUpAnimation.FirstFrameSprite.SpriteCenter
            };

            _nextLocation = Position;
            Speed = configuration.MovementSpeed;

            MainAnimationsAnimationManager = new AnimationsManager()
                .AddAnimation(AnimationsNames.WalkUp.ToString(), configuration.WalkUpAnimation)
                .AddAnimation(AnimationsNames.WalkDown.ToString(), configuration.WalkDownAnimation)
                .AddAnimation(AnimationsNames.WalkRight.ToString(), configuration.WalkSideAnimation)
                .AddAnimation(AnimationsNames.WalkLeft.ToString(), configuration.WalkSideAnimation)
                .AddAnimation(AnimationsNames.IdleUp.ToString(), configuration.IdleUpAnimation)
                .AddAnimation(AnimationsNames.IdleDown.ToString(), configuration.IdleDownAnimation)
                .AddAnimation(AnimationsNames.IdleLeft.ToString(), configuration.IdleSideAnimation)
                .AddAnimation(AnimationsNames.IdleRight.ToString(), configuration.IdleSideAnimation);

            if (configuration.DieAnimation != null)
                MainAnimationsAnimationManager.AddAnimation(AnimationsNames.Die.ToString(), configuration.DieAnimation);

            CurrentCharacterState = CharacterState.IdleDown;
            MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleDown.ToString());

            if (configuration.ShadowWhileWalkingDownAnimation != null)
            {
                ShadowsAnimationManager = new AnimationsManager()
                    .AddAnimation(ShadowsAnimationsNames.WhileWalkingUp.ToString(), configuration.ShadowWhileWalkingUpAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileWalkingDown.ToString(), configuration.ShadowWhileWalkingDownAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileWalkingRight.ToString(), configuration.ShadowWhileWalkingSideAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileWalkingLeft.ToString(), configuration.ShadowWhileWalkingSideAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileIdleUp.ToString(), configuration.ShadowWhileIdleUpAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileIdleDown.ToString(), configuration.ShadowWhileIdleDownAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileIdleLeft.ToString(), configuration.ShadowWhileIdleSideAnimation)
                    .AddAnimation(ShadowsAnimationsNames.WhileIdleRight.ToString(), configuration.ShadowWhileIdleSideAnimation);

                _shadowsDrawingInfos = new DrawingInfos();
                PlayShadowFor(ShadowsAnimationsNames.WhileIdleDown);
            }

            if (configuration.CharacterTorchConfiguration != null)
            {
                AreLightsActive = true;
                _torchPositionTraslationUp = configuration.CharacterTorchConfiguration.TorchPositionTraslationUp;
                _torchPositionTraslationLeft = configuration.CharacterTorchConfiguration.TorchPositionTraslationLeft;
                _torchPositionTraslationRight = configuration.CharacterTorchConfiguration.TorchPositionTraslationRight;
                _torchPositionTraslationDown = configuration.CharacterTorchConfiguration.TorchPositionTraslationDown;
                _lightMap = new Light(
                    configuration.CharacterTorchConfiguration.LightMapSprite,
                    configuration.CharacterTorchConfiguration.FirstTorchColor,
                    configuration.CharacterTorchConfiguration.SecondTorchColor,
                    configuration.CharacterTorchConfiguration.TorchScale);
                _torch = new Light(
                    configuration.CharacterTorchConfiguration.TorchSprite,
                    configuration.CharacterTorchConfiguration.FirstTorchColor,
                    configuration.CharacterTorchConfiguration.SecondTorchColor,
                    configuration.CharacterTorchConfiguration.TorchScale);
                _lightMap.SetPosition(MainFigureDrawingInfos.Position);
                _torchPositionTraslation = _torchPositionTraslationDown;
                SetLightPosition();
            }

            _bloodEffect = new BloodParticleSystem(assetsLoader);
        }

        public void SetTemporaryColor(Color color)
        {
            _lightMap.SetTemporaryColorForAnimation(color);
            _torch?.SetTemporaryColorForAnimation(color);
        }

        public void ResetNormalColor()
        {
            _lightMap.ResetColor();
            _torch?.ResetColor();
        }

        public Vector2 TorchPosition
            => _torch.Position;

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
            _soundManager.PlayBlood();
            _bloodEffect.AddParticles(WorldCenter);
            CurrentCharacterState = CharacterState.Dying;
            MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.Die.ToString());
        }

        public void DieForTrap(Vector2 trapCenter)
        {
            _characterFallingATrapAnimation = new CharacterFallingInTrapAnimation(
                MainAnimationsAnimationManager.GetAnimation(AnimationsNames.IdleDown.ToString()).FirstFrameSprite,
                trapCenter,
                this,
                _game);

            CurrentCharacterState = CharacterState.Dying;

            if (AreLightsActive)
                SetLightPosition();
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

                    break;

                case CharacterActions.Down:
                    MainFigureDrawingInfos.IsFlipped = false;
                    if (!DontPlayDefaultAnimations)
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleDown.ToString());
                    ShadowsAnimationManager?.PlayAnimation(ShadowsAnimationsNames.WhileIdleDown.ToString());
                    _torchPositionTraslation = _torchPositionTraslationDown;

                    break;

                case CharacterActions.Right:
                    MainFigureDrawingInfos.IsFlipped = false;
                    if (!DontPlayDefaultAnimations)
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleRight.ToString());
                    ShadowsAnimationManager?.PlayAnimation(ShadowsAnimationsNames.WhileIdleRight.ToString());
                    _torchPositionTraslation = _torchPositionTraslationRight;

                    break;

                case CharacterActions.Left:
                    MainFigureDrawingInfos.IsFlipped = true;
                    if (!DontPlayDefaultAnimations)
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleLeft.ToString());
                    ShadowsAnimationManager?.PlayAnimation(ShadowsAnimationsNames.WhileIdleLeft.ToString());
                    _torchPositionTraslation = _torchPositionTraslationLeft;

                    break;
            }
        }

        private void PlayShadowFor(ShadowsAnimationsNames shadowsAnimationName)
        {
            if (ShadowsAnimationManager == null)
                return;

            ShadowsAnimationManager.PlayAnimation(shadowsAnimationName.ToString());

            var traslationUpRight = new Vector2(-32, 0);
            var traslationUpRightFlipped = new Vector2(32, 0);
            var traslationUpLeft = new Vector2(32, 32);

            _shadowsDrawingInfos.Origin = ShadowsAnimationManager.CurrentAnimation.FirstFrameSprite.SpriteCenter;

            switch (shadowsAnimationName)
            {
                case ShadowsAnimationsNames.WhileWalkingUp:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = traslationUpLeft;
                    break;

                case ShadowsAnimationsNames.WhileWalkingDown:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = traslationUpRight;
                    break;

                case ShadowsAnimationsNames.WhileWalkingLeft:
                    _shadowsDrawingInfos.IsFlipped = true;
                    _shadowPositionTraslation = traslationUpRightFlipped;
                    break;

                case ShadowsAnimationsNames.WhileWalkingRight:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = traslationUpRight;
                    break;

                case ShadowsAnimationsNames.WhileIdleUp:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = traslationUpLeft;
                    break;

                case ShadowsAnimationsNames.WhileIdleDown:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = traslationUpRight;
                    break;

                case ShadowsAnimationsNames.WhileIdleLeft:
                    _shadowsDrawingInfos.IsFlipped = true;
                    _shadowPositionTraslation = traslationUpRightFlipped;
                    break;

                case ShadowsAnimationsNames.WhileIdleRight:
                    _shadowsDrawingInfos.IsFlipped = false;
                    _shadowPositionTraslation = traslationUpRight;
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
                _startingLerpAmount = 0f;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationUp;

                PlayShadowFor(ShadowsAnimationsNames.WhileWalkingUp);
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
                _startingLerpAmount = 0f;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationDown;

                PlayShadowFor(ShadowsAnimationsNames.WhileWalkingDown);
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
                _startingLerpAmount = 0f;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationRight;

                PlayShadowFor(ShadowsAnimationsNames.WhileWalkingRight);
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
                _startingLerpAmount = 0f;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationLeft;

                PlayShadowFor(ShadowsAnimationsNames.WhileWalkingLeft);
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
                                CurrentCharacterState = CharacterState.IdleLeft;
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
                            _startingLerpAmount = 1f - _movementLerpAmount;
                            _movementLerpAmount = 0f;
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
                            _startingLerpAmount = 1f - _movementLerpAmount;
                            _movementLerpAmount = 0f;
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
                            _startingLerpAmount = 1f - _movementLerpAmount;
                            _movementLerpAmount = 0f;
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
                            _startingLerpAmount = 1f - _movementLerpAmount;
                            _movementLerpAmount = 0f;
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
                    _movementLerpAmount = MathHelper.Clamp(
                        _movementLerpAmount + Speed * (float)elapsed.TotalSeconds,
                        _startingLerpAmount,
                        1f);

                    MainFigureDrawingInfos.Position = Vector2.Lerp(_startingLocation, _nextLocation, _movementLerpAmount);

                    if (_movementLerpAmount >= 1f)
                        MainFigureDrawingInfos.Position = _nextLocation;
                }

                SetShadowPosition();
            }

            if (IsDying)
            {
                if (_characterFallingATrapAnimation != null)
                {
                    _characterFallingATrapAnimation.Update(elapsed);
                }
                else
                {
                    _bloodEffect.Update(elapsed);

                    if (MainAnimationsAnimationManager.CurrentAnimation.HasFinishedPlaying || !_bloodEffect.HasActiveParticles)
                    {
                        CurrentCharacterState = CharacterState.Dead;
                    }
                }
            }

            MainAnimationsAnimationManager.Update(elapsed);
            ShadowsAnimationManager?.Update(elapsed);

            if (AreLightsActive)
            {
                SetLightPosition();
                _lightMap.Update(elapsed);
                _torch?.Update(elapsed);
            }

            if (_characterFallingATrapAnimation == null)
                RecalculateZIndex();
        }

        private void RecalculateZIndex()
        {
            MainFigureDrawingInfos.LayerDepth = ZIndexCalculator.ForCharacter(
                this,
                Position);

            if (_shadowsDrawingInfos != null)
                _shadowsDrawingInfos.LayerDepth = ZIndexCalculator.ForGenericObject(_shadowsDrawingInfos.Position - _shadowsDrawingInfos.Origin);
            if (_torch != null)
                _torch.DrawingInfos.LayerDepth = MainFigureDrawingInfos.LayerDepth + 0.00001f;
        }

        protected void SetLightPosition(float lerpAmount = 0.2f)
        {
            var newLightPosition = WorldCenter + _torchPositionTraslation;
            _lightMap.SetPosition(Vector2.Lerp(_lightMap.Position, newLightPosition, lerpAmount));
            _torch?.SetPosition(newLightPosition);
        }

        protected void SetShadowPosition()
        {
            if (_shadowsDrawingInfos != null)
            {
                _shadowsDrawingInfos.Position = WorldCenter + _shadowPositionTraslation;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_characterFallingATrapAnimation != null)
            {
                _characterFallingATrapAnimation.Draw(spriteBatch);
                return;
            }

            if (!IsDead && !IsDying)
            {
                ShadowsAnimationManager?.Draw(spriteBatch, _shadowsDrawingInfos);
            }

            MainAnimationsAnimationManager.Draw(spriteBatch, MainFigureDrawingInfos);

            if (AreLightsActive && !IsDead)
            {
                _torch?.Draw(spriteBatch);
            }
        }

        public virtual void DrawEffects(SpriteBatch spriteBatch)
        {
            if (CurrentCharacterState == CharacterState.Dying)
            {
                _bloodEffect.Draw(spriteBatch);
            }
        }

        public virtual void DrawLightMap(SpriteBatch spriteBatch)
        {
            if (AreLightsActive && (!IsDead || this is Player))
            {
                _lightMap.Draw(spriteBatch);
            }
        }

    }
}
