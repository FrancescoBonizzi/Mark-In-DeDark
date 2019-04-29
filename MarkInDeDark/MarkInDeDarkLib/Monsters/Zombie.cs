#region Using

using Mark.Engine.AI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class Zombie : Enemy
    {
        private List<Cell> path_;

        public Zombie(List<Cell> Path)
            : base(Path[0].WorldLocation)
        {
            this.light_.OverlayColor = new Color(139, 238, 90);
            this.light_.Scale = 0.5f;
            this.pixel_light_.Animate(Color.White, this.light_.OverlayColor);
            this.speed_ = 1.7f;

            this.path_ = Path;

            this.light_position_traslation_down_ = new Vector2(0, -10);
            this.light_position_traslation_up_ = new Vector2(0, -20);
            this.light_position_traslation_right_ = new Vector2(0, -10);
            this.light_position_traslation_left_ = new Vector2(0, -10);

            this.light_position_traslation_ = light_position_traslation_right_;
            this.current_state_ = new PatrolAreaState(path_);

            this.attack_sound_index_ = GameReferencesLocator.SoundManagerReference.RegisterZombieAttackSound();
            this.scream_sound_index_ = GameReferencesLocator.SoundManagerReference.RegisterZombieScreamSound();
        }

        protected override void LoadEveryAnimation()
        {
            this.frame_width_ = Loader.zombie_down_[0].Width;
            this.frame_heigth_ = Loader.zombie_down_[0].Height;
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);

            LoadAnimation("su", Loader.zombie_up_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giu", Loader.zombie_down_, true, 0.2f, Loader.atlas_);
            LoadAnimation("lato", Loader.zombie_side_, true, 0.2f, Loader.atlas_);
            LoadAnimation("suF", Loader.zombie_up_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuF", Loader.zombie_down_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoF", Loader.zombie_side_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("suAttack", Loader.zombie_up_attack_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuAttack", Loader.zombie_down_attack_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoAttack", Loader.zombie_side_attack_, true, 0.2f, Loader.atlas_);
         
            PlayAnimation("su");
        }

        public List<Cell> Path
        {
            get { return path_; }
        }
    }
}
