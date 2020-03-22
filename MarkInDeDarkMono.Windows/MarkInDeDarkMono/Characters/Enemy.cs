using FbonizziMonoGame;
using MarkInDeDarkMono.AI;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDarkMono.Characters
{
    public class Enemy : Character
    {
        public IEnemyAI CurrentAI { get; private set; }

        private TimeSpan _screamTimer;
        private readonly int _screamSoundIndex;
        private readonly int _attackSoundIndex;

        private readonly SoundManager _soundManager;
        private readonly Player _player;

        public int NCellsCandleFindingRange { get; }

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
            AssetsLoader assetsLoader)
            : base(grid, soundManager, assetsLoader, characterBasicConfiguration)
        {
            _soundManager = soundManager;
            _player = player;
            NCellsCandleFindingRange = enemyConfiguration.NCellsFindingRange;
            SetRandomScreamTimer();

            AnimationsManager
                .AddAnimation(AnimationsNames.AttackUp.ToString(), enemyConfiguration.AttackUpAnimation)
                .AddAnimation(AnimationsNames.AttackDown.ToString(), enemyConfiguration.AttackDownAnimation)
                .AddAnimation(AnimationsNames.AttackLeft.ToString(), enemyConfiguration.AttackLeftAnimation)
                .AddAnimation(AnimationsNames.AttackRight.ToString(), enemyConfiguration.AttackRightAnimation);

            _attackSoundIndex = enemyConfiguration.AttackSoundIndex;
            _screamSoundIndex = enemyConfiguration.ScreamSoundIndex;
            CurrentAI = enemyConfiguration.StartingAI;
        }

        public void SetRandomScreamTimer()
            => _screamTimer = TimeSpan.FromSeconds(Numbers.RandomBetween(5, 8));

        public CharacterActions SetNewAI(IEnemyAI newAI)
        {
            CurrentAI = newAI;
            return CharacterActions.Idle;
        }

        public void Attack()
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.GoingUp:
                case CharacterState.IdleUp:
                    CurrentCharacterState = CharacterState.Attacking;
                    AnimationsManager.PlayAnimation(AnimationsNames.AttackUp.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    PlayAttackSound();
                    break;

                case CharacterState.GoingDown:
                case CharacterState.IdleDown:
                    CurrentCharacterState = CharacterState.Attacking;
                    AnimationsManager.PlayAnimation(AnimationsNames.AttackDown.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    PlayAttackSound();
                    break;

                case CharacterState.GoingLeft:
                case CharacterState.IdleLeft:
                    CurrentCharacterState = CharacterState.Attacking;
                    AnimationsManager.PlayAnimation(AnimationsNames.AttackLeft.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    PlayAttackSound();
                    break;

                case CharacterState.GoingRight:
                case CharacterState.IdleRight:
                    CurrentCharacterState = CharacterState.Attacking;
                    AnimationsManager.PlayAnimation(AnimationsNames.AttackRight.ToString());
                    _soundManager.ComputeIndexSoundsVolume(
                        _attackSoundIndex,
                        _screamSoundIndex,
                        Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
                    break;

            }
        }

        public void StopAttacking()
        {
            if (CurrentCharacterState != CharacterState.Dying && CurrentCharacterState != CharacterState.Dead)
            {
                StopAttackSound();
                SetDirectionAnimation(CharacterActions.Down);
            }
        }

        private void PlayMyVoice()
        {
            if (_soundManager.ComputeIndexSoundsVolume(
                _screamSoundIndex,
                _attackSoundIndex,
                Vector2.DistanceSquared(_player.WorldCenter, WorldCenter)) >= 0.1f)
            {
                _soundManager.PlayIndexedSound(_screamSoundIndex);
            }
        }

        private void StopMyVoice()
        {
            _soundManager.StopIndexedSound(_screamSoundIndex);
        }

        private void StopAttackSound()
        {
            _soundManager.StopIndexedSound(_attackSoundIndex);
        }

        private void PlayAttackSound()
        {
            _soundManager.PlayIndexedSound(_attackSoundIndex);
        }

        public void HitPlayer()
            => CurrentAI = new IdleAI();

        public override void Update(TimeSpan elapsed)
        {
            // Rimodulo sempre il volume dell'attacco per fare il fade
            // quando il player si allontana.
            if (AnimationsManager.CurrentAnimationName.Contains("Attack"))
            {
                _soundManager.ComputeIndexSoundsVolume(
                    _screamSoundIndex,
                    _attackSoundIndex,
                    Vector2.DistanceSquared(_player.WorldCenter, WorldCenter));
            }

            _screamTimer -= elapsed;
            if (_screamTimer <= TimeSpan.Zero)
            {
                PlayMyVoice();
                SetRandomScreamTimer();
            }

            if (!(_player.IsDead || _player.IsDying)
                && !_player.HasWon
                && CollisionRectangle.Intersects(_player.CollisionRectangle))
            {
                Attack();
                HitPlayer();
                _player.Die();
                CurrentAI = new IdleAI();
            }

            if (!IsMoving)
            {
                base.HandleInput(CurrentAI.Update(this, elapsed));
            }

            base.Update(elapsed);
        }
    }
}
