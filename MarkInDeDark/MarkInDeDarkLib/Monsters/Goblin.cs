#region Using

using Mark.Engine.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class Goblin : Enemy
    {
        private const float min_speed_ = 1.0f;
        private float max_speed_;

        public Goblin(Vector2 StartingPosition)
            : base(StartingPosition)
        {
            this.light_.OverlayColor = new Color(202, 240, 118);
            this.light_.Scale = 0.5f;
            this.pixel_light_.Animate(Color.White, this.light_.OverlayColor);
            this.speed_ = min_speed_;

            this.light_position_traslation_down_ = new Vector2(-20, 5);
            this.light_position_traslation_up_ = new Vector2(20, -15);
            this.light_position_traslation_right_ = new Vector2(-5, 5);
            this.light_position_traslation_left_ = new Vector2(5, 5);

            this.light_position_traslation_ = light_position_traslation_right_;
            this.max_speed_ = GameReferencesLocator.PlayerReference.Speed;
            this.current_state_ = new HuntingPlayerState();

            this.attack_sound_index_ = GameReferencesLocator.SoundManagerReference.RegisterGoblinAttackSound();
            this.scream_sound_index_ = GameReferencesLocator.SoundManagerReference.RegisterGoblinScreamSound();
        }

        protected override void LoadEveryAnimation()
        {
            this.frame_width_ = Loader.goblin_down_[0].Width;
            this.frame_heigth_ = Loader.goblin_down_[0].Height;
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);

            LoadAnimation("su", Loader.goblin_up_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giu", Loader.goblin_down_, true, 0.2f, Loader.atlas_);
            LoadAnimation("lato", Loader.goblin_side_, true, 0.2f, Loader.atlas_);
            LoadAnimation("suF", Loader.goblin_up_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuF", Loader.goblin_down_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoF", Loader.goblin_side_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("suAttack", Loader.goblin_up_attack_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuAttack", Loader.goblin_down_attack_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoAttack", Loader.goblin_side_attack_, true, 0.2f, Loader.atlas_);

            PlayAnimation("su");
        }

        protected override void PersonalAction()
        {
            base.PersonalAction();
            this.speed_ = MathHelper.Clamp(min_speed_ + (GameReferencesLocator.CandlesReference.PlayerUsedCandles / 10.0f), min_speed_, max_speed_);
        }

     
    }
}
