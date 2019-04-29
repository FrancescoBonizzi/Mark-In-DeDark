#region Using

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
    public class Candle : GameObject
    {
        #region Dichiarazioni

        private Light light_;
        private Light pixel_light_;

        private bool activate_ttl_;
        private float ttl_ = 2.2f; // s

        private ParticleSystemAnimation smoke_ = null;
        private bool some_monster_is_following_me_ = false;

        #endregion

        #region Costruttore

        public Candle(Vector2 Location)
        {
            this.world_location_ = Location;
            this.light_ = new Light();
            this.light_.WorldLocation = this.WorldCenter;
            this.light_.Animate(new Color(255, 195, 163), new Color(255, 247, 207));
            this.collision_rectangle_threshold_ = 50;

            this.pixel_light_ = new Light(Loader.light_pixel_);
            this.pixel_light_.Animate(Color.White, new Color(255, 195, 163));
            this.pixel_light_.WorldLocation = new Vector2(this.WorldCenter.X, this.WorldCenter.Y - 15);

            this.activate_ttl_ = false;
        }

        protected override void LoadEveryAnimation()
        {
            this.frame_width_ = Loader.candle_[0].Width;
            this.frame_heigth_ = Loader.candle_[0].Height;
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);

            LoadAnimation("fire", Loader.candle_, true, 0.2f, Loader.atlas_);

            PlayAnimation("fire");
        }

        #endregion

        #region Metodi

        public void GoingToDie()
        {
            activate_ttl_ = true;
            // Quando ci sono più mostri sulla candela, fa più in fretta a finire
            ttl_ -= 0.2f;

            if (smoke_ == null)
                smoke_ = new ParticleSystemAnimation(
                    this.WorldCenter - new Vector2(0.0f, 15.0f),
                    new SmokeParticleSystem());
        }

        #endregion

        #region Get/Set

        public Light MyLight
        {
            get { return light_; }
        }

        public bool Active
        {
            get { return ttl_ > 0.0f; }
        }

        public bool Dying
        {
            get { return activate_ttl_; }
        }

        public bool SomeMonsterIsFollowingMe
        {
            get { return some_monster_is_following_me_; }
            set { some_monster_is_following_me_ = value; }
        }

        #endregion

        #region Update

        public override void Update(float elapsed)
        {
            if (activate_ttl_)
                ttl_ -= elapsed;

            if (smoke_ != null)
                smoke_.Update(elapsed);

            light_.Update(elapsed);
            pixel_light_.Update(elapsed);
            base.Update(elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            pixel_light_.Draw(spriteBatch);
            base.Draw(spriteBatch);
        }

        public void DrawSmoke(SpriteBatch spriteBatch)
        {
            if (smoke_ != null)
                smoke_.Draw(spriteBatch);
        }

        #endregion
    }
}
