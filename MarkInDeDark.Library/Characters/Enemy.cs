using FbonizziMonoGame;
using MarkInDeDark.Library.AI;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework;
using System;

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

            CurrentDieAnimationType = DieAnimationType.Explosion;
            ShouldDoExplosionAnimation = true;

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

        public CharacterActions SetNewAI(IEnemyAI newAI)
        {
            CurrentAI = newAI;
            return CharacterActions.Idle;
        }

        public virtual void CustomActionOnPlayerDead()
        {

        }

        public void Attack(Player player = null)
        {
            // Se il player non è null, il mostro si deve rivolgere a lui
            if (player != null)
            {
                // No idle perché se il player è fermo è il mostro ad andargli addosso
                switch(player.CurrentCharacterState)
                {
                    case CharacterState.GoingUp:
                        CurrentCharacterState = CharacterState.Attacking;
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackDown.ToString());
                        _soundManager.ComputeIndexSoundsVolume(
                            _attackSoundIndex,
                            _screamSoundIndex,
                            Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                        //PlayAttackSound();
                        return;

                    case CharacterState.GoingDown:
                        CurrentCharacterState = CharacterState.Attacking;
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackUp.ToString());
                        _soundManager.ComputeIndexSoundsVolume(
                            _attackSoundIndex,
                            _screamSoundIndex,
                            Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                        //PlayAttackSound();
                        return;

                    case CharacterState.GoingLeft:
                        CurrentCharacterState = CharacterState.Attacking;
                        SetDirectionAnimation(CharacterActions.Right);
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackRight.ToString());
                        _soundManager.ComputeIndexSoundsVolume(
                            _attackSoundIndex,
                            _screamSoundIndex,
                            Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                        //PlayAttackSound();
                        return;

                    case CharacterState.GoingRight:
                        CurrentCharacterState = CharacterState.Attacking;
                        SetDirectionAnimation(CharacterActions.Left);
                        MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackLeft.ToString());
                        _soundManager.ComputeIndexSoundsVolume(
                            _attackSoundIndex,
                            _screamSoundIndex,
                            Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                        return;
                }
            }

            // Se il player è null, attacca in base a dove stava guardando
            switch (CurrentCharacterState)
            {
                case CharacterState.GoingUp:
                case CharacterState.IdleUp:
                    CurrentCharacterState = CharacterState.Attacking;
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackUp.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    //PlayAttackSound();
                    break;

                case CharacterState.GoingDown:
                case CharacterState.IdleDown:
                    CurrentCharacterState = CharacterState.Attacking;
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackDown.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    //PlayAttackSound();
                    break;

                case CharacterState.GoingLeft:
                case CharacterState.IdleLeft:
                    CurrentCharacterState = CharacterState.Attacking;
                    MainAnimationsAnimationManager.PlayAnimation(AnimationsNames.AttackLeft.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    //PlayAttackSound();
                    break;

                case CharacterState.GoingRight:
                case CharacterState.IdleRight:
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
                SetDirectionAnimation(CharacterActions.Down);
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
            SetNewAI(new KillCandleAI(possibleCandle, Array.Empty<CharacterActions>(), CurrentAI));
            CurrentAI.Update(this, elapsed);
        }

        public override void Update(TimeSpan elapsed)
        {
            if (_gameFinished)
                return;

            // Se incontra una candela, la spegne all'istante
            if (_killsCandlesIfCollidesWithThem
                && !(CurrentAI is ChargeDirectionAI))
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
            //if (MainAnimationsAnimationManager.CurrentAnimationName.Contains("Attack"))
            //{
            //    _soundManager.ComputeIndexSoundsVolume(
            //        _screamSoundIndex,
            //        _attackSoundIndex,
            //        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
            //}

            //_screamTimer -= elapsed;
            //if (_screamTimer <= TimeSpan.Zero)
            //{
            //    //PlayMyVoice();
            //    SetRandomScreamTimer();
            //}

            if (!(_player.IsDead || _player.IsDying)
                && !_player.HasWon
                && !IsDying
                && !IsDead
                && CustomConditionToAttackPlayer()
                && CollisionRectangle.Intersects(_player.CollisionRectangle))
            {
                CustomActionOnPlayerDead();
                Attack(_player);
                HitPlayer();
                _player.Die();
                CurrentAI = new IdleAI();
                _stopMovement = true;
            }

            if (!IsMoving)
            {
                base.HandleInput(CurrentAI.Update(this, elapsed));
            }

            base.Update(elapsed);
        }
    }
}
