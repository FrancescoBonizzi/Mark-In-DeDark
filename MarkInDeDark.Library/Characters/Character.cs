using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Levels;
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
            Idle,
            Moving,
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

        public CharacterState CurrentCharacterState { get; set; }

        protected bool _canGoOverWalls = false;

        public bool IsDead => CurrentCharacterState == CharacterState.Dead;
        public bool IsDying => CurrentCharacterState == CharacterState.Dying;
        private readonly Light _lightMap;
        protected Light _torch;

        public bool AreLightsActive { get; set; }

        public float Speed { get; set; }

        protected bool _stopMovement = false;

        public DrawingInfos MainFigureDrawingInfos { get; set; }
        protected DrawingInfos _shadowsDrawingInfos;

        protected readonly GameGrid _gameGrid;
        private readonly SoundManager _soundManager;
        private readonly MarkInDeDarkGame _game;

        private Vector2 _torchPositionTraslation;
        private Vector2 _torchPositionTraslationUp;
        private Vector2 _torchPositionTraslationLeft;
        private Vector2 _torchPositionTraslationRight;
        private Vector2 _torchPositionTraslationDown;

        private Vector2 _shadowPositionTraslation;
        private Vector2 _shadowPositionTraslationUp;
        private Vector2 _shadowPositionTraslationLeft;
        private Vector2 _shadowPositionTraslationRight;
        private Vector2 _shadowPositionTraslationDown;
        private Vector2 _shadowPositionTraslationDownIdle;
        private CharacterActions _previousAction;

        public CharacterActions CurrentAction { get; private set; }

        public AnimationsManager MainAnimationsAnimationManager { get; }
        public AnimationsManager ShadowsAnimationManager { get; }

        private CharacterFallingInTrapAnimation _characterFallingATrapAnimation;
        private Vector2? _deadPosition;

        public float CurrentZIndex => MainFigureDrawingInfos.LayerDepth;

        public bool DontPlayDefaultAnimations { get; set; } = false;

        protected TimeSpan _deadBodyPartLifetime = TimeSpan.FromSeconds(3);
        protected bool _deadBodyEffectWithFading = true;

        protected CharacterTorchParticleSystem _dieMarkEffect;
        private readonly CharacterExplosionParticleSystem _characterDieExplosionParticleSystem;

        public Character(
            GameGrid grid,
            SoundManager soundManager,
            AssetsLoader assetsLoader,
            CharacterConfiguration configuration,
            MarkInDeDarkGame game)
        {
            _game = game;

            _gameGrid = grid;
            _soundManager = soundManager;

            MainFigureDrawingInfos = new DrawingInfos()
            {
                Position = configuration.StartingPosition,
                HitBoxTolerance = new Rectangle(50, 50, 100, 100),
                Origin = configuration.WalkUpAnimation.FirstFrameSprite.SpriteCenter
            };

            Speed = configuration.MovementSpeed;

            if (configuration.BodyParts != null)
            {
                _characterDieExplosionParticleSystem = new CharacterExplosionParticleSystem(configuration);
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

                _shadowsDrawingInfos = new DrawingInfos();
            }

            CurrentAction = CharacterActions.Idle;
            _previousAction = CharacterActions.Down;
            SetDirectionAnimation(CharacterActions.Idle);

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
                _dieMarkEffect = new CharacterTorchParticleSystem(
                    assetsLoader,
                    configuration.CharacterTorchConfiguration.FirstTorchColor,
                    configuration.CharacterTorchConfiguration.SecondTorchColor);
            }
        }

        public void SetTemporaryColor(Color color)
        {
            _lightMap.SetTemporaryColorForAnimation(color);
            _torch?.SetTemporaryColorForAnimation(color);
        }

        // TODO Si può mettere nell'enemy
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
            //_soundManager.PlayBlood();
            CurrentCharacterState = CharacterState.Dying;
            _characterDieExplosionParticleSystem.Explode(
                WorldCenter,
                ZIndexCalculator.OverAllLayerDepth,
                _deadBodyPartLifetime,
                _deadBodyEffectWithFading);


            if (_dieMarkEffect != null)
            {
                _dieMarkEffect.LayerDepth = ZIndexCalculator.ForGenericObject(Position);
            }
        }

        public void DieForTrap(Vector2 trapCenter)
        {
            _deadPosition = trapCenter + new Vector2(0, 35f);
            _characterFallingATrapAnimation = new CharacterFallingInTrapAnimation(
                MainAnimationsAnimationManager.GetAnimation(AnimationsNames.IdleDown.ToString()).FirstFrameSprite,
                trapCenter,
                this,
                _game);

            CurrentCharacterState = CharacterState.Dying;

            if (AreLightsActive)
            {
                SetLightPosition();
            }
        }

        private void SetThingsWhenFacing(CharacterActions characterAction)
        {
            switch (characterAction)
            {
                case CharacterActions.Up:
                    MainFigureDrawingInfos.IsFlipped = false;
                    _torchPositionTraslation = _torchPositionTraslationUp;
                    break;

                case CharacterActions.Down:
                    MainFigureDrawingInfos.IsFlipped = false;
                    _torchPositionTraslation = _torchPositionTraslationDown;
                    break;

                case CharacterActions.Right:
                    MainFigureDrawingInfos.IsFlipped = false;
                    _torchPositionTraslation = _torchPositionTraslationRight;
                    break;

                case CharacterActions.Left:
                    MainFigureDrawingInfos.IsFlipped = true;
                    _torchPositionTraslation = _torchPositionTraslationLeft;
                    break;
            }
        }

        public void SetDirectionAnimation(CharacterActions direction)
        {
            switch (direction)
            {
                case CharacterActions.Idle:

                    switch (_previousAction)
                    {
                        case CharacterActions.Up:
                            if (!DontPlayDefaultAnimations)
                                MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleUp.ToString());

                            PlayShadowFor(ShadowsAnimationsNames.WhileIdleUp);
                            break;

                        case CharacterActions.Down:
                            if (!DontPlayDefaultAnimations)
                                MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleDown.ToString());

                            PlayShadowFor(ShadowsAnimationsNames.WhileIdleDown);
                            break;

                        case CharacterActions.Right:
                            if (!DontPlayDefaultAnimations)
                                MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleRight.ToString());

                            PlayShadowFor(ShadowsAnimationsNames.WhileIdleRight);
                            break;

                        case CharacterActions.Left:
                            if (!DontPlayDefaultAnimations)
                                MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.IdleLeft.ToString());

                            PlayShadowFor(ShadowsAnimationsNames.WhileIdleLeft);
                            break;
                    }

                    break;

                case CharacterActions.Up:
                    SetThingsWhenFacing(direction);
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkUp.ToString());
                    PlayShadowFor(ShadowsAnimationsNames.WhileWalkingUp);
                    break;

                case CharacterActions.Down:
                    SetThingsWhenFacing(direction);
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkDown.ToString());
                    PlayShadowFor(ShadowsAnimationsNames.WhileWalkingDown);
                    break;

                case CharacterActions.Left:
                    SetThingsWhenFacing(direction);
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkLeft.ToString());
                    PlayShadowFor(ShadowsAnimationsNames.WhileWalkingLeft);
                    break;

                case CharacterActions.Right:
                    SetThingsWhenFacing(direction);
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.WalkRight.ToString());
                    PlayShadowFor(ShadowsAnimationsNames.WhileWalkingRight);
                    break;
            }

            SetShadowPosition();
        }

        private void PlayShadowFor(ShadowsAnimationsNames shadowsAnimationName)
        {
            if (ShadowsAnimationManager == null)
            {
                return;
            }

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

        public static Vector2 GetDirectionToPosition(Vector2 from, Vector2 to)
        {
            var difference = (to - from);

            // Altrimenti Normalize ritorna NaN
            if (difference == Vector2.Zero)
                return difference;

            difference.Normalize();
            return difference;
        }

        public static CharacterActions GetActionForDirection(Vector2 normalizedMovement)
        {
            normalizedMovement.X = (float)Math.Round(normalizedMovement.X);
            normalizedMovement.Y = (float)Math.Round(normalizedMovement.Y);

            if (normalizedMovement.X > 0)
                return CharacterActions.Right;
            if (normalizedMovement.X < 0)
                return CharacterActions.Left;
            if (normalizedMovement.Y < 0)
                return CharacterActions.Up;
            if (normalizedMovement.Y > 0)
                return CharacterActions.Down;

            return CharacterActions.Idle;
        }

        public bool CanMove(Vector2 direction)
        {
            if (_canGoOverWalls)
                return true;

            // TODO In base alla direzione devo anche capire 
            // se dove sto andando c'è un muro in mezzo alle celle
            var currentCell = _gameGrid.GetCellAtPixel(WorldCenter);

            if (currentCell == null)
                return false;

            var characterWorldCenter = WorldCenter;
            const float verticalMovementMargin = 50f;
            const float horizontalMovementMargin = 80f;

            bool canMove = true;

            if (direction.Y < 0)
            {
                bool isOnCellBorder = characterWorldCenter.Y <= currentCell.TopLeftPosition.Y + verticalMovementMargin;
                if (isOnCellBorder)
                    canMove = canMove && currentCell.IsNorthPassable && currentCell.IsNorthDoorPassable && !currentCell.NorthCell.IsUnwalkable;
            }

            if (direction.Y > 0)
            {
                bool isOnCellBorder = characterWorldCenter.Y >= currentCell.BottomLeftPosition.Y - verticalMovementMargin;
                if (isOnCellBorder)
                    canMove = canMove && currentCell.IsSouthPassable && currentCell.IsSouthDoorPassable && !currentCell.SouthCell.IsUnwalkable;
            }

            if (direction.X > 0)
            {
                bool isOnCellBorder = characterWorldCenter.X >= currentCell.TopRightPosition.X - horizontalMovementMargin;
                if (isOnCellBorder)
                    canMove = canMove && currentCell.IsEastPassable && currentCell.IsEastDoorPassable && !currentCell.EastCell.IsUnwalkable;
                else return true;
            }

            if (direction.X < 0)
            {
                bool isOnCellBorder = characterWorldCenter.X <= currentCell.TopLeftPosition.X + horizontalMovementMargin;
                if (isOnCellBorder)
                    canMove = canMove && currentCell.IsWestPassable && currentCell.IsWestDoorPassable && !currentCell.WestCell.IsUnwalkable;
            }

            return canMove;
        }

        public virtual void Update(TimeSpan elapsed, Vector2 direction)
        {
            if (!IsDying && !IsDead
                && !_stopMovement
                && CurrentCharacterState != CharacterState.Attacking
                && direction != Vector2.Zero)
            {
                // Già normalizzato!
                // direction.Normalize();

                if (CanMove(direction))
                {
                    MainFigureDrawingInfos.Position += Speed * (float)elapsed.TotalSeconds * direction;
                    CurrentCharacterState = CharacterState.Moving;
                }
                else
                {
                    CurrentCharacterState = CharacterState.Idle;
                }

                _previousAction = CurrentAction;
                CurrentAction = GetActionForDirection(direction);
                SetDirectionAnimation(CurrentAction);
            }

            if (direction == Vector2.Zero)
            {
                _previousAction = CurrentAction;
                CurrentAction = CharacterActions.Idle;
                SetDirectionAnimation(CurrentAction);
            }

            if (IsDying)
            {
                if (_characterFallingATrapAnimation != null)
                {
                    _characterFallingATrapAnimation.Update(elapsed);
                }
                else
                {
                    if (_characterDieExplosionParticleSystem != null)
                    {
                        _characterDieExplosionParticleSystem.Update(elapsed);
                        if (_characterDieExplosionParticleSystem.HasFinished)
                        {
                            CurrentCharacterState = CharacterState.Dead;
                        }
                    }
                    else
                    {
                        CurrentCharacterState = CharacterState.Dead;
                    }
                }
            }

            if (_characterDieExplosionParticleSystem == null
                || !_characterDieExplosionParticleSystem.HasStarted)
            {
                MainAnimationsAnimationManager.Update(elapsed);
                ShadowsAnimationManager?.Update(elapsed);
            }

            if (AreLightsActive)
            {
                SetLightPosition();
                _lightMap.Update(elapsed);
                _torch?.Update(elapsed);
            }

            if (_characterFallingATrapAnimation == null)
            {
                RecalculateZIndex();
            }
        }

        public virtual void UpdateAfterDeath(TimeSpan elapsed)
        {
            _dieMarkEffect?.AddParticles(_deadPosition ?? WorldCenter);
            _dieMarkEffect?.Update(elapsed);
        }

        protected virtual void RecalculateZIndex()
        {
            MainFigureDrawingInfos.LayerDepth = ZIndexCalculator.ForCharacter(
                this,
                Position);

            if (_shadowsDrawingInfos != null)
            {
                _shadowsDrawingInfos.LayerDepth = ZIndexCalculator.ForGenericObject(_shadowsDrawingInfos.Position - _shadowsDrawingInfos.Origin);
            }

            if (_torch != null)
            {
                _torch.DrawingInfos.LayerDepth = MainFigureDrawingInfos.LayerDepth + 0.00001f;
            }
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

                if (AreLightsActive)
                {
                    _torch?.Draw(spriteBatch);
                }
            }

            if (_characterDieExplosionParticleSystem != null)
            {
                _characterDieExplosionParticleSystem.Draw(spriteBatch);
            }

            if (_characterDieExplosionParticleSystem == null
                || !_characterDieExplosionParticleSystem.HasStarted)
            {
                MainAnimationsAnimationManager.Draw(spriteBatch, MainFigureDrawingInfos);
            }

            if (CurrentCharacterState == CharacterState.Dead)
            {
                _dieMarkEffect?.Draw(spriteBatch);
            }
        }

        public virtual void DrawEffectsOverDark(SpriteBatch spriteBatch)
        {

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
