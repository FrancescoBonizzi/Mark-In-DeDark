using FbonizziMonoGame;
using MarkInDeDark.Library.AI;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Characters
{
    public class Enemy : Character
    {
        public IEnemyAI CurrentAI { get; private set; }

        private readonly bool _killsCandlesIfCollidesWithThem;
        private TimeSpan _screamTimer;
        private bool _gameFinished;
        private readonly int _screamSoundIndex;
        private readonly int _attackSoundIndex;

        private readonly SoundManager _soundManager;
        protected readonly Player _player;
        protected readonly CandlesManager _candlesManager;

        public int? NCellsCandleFindingRange { get; }

        private enum AnimationsNames
        {
            AttackUp,
            AttackDown,
            AttackLeft,
            AttackRight
        }

        public Enemy(
            GameGrid grid,
            CharacterConfiguration characterBasicConfiguration,
            EnemyConfiguration enemyConfiguration,
            SoundManager soundManager,
            Player player,
            AssetsLoader assetsLoader,
            MarkInDeDarkGame game,
            CandlesManager candlesManager)
            : base(grid, soundManager, assetsLoader, characterBasicConfiguration, game)
        {
            _candlesManager = candlesManager;
            _soundManager = soundManager;
            _player = player;
            NCellsCandleFindingRange = enemyConfiguration.NCellsFindingRange;
            SetRandomScreamTimer();

            MainAnimationsAnimationManager
                .AddAnimation(AnimationsNames.AttackUp.ToString(), enemyConfiguration.AttackUpAnimation)
                .AddAnimation(AnimationsNames.AttackDown.ToString(), enemyConfiguration.AttackDownAnimation)
                .AddAnimation(AnimationsNames.AttackLeft.ToString(), enemyConfiguration.AttackLeftAnimation)
                .AddAnimation(AnimationsNames.AttackRight.ToString(), enemyConfiguration.AttackRightAnimation);

            _attackSoundIndex = enemyConfiguration.AttackSoundIndex;
            _screamSoundIndex = enemyConfiguration.ScreamSoundIndex;
            CurrentAI = enemyConfiguration.StartingAI;
            _killsCandlesIfCollidesWithThem = enemyConfiguration.KillsCandlesIfCollidesWithThem;
        }

        public void SetRandomScreamTimer()
            => _screamTimer = TimeSpan.FromSeconds(Numbers.RandomBetween(5, 8));

        public void SetNewAI(IEnemyAI newAI)
        {
            CurrentAI = newAI;
        }

        public void Attack()
        {
            switch (CurrentAction)
            {
                case CharacterActions.Up:
                    CurrentCharacterState = CharacterState.Attacking;
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackUp.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    //PlayAttackSound();
                    break;

                case CharacterActions.Down:
                    CurrentCharacterState = CharacterState.Attacking;
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackDown.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    //PlayAttackSound();
                    break;

                case CharacterActions.Left:
                    CurrentCharacterState = CharacterState.Attacking;
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackLeft.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    //PlayAttackSound();
                    break;

                case CharacterActions.Right:
                    CurrentCharacterState = CharacterState.Attacking;
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackRight.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    break;

            }
        }

        public void GameFinished()
        {
            StopAttacking();
            _gameFinished = true;
        }

        public void StopAttacking()
        {
            if (CurrentCharacterState != CharacterState.Dying && CurrentCharacterState != CharacterState.Dead)
            {
                //StopAttackSound();
                CurrentCharacterState = CharacterState.Idle;
                SetDirectionAnimation(CharacterActions.Down);
                SetDirectionAnimation(CharacterActions.Idle);
            }
        }

        //private void PlayMyVoice()
        //{
        //    if (_soundManager.ComputeIndexSoundsVolume(
        //        _screamSoundIndex,
        //        _attackSoundIndex,
        //        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter)) >= 0.1f)
        //    {
        //        _soundManager.PlayIndexedSound(_screamSoundIndex);
        //    }
        //}

        //private void StopMyVoice()
        //{
        //    _soundManager.StopIndexedSound(_screamSoundIndex);
        //}

        //private void StopAttackSound()
        //{
        //    _soundManager.StopIndexedSound(_attackSoundIndex);
        //}

        //private void PlayAttackSound()
        //{
        //    _soundManager.PlayIndexedSound(_attackSoundIndex);
        //}

        public void HitPlayer()
            => CurrentAI = new IdleAI();

        public override void Die()
        {
            //StopAttackSound();
            base.Die();
            // Così quando muoiono in tanti sembrano più sparpagliati
            MainFigureDrawingInfos.Rotation = Numbers.RandomBetween(-1f, 1f);
        }

        public virtual bool CustomConditionToAttackPlayer()
        {
            return true;
        }

        protected virtual void KillCandleLogic(Candle possibleCandle, TimeSpan elapsed)
        {
            SetNewAI(new KillCandleAI(possibleCandle, new List<Vector2>(), CurrentAI));
        }

        public virtual void Update(TimeSpan elapsed)
        {
            if (_gameFinished)
                return;

            // Se incontra una candela, la spegne all'istante
            if (_killsCandlesIfCollidesWithThem)
            {
                if (!(CurrentAI is KillCandleAI))
                {
                    var currentCell = _gameGrid.GetCellAtPixel(Position);
                    var possibleCandle = _candlesManager.IsThereACandle(currentCell);
                    if (possibleCandle != null)
                    {
                        KillCandleLogic(possibleCandle, elapsed);
                    }
                }
            }

            // Rimodulo sempre il volume dell'attacco per fare il fade
            // quando il player si allontana.
            if (MainAnimationsAnimationManager.CurrentAnimationName.Contains("Attack"))
            {
                _soundManager.ComputeIndexSoundsVolume(
                    _screamSoundIndex,
                    _attackSoundIndex,
                    Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
            }

            _screamTimer -= elapsed;
            if (_screamTimer <= TimeSpan.Zero)
            {
                //PlayMyVoice();
                SetRandomScreamTimer();
            }

            if (!(_player.IsDead || _player.IsDying)
                && !_player.HasWon
                && CustomConditionToAttackPlayer()
                && CollisionRectangle.Intersects(_player.CollisionRectangle))
            {
                Attack();
                HitPlayer();
                _player.Die();
                CurrentAI = new IdleAI();
                _stopMovement = true;
            }

            base.Update(elapsed, CurrentAI.GetNextDirection(this, elapsed));
        }
    }
}
