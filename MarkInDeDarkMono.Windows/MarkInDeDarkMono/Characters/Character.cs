using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Levels;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Characters
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

        public CharacterState CurrentCharacterState { get; protected set; }

        public bool IsDead => CurrentCharacterState == CharacterState.Dead;
        public bool IsDying => CurrentCharacterState == CharacterState.Dying;
        private readonly Light _lightMap;
        private readonly Light _torch;

        public bool AreLightsActive { get; set; }

        public bool IsMoving => _movementLerpAmount < 1f && !IsDead && !IsDying;
        public float Speed { get; set; }

        protected DrawingInfos _drawingInfos;

        private readonly GameGrid _grid;
        private readonly SoundManager _soundManager;
        private float _movementLerpAmount;
        private float _startingLerpAmount;
        private Vector2 _nextLocation;
        private Vector2 _startingLocation;

        private Vector2 _torchPositionTraslation;
        private Vector2 _torchPositionTraslationUp;
        private Vector2 _torchPositionTraslationLeft;
        private Vector2 _torchPositionTraslationRight;
        private Vector2 _torchPositionTraslationDown;

        private CharacterActions? _nextActionToProcess;
        private BloodParticleSystem _bloodEffect;

        public AnimationsManager AnimationsManager { get; }

        public Character(
            GameGrid grid,
            SoundManager soundManager,
            AssetsLoader assetsLoader,
            CharacterConfiguration configuration)
        {
            _startingLerpAmount = 0f;
            _movementLerpAmount = 1.0f;

            _grid = grid;
            _soundManager = soundManager;
            AreLightsActive = true;

            _torchPositionTraslationUp = configuration.TorchPositionTraslationUp;
            _torchPositionTraslationLeft = configuration.TorchPositionTraslationLeft;
            _torchPositionTraslationRight = configuration.TorchPositionTraslationRight;
            _torchPositionTraslationDown = configuration.TorchPositionTraslationDown;

            _drawingInfos = new DrawingInfos()
            {
                Position = configuration.StartingPosition,
                HitBoxTolerance = new Rectangle(50, 50, 50, 50),
                Origin = configuration.WalkUpAnimation.FirstFrameSprite.SpriteCenter
            };

            _nextLocation = Position;

            _lightMap = new Light(configuration.LightMapSprite, configuration.FirstTorchColor, configuration.SecondTorchColor, configuration.TorchScale);
            _torch = new Light(configuration.TorchSprite, configuration.FirstTorchColor, configuration.SecondTorchColor, configuration.TorchScale);
            Speed = configuration.MovementSpeed;

            _lightMap.SetPosition(_drawingInfos.Position);

            AnimationsManager = new AnimationsManager()
                .AddAnimation(AnimationsNames.WalkUp.ToString(), configuration.WalkUpAnimation)
                .AddAnimation(AnimationsNames.WalkDown.ToString(), configuration.WalkDownAnimation)
                .AddAnimation(AnimationsNames.WalkRight.ToString(), configuration.WalkSideAnimation)
                .AddAnimation(AnimationsNames.WalkLeft.ToString(), configuration.WalkSideAnimation)
                .AddAnimation(AnimationsNames.IdleUp.ToString(), configuration.IdleUpAnimation)
                .AddAnimation(AnimationsNames.IdleDown.ToString(), configuration.IdleDownAnimation)
                .AddAnimation(AnimationsNames.IdleLeft.ToString(), configuration.IdleSideAnimation)
                .AddAnimation(AnimationsNames.IdleRight.ToString(), configuration.IdleSideAnimation)
                .AddAnimation(AnimationsNames.Die.ToString(), configuration.DieAnimation);

            CurrentCharacterState = CharacterState.IdleDown;
            AnimationsManager.PlayAnimation(AnimationsNames.IdleDown.ToString());
            _torchPositionTraslation = _torchPositionTraslationDown;
            SetLightPosition();

            _bloodEffect = new BloodParticleSystem(assetsLoader);
        }

        public void SetTemporaryColor(Color color)
        {
            _lightMap.SetTemporaryColor(color);
            _torch.SetTemporaryColor(color);
        }

        public void ResetNormalColor()
        {
            _lightMap.ResetColor();
            _torch.ResetColor();
        }

        public Vector2 WorldCenter
            => _drawingInfos.Center(
                AnimationsManager.CurrentAnimation.CurrentFrameWidth,
                AnimationsManager.CurrentAnimation.CurrentFrameHeight);

        public Vector2 Position
            => _drawingInfos.Position;

        public Rectangle CollisionRectangle
            => _drawingInfos.HitBox(
                AnimationsManager.CurrentAnimation.CurrentFrameWidth,
                AnimationsManager.CurrentAnimation.CurrentFrameWidth);

        public float Height
            => AnimationsManager.CurrentAnimation.CurrentFrameHeight;

        public float Width
            => AnimationsManager.CurrentAnimation.CurrentFrameWidth;

        public virtual void Die()
        {
            _grid.ApplyBloodDecal(WorldCenter);
            _soundManager.PlayBlood();
            _bloodEffect.AddParticles(WorldCenter);
            CurrentCharacterState = CharacterState.Dying;
            AnimationsManager.PlayAnimation(AnimationsNames.Die.ToString());
        }

        public void DieForTrap(Vector2 trapPosition)
        {
            Die();
            _drawingInfos.Position = trapPosition;
            _drawingInfos.Scale = 0.8f;
        }

        public void SetDirectionAnimation(CharacterActions direction)
        {
            switch (direction)
            {
                case CharacterActions.Up:
                    _drawingInfos.IsFlipped = false;
                    AnimationsManager.PlayAnimation(AnimationsNames.IdleUp.ToString());
                    _torchPositionTraslation = _torchPositionTraslationUp;

                    break;

                case CharacterActions.Down:
                    _drawingInfos.IsFlipped = false;
                    AnimationsManager.PlayAnimation(AnimationsNames.IdleDown.ToString());
                    _torchPositionTraslation = _torchPositionTraslationDown;

                    break;

                case CharacterActions.Right:
                    _drawingInfos.IsFlipped = false;
                    AnimationsManager.PlayAnimation(AnimationsNames.IdleRight.ToString());
                    _torchPositionTraslation = _torchPositionTraslationRight;

                    break;

                case CharacterActions.Left:
                    _drawingInfos.IsFlipped = true;
                    AnimationsManager.PlayAnimation(AnimationsNames.IdleLeft.ToString());
                    _torchPositionTraslation = _torchPositionTraslationLeft;

                    break;
            }
        }

        private void GoNextCellUp(GameCell currentCell)
        {
            if (currentCell.IsNorthPassable)
            {
                CurrentCharacterState = CharacterState.GoingUp;
                AnimationsManager.PlayAnimation(AnimationsNames.WalkUp.ToString());
                _startingLocation = _drawingInfos.Position;
                _drawingInfos.IsFlipped = false;
                _nextLocation = currentCell.NorthCell.CenterPosition;
                _startingLerpAmount = 0f;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationUp;
            }
        }

        private void GoNextCellDown(GameCell currentCell)
        {
            if (currentCell.IsSouthPassable)
            {
                CurrentCharacterState = CharacterState.GoingDown;
                AnimationsManager.PlayAnimation(AnimationsNames.WalkDown.ToString());
                _startingLocation = _drawingInfos.Position;
                _drawingInfos.IsFlipped = false;
                _nextLocation = currentCell.SouthCell.CenterPosition;
                _startingLerpAmount = 0f;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationDown;
            }
        }

        private void GoNextCellRight(GameCell currentCell)
        {
            if (currentCell.IsEastPassable)
            {
                CurrentCharacterState = CharacterState.GoingRight;
                AnimationsManager.PlayAnimation(AnimationsNames.WalkRight.ToString());
                _drawingInfos.IsFlipped = false;
                _startingLocation = _drawingInfos.Position;
                _nextLocation = currentCell.EastCell.CenterPosition;
                _startingLerpAmount = 0f;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationRight;
            }
        }

        private void GoNextCellLeft(GameCell currentCell)
        {
            if (currentCell.IsWestPassable)
            {
                CurrentCharacterState = CharacterState.GoingLeft;
                AnimationsManager.PlayAnimation(AnimationsNames.WalkLeft.ToString());
                _drawingInfos.IsFlipped = true;
                _startingLocation = _drawingInfos.Position;
                _nextLocation = currentCell.WestCell.CenterPosition;
                _startingLerpAmount = 0f;
                _movementLerpAmount = 0f;
                _torchPositionTraslation = _torchPositionTraslationLeft;
            }
        }

        private void HandleBufferedInput()
        {
            var currentCell = _grid.GetCellAtPixel(_drawingInfos.Position);

            if (!IsMoving)
            {
                switch (_nextActionToProcess)
                {
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
                                AnimationsManager.PlayAnimation(AnimationsNames.IdleUp.ToString());
                                break;

                            case CharacterState.GoingDown:
                                CurrentCharacterState = CharacterState.IdleDown;
                                AnimationsManager.PlayAnimation(AnimationsNames.IdleDown.ToString());
                                break;

                            case CharacterState.GoingLeft:
                                CurrentCharacterState = CharacterState.IdleLeft;
                                AnimationsManager.PlayAnimation(AnimationsNames.IdleLeft.ToString());
                                break;

                            case CharacterState.GoingRight:
                                CurrentCharacterState = CharacterState.IdleLeft;
                                AnimationsManager.PlayAnimation(AnimationsNames.IdleRight.ToString());
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
                            AnimationsManager.PlayAnimation(AnimationsNames.WalkDown.ToString());
                            Vector2 tmp = _nextLocation;
                            _nextLocation = _startingLocation;
                            _startingLocation = tmp;
                            _startingLerpAmount = 1f - _movementLerpAmount;
                            _movementLerpAmount = 0f;
                            _torchPositionTraslation = _torchPositionTraslationDown;
                        }
                        break;

                    case CharacterState.GoingDown:
                        if (_nextActionToProcess == CharacterActions.Up)
                        {
                            CurrentCharacterState = CharacterState.GoingUp;
                            AnimationsManager.PlayAnimation(AnimationsNames.WalkUp.ToString());
                            Vector2 tmp = _nextLocation;
                            _nextLocation = _startingLocation;
                            _startingLocation = tmp;
                            _startingLerpAmount = 1f - _movementLerpAmount;
                            _movementLerpAmount = 0f;
                            _torchPositionTraslation = _torchPositionTraslationUp;
                        }
                        break;

                    case CharacterState.GoingLeft:
                        if (_nextActionToProcess == CharacterActions.Right)
                        {
                            CurrentCharacterState = CharacterState.GoingRight;
                            AnimationsManager.PlayAnimation(AnimationsNames.WalkRight.ToString());
                            _drawingInfos.IsFlipped = false;
                            Vector2 tmp = _nextLocation;
                            _nextLocation = _startingLocation;
                            _startingLocation = tmp;
                            _startingLerpAmount = 1f - _movementLerpAmount;
                            _movementLerpAmount = 0f;
                            _torchPositionTraslation = _torchPositionTraslationRight;
                        }
                        break;

                    case CharacterState.GoingRight:
                        if (_nextActionToProcess == CharacterActions.Left)
                        {
                            CurrentCharacterState = CharacterState.GoingLeft;
                            AnimationsManager.PlayAnimation(AnimationsNames.WalkLeft.ToString());
                            _drawingInfos.IsFlipped = true;
                            Vector2 tmp = _nextLocation;
                            _nextLocation = _startingLocation;
                            _startingLocation = tmp;
                            _startingLerpAmount = 1f - _movementLerpAmount;
                            _movementLerpAmount = 0f;
                            _torchPositionTraslation = _torchPositionTraslationLeft;
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

                if (IsMoving)
                {
                    _movementLerpAmount = MathHelper.Clamp(
                        _movementLerpAmount + Speed * (float)elapsed.TotalSeconds,
                        _startingLerpAmount,
                        1f);

                    _drawingInfos.Position = Vector2.Lerp(_startingLocation, _nextLocation, _movementLerpAmount);

                    if (_movementLerpAmount >= 1f)
                        _drawingInfos.Position = _nextLocation;

                }
                SetLightPosition();
            }

            if (IsDying)
            {
                _bloodEffect.Update(elapsed);

                if (AnimationsManager.CurrentAnimation.HasFinishedPlaying || !_bloodEffect.HasActiveParticles)
                {
                    CurrentCharacterState = CharacterState.Dead;
                }
            }

            AnimationsManager.Update(elapsed);
            _lightMap.Update(elapsed);
            _torch.Update(elapsed);
        }

        protected void SetLightPosition(float lerpAmount = 0.2f)
        {
            var newLightPosition = WorldCenter + _torchPositionTraslation;
            _lightMap.SetPosition(Vector2.Lerp(_lightMap.Position, newLightPosition, lerpAmount));
            _torch.SetPosition(newLightPosition);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            AnimationsManager.Draw(spriteBatch, _drawingInfos);

            if (AreLightsActive)
            {
                _torch.Draw(spriteBatch);
            }

            if (CurrentCharacterState == CharacterState.Dying)
            {
                _bloodEffect.Draw(spriteBatch);
            }
        }

        public virtual void DrawLightMap(SpriteBatch spriteBatch)
        {
            if (AreLightsActive)
            {
                _lightMap.Draw(spriteBatch);
            }
        }

    }
}
