#region Using

using Mark.Engine.AI;
using Mark.Engine.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class Werewolf : Enemy
    {
        private ParticleSystemAnimation sparkle_ = null;
        protected bool disappeared_;

        public Werewolf(Vector2 StartingPosition)
            : base(StartingPosition)
        {
            this.light_.OverlayColor = new Color(240, 137, 118);
            this.light_.Scale = 0.5f;
            this.pixel_light_.Animate(Color.White, this.light_.OverlayColor);
            this.speed_ = 0.5f;

            this.light_position_traslation_down_ = new Vector2(-25, 10);
            this.light_position_traslation_up_ = new Vector2(25, 10);
            this.light_position_traslation_right_ = new Vector2(0, 25);
            this.light_position_traslation_left_ = new Vector2(0, 25);

            this.light_.Active = false;
            this.pixel_light_.Active = false;

            this.light_position_traslation_ = light_position_traslation_right_;

            this.disappeared_ = true;
            this.current_state_ = new GhostState(this);

            this.attack_sound_index_ = GameReferencesLocator.SoundManagerReference.RegisterWolfAttackSound();
            this.scream_sound_index_ = GameReferencesLocator.SoundManagerReference.RegisterWolfScreamSound();
        }

        protected override void LoadEveryAnimation()
        {
            this.frame_width_ = Loader.werewolf_down_[0].Width;
            this.frame_heigth_ = Loader.werewolf_down_[0].Height;
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);

            LoadAnimation("su", Loader.werewolf_up_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giu", Loader.werewolf_down_, true, 0.2f, Loader.atlas_);
            LoadAnimation("lato", Loader.werewolf_side_, true, 0.2f, Loader.atlas_);
            LoadAnimation("suF", Loader.werewolf_up_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuF", Loader.werewolf_down_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoF", Loader.werewolf_side_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("suAttack", Loader.werewolf_up_attack_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuAttack", Loader.werewolf_down_attack_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoAttack", Loader.werewolf_side_attack_, true, 0.2f, Loader.atlas_);
      
            PlayAnimation("su");
        }

        public bool SparkleActive()
        {
            if (sparkle_ != null)
                return !sparkle_.Finished;

            return false;
        }

        public void AppearEffect()
        {
            if (disappeared_)
            {
                disappeared_ = false;
                this.sparkle_ = new ParticleSystemAnimation(base.WorldCenter, new SparkleParticleSystem());
                // La luce non va riattivata perchè deve apparire al buio
            }
            else
            {
                disappeared_ = true;
                this.light_.Active = false;
            }
        }

        public override bool IsCollidingWith(Rectangle OtherObjectCollisionRectangle)
        {
            if (disappeared_)
                return false;

            return base.IsCollidingWith(OtherObjectCollisionRectangle);
        }

        public override void Update(float elapsed, GameGrid g)
        {
            #region Sparkle

            if (sparkle_ != null)
            {
                sparkle_.Update(elapsed);

                if (sparkle_.Finished)
                {
                    light_.WorldLocation = base.WorldCenter + this.light_position_traslation_;
                    pixel_light_.WorldLocation = base.WorldCenter + this.light_position_traslation_;
                    light_.Animate(new Color(240, 137, 118), new Color(240, 137, 118));
                    pixel_light_.Animate(Color.White, new Color(240, 137, 118));
                    light_.Active = true;
                    pixel_light_.Active = true;

                    sparkle_ = null;
                }

                return;
            }

            #endregion

            base.Update(elapsed, g);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (!disappeared_)
                base.Draw(spriteBatch);

            if (sparkle_ != null)
                sparkle_.Draw(spriteBatch);
        }

        public void DrawSparkles(SpriteBatch spriteBatch)
        {
            if (sparkle_ != null)
                sparkle_.DrawSparkleLightmap(spriteBatch);
        }
    }
}
