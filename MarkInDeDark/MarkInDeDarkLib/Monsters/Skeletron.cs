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
    public class Skeletron : Enemy
    {
        private List<Cell> path_;

        public Skeletron(List<Cell> Path)
            : base(Path[0].WorldLocation)
        {
            this.light_.OverlayColor = new Color(196, 118, 240);
            this.light_.Scale = 0.5f;
            this.pixel_light_.Animate(Color.White, this.light_.OverlayColor);
            this.speed_ = 1.5f;

            this.path_ = Path;

            this.light_position_traslation_down_ = new Vector2(20, 0);
            this.light_position_traslation_up_ = new Vector2(-20, -15);
            this.light_position_traslation_right_ = new Vector2(20, -5);
            this.light_position_traslation_left_ = new Vector2(-20, -5);

            this.light_position_traslation_ = light_position_traslation_right_;
            this.current_state_ = new PatrolAreaState(path_);

            this.attack_sound_index_ = GameReferencesLocator.SoundManagerReference.RegisterSkeletronAttackSound();
            this.scream_sound_index_ = GameReferencesLocator.SoundManagerReference.RegisterSkeletronScreamSound();
        }

        protected override void LoadEveryAnimation()
        {
            this.frame_width_ = Loader.skeletron_down_[0].Width;
            this.frame_heigth_ = Loader.skeletron_down_[0].Height;
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);

            LoadAnimation("su", Loader.skeletron_up_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giu", Loader.skeletron_down_, true, 0.2f, Loader.atlas_);
            LoadAnimation("lato", Loader.skeletron_side_, true, 0.2f, Loader.atlas_);
            LoadAnimation("suF", Loader.skeletron_up_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuF", Loader.skeletron_down_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoF", Loader.skeletron_side_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("suAttack", Loader.skeletron_up_attack_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuAttack", Loader.skeletron_down_attack_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoAttack", Loader.skeletron_side_attack_, true, 0.2f, Loader.atlas_);
           
            PlayAnimation("su");
        }
    }
}
