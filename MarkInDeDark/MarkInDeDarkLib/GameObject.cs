#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public abstract class GameObject
    {
        #region Dichiarazioni

        protected Vector2 world_location_;
        
        protected int frame_width_;
        protected int frame_heigth_;

        // Sono privati perchè quando modifico il bool modifico anche lo spriteeffect nel metodo set
        private bool flipped_;
        protected SpriteEffects effect_;

        protected float draw_depth_;
        protected Color overlay_color_;
        protected Vector2 origin_;
        protected Vector2 frame_center_;
        protected float scale_;
        protected float rotation_;

        protected Dictionary<string, Animation> animations_;
        protected string current_animation_;

        protected int collision_rectangle_threshold_;

        #endregion

        #region Costruttore

        public GameObject()
        {
            flipped_ = false;
            effect_ = SpriteEffects.None;
            draw_depth_ = 1.0f;
            overlay_color_ = Color.White;
            origin_ = Vector2.Zero;
            scale_ = 1.0f;
            rotation_ = 0.0f;
            animations_ = new Dictionary<string, Animation>();
            collision_rectangle_threshold_ = 0;
        
            LoadEveryAnimation();
        }

        protected abstract void LoadEveryAnimation();

        #endregion

        #region Proprietà

        public Vector2 WorldLocation
        {
            get { return world_location_; }
            set { world_location_ = value; }
        }

        public float Scale
        {
            get { return scale_; }
            set { scale_ = value; }
        }

        public bool Flipped
        {
            get { return flipped_; }
            set
            {
                flipped_ = value;
                if (value)
                {
                    effect_ = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    effect_ = SpriteEffects.None;
                }
            }
        }

        public Vector2 WorldCenter
        {
            get
            {
                return new Vector2(
                    (int)world_location_.X + frame_center_.X,
                    (int)world_location_.Y + frame_center_.Y);
            }
        }

        public Rectangle WorldRectangle
        {
            get
            {
                return new Rectangle(
                    (int)world_location_.X,
                    (int)world_location_.Y,
                    frame_width_,
                    frame_heigth_);
            }
        }

        public Rectangle CollisionRectangle
        {
            get
            {
                return new Rectangle(
                    (int)world_location_.X + collision_rectangle_threshold_,
                    (int)world_location_.Y + collision_rectangle_threshold_,
                    frame_width_ - collision_rectangle_threshold_,
                    frame_heigth_ - collision_rectangle_threshold_);
            }
        }

        public virtual bool IsCollidingWith(Rectangle OtherObjectCollisionRectangle)
        {
            return (OtherObjectCollisionRectangle.Intersects(CollisionRectangle));
        }

        public Color OverlayColor
        {
            get { return overlay_color_; }
            set { overlay_color_ = value; }
        }

        #endregion

        #region Animazioni

        private void updateAnimation(float dt)
        {
            if (current_animation_ != null)
            {
                if (animations_.ContainsKey(current_animation_))
                {
                    if (animations_[current_animation_].FinishedPlaying)
                    {
                        PlayAnimation(animations_[current_animation_].NextAnimation);
                    }
                    else
                    {
                        animations_[current_animation_].Update(dt);
                    }
                }
            }
        }

        public void PlayAnimation(string name)
        {
            if (!(name == null) && animations_.ContainsKey(name))
            {
                current_animation_ = name;
                animations_[name].Play();
            }
        }

        protected void LoadAnimation(
           string Name,
           List<Rectangle> FrameList,
           bool LoopAnimation,
           float FrameLength,
           Texture2D TextureReference)
        {
            animations_.Add(
               Name,
               new Animation(
                    FrameList,
                    Name,
                    TextureReference
               ));
            animations_[Name].LoopAnimation = LoopAnimation;
            animations_[Name].FrameLength = FrameLength;
        }

        #endregion

        #region Update/Draw

        public virtual void Update(float elapsed)
        {
            updateAnimation(elapsed);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (current_animation_ != null)
            {
                if (animations_.ContainsKey(current_animation_))
                {
                    spriteBatch.Draw(
                        animations_[current_animation_].Texture,
                        Camera.WorldToScreen(world_location_),
                        animations_[current_animation_].FrameRectangle,
                        overlay_color_, rotation_, origin_, scale_, effect_, draw_depth_);
                }
            }
        }

        #endregion
    }
}
