#region Using

using Mark.Engine.AI;
using Mark.Engine.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class Enemy : Character
    {
        #region Dichiarazioni

        public EnemyAI current_state_;
        private float voice_timer_;
        protected int scream_sound_index_;
        protected int attack_sound_index_;

        #endregion

        #region Costruttore

        public Enemy(Vector2 StartingPosition)
            : base(StartingPosition)
        {
            this.speed_ = 1.2f;
            this.light_.WorldLocation = StartingPosition;
            SetRandomVoiceTimer();
        }

        protected override void LoadEveryAnimation()
        {
        }

        #endregion

        #region Metodi

        public virtual void Attack()
        {
            if (!current_animation_.Contains("Attack"))
            {
                current_animation_ += "Attack";


                GameReferencesLocator.SoundManagerReference.ComputeIndexSoundsVolume(attack_sound_index_, scream_sound_index_,
                   Vector2.DistanceSquared(GameReferencesLocator.PlayerReference.WorldCenter, this.WorldCenter));
                PlayAttackSound();
            }
        }

        public virtual void StopAttacking()
        {
            int index = current_animation_.IndexOf("Attack");
            current_animation_ =
                (index < 0) ? current_animation_ : current_animation_.Remove(index, "Attack".Length);

            StopAttackSound();
        }

        private void SetRandomVoiceTimer()
        {
            voice_timer_ = Helper.random_.Next(3, 6);
        }

        protected void PlayMyVoice()
        {
            if (GameReferencesLocator.SoundManagerReference.ComputeIndexSoundsVolume(scream_sound_index_, attack_sound_index_,
                Vector2.DistanceSquared(GameReferencesLocator.PlayerReference.WorldCenter, this.WorldCenter)) >= 0.1f)
                GameReferencesLocator.SoundManagerReference.PlayIndexedSound(scream_sound_index_);
        }

        protected void StopMyVoice()
        {
            GameReferencesLocator.SoundManagerReference.StopIndexedSound(scream_sound_index_);
        }

        protected void StopAttackSound()
        {
            GameReferencesLocator.SoundManagerReference.StopIndexedSound(attack_sound_index_);
        }

        protected void PlayAttackSound()
        {
            GameReferencesLocator.SoundManagerReference.PlayIndexedSound(attack_sound_index_);
        }

        public void HitPlayer()
        {
            if (current_state_ is HuntingPlayerState)
                return;

            // Lo faccio perchè se per caso il player ci va contro mentre magari lui sta inseguendo
            // una candela, o facendo altro, non continua per la sua strada ma si gira verso di lui

            current_state_ = new IdleState();
        }

        protected virtual void PersonalAction()
        {
            if (current_animation_.Contains("Attack"))
                GameReferencesLocator.SoundManagerReference.ComputeIndexSoundsVolume(attack_sound_index_, scream_sound_index_,
                Vector2.DistanceSquared(GameReferencesLocator.PlayerReference.WorldCenter, this.WorldCenter));
        }

        #endregion

        #region Update/Draw

        public virtual void Update(float elapsed, GameGrid g)
        {
            PersonalAction();
            InputManager.InputAction ActionToDo;
            ActionToDo = current_state_.Update(this, elapsed);

            voice_timer_ -= elapsed;
            if (voice_timer_ <= 0.0f)
            {
                PlayMyVoice();
                SetRandomVoiceTimer();
            }

            base.Update(elapsed, g, ActionToDo);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        #endregion
    }
}
