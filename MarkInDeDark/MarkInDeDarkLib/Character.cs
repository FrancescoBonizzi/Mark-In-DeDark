#region Using

using Mark.Engine.AI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public abstract class Character : GameObject
    {
        #region Dichiarazioni

        protected bool dead_ = false;

        protected float speed_;
        protected float lerp_amount_;

        private Vector2 next_location_;
        private Vector2 starting_location_;

        protected Light light_;
        protected Light pixel_light_;
        protected Vector2 light_position_traslation_;

        protected Vector2 light_position_traslation_up_;
        protected Vector2 light_position_traslation_left_;
        protected Vector2 light_position_traslation_right_;
        protected Vector2 light_position_traslation_down_;

        protected Texture2D atlas_;

        private float starting_lerp_value_ = 0.0f;
        protected InputManager.InputAction current_action_;

        #endregion

        #region Costruttore

        public Character(
            Vector2 StartingPosition)
            : base()
        {
            this.world_location_ = StartingPosition;
            this.light_ = new Light();
            this.speed_ = 1.0f;
            this.lerp_amount_ = 1.0f;
            this.light_position_traslation_ = Vector2.Zero;
            this.pixel_light_ = new Light(Loader.light_pixel_);
            this.pixel_light_.WorldLocation = light_.WorldLocation + light_position_traslation_;
            this.collision_rectangle_threshold_ = 50;

            this.atlas_ = Loader.atlas_;
        }

        #endregion

        #region Get/Set

        public InputManager.InputAction CurrentAction
        {
            get { return current_action_; }
        }

        public Light MyLight
        {
            get { return light_; }
        }

        public bool Dead
        {
            get { return dead_; }
            set { dead_ = value; }
        }

        public bool IsMoving
        {
            get { return lerp_amount_ < 1.0f; }
        }

        public float Speed
        {
            get { return speed_; }
            set { speed_ = value; }
        }

        #endregion

        #region Metodi

        public virtual void Die()
        {
            PlayAnimation("die");
            dead_ = true;
        }

        #endregion

        #region Update

        public virtual void Update(float elapsed, GameGrid g, InputManager.InputAction ActionPressed)
        {
            string new_animation_ = this.current_animation_;
    
            if (!this.IsMoving)
            {
                #region Movimento di base

                Cell current_cell = g.GetCellAtPixel(this.world_location_);
                if (ActionPressed == InputManager.InputAction.Su)
                {
                    if (!this.IsMoving)
                    {
                        if (current_cell.IsNorthPassable)
                        {
                            if (!this.current_animation_.Contains("Attack"))
                            {
                                new_animation_ = "su";
                                this.light_position_traslation_ = this.light_position_traslation_up_;
                            }
                            base.Flipped = false;
                            this.starting_location_ = this.world_location_;
                            this.next_location_ = current_cell.Links[0].WorldLocation;
                            this.lerp_amount_ = 0f;
                            this.starting_lerp_value_ = 0f;
                            this.current_action_ = ActionPressed;
                        }
                    }
                }
                else if (ActionPressed == InputManager.InputAction.Destra)
                {
                    if (!this.IsMoving)
                    {
                        if (current_cell.IsEastPassable)
                        {
                            if (!this.current_animation_.Contains("Attack"))
                            {
                                new_animation_ = "lato";
                                this.light_position_traslation_ = this.light_position_traslation_right_;
                            }
                            base.Flipped = false;
                            this.starting_location_ = this.world_location_;
                            this.next_location_ = current_cell.Links[1].WorldLocation;
                            this.lerp_amount_ = 0f;
                            this.starting_lerp_value_ = 0f;
                            this.current_action_ = ActionPressed;
                        }
                    }
                }
                else if (ActionPressed == InputManager.InputAction.Giu)
                {
                    if (!this.IsMoving)
                    {
                        if (current_cell.IsSouthPassable)
                        {
                            if (!this.current_animation_.Contains("Attack"))
                            {
                                new_animation_ = "giu";
                                this.light_position_traslation_ = this.light_position_traslation_down_;
                            }
                            base.Flipped = false;
                            this.starting_location_ = this.world_location_;
                            this.next_location_ = current_cell.Links[2].WorldLocation;
                            this.lerp_amount_ = 0f;
                            this.starting_lerp_value_ = 0f;
                            this.current_action_ = ActionPressed;
                        }
                    }
                }
                else if (ActionPressed == InputManager.InputAction.Sinistra)
                {
                    if (!this.IsMoving)
                    {
                        if (current_cell.IsWestPassable)
                        {
                            if (!this.current_animation_.Contains("Attack"))
                            {
                                new_animation_ = "lato";
                                this.light_position_traslation_ = this.light_position_traslation_left_;
                            }
                            base.Flipped = true;
                            this.starting_location_ = this.world_location_;
                            this.next_location_ = current_cell.Links[3].WorldLocation;
                            this.lerp_amount_ = 0f;
                            this.starting_lerp_value_ = 0f;
                            this.current_action_ = ActionPressed;
                        }
                    }
                }
                else if (new_animation_ != "die" && !new_animation_.Contains("Attack"))
                {
                    new_animation_ = this.current_animation_ + "F";
                }

                #endregion
            }
            else
            {
                #region Reverse del movimento

                switch (this.current_action_)
                {
                    case InputManager.InputAction.Su:
                        if (ActionPressed == InputManager.InputAction.Giu)
                        {
                            Vector2 tmp = this.next_location_;
                            this.next_location_ = this.starting_location_;
                            this.starting_location_ = tmp;
                            this.starting_lerp_value_ = 1f - this.lerp_amount_;
                            this.lerp_amount_ = 0f;
                            this.current_action_ = ActionPressed;
                        }
                        break;
                    case InputManager.InputAction.Giu:
                        if (ActionPressed == InputManager.InputAction.Su)
                        {
                            Vector2 tmp = this.next_location_;
                            this.next_location_ = this.starting_location_;
                            this.starting_location_ = tmp;
                            this.starting_lerp_value_ = 1f - this.lerp_amount_;
                            this.lerp_amount_ = 0f;
                            this.current_action_ = ActionPressed;
                        }
                        break;
                    case InputManager.InputAction.Destra:
                        if (ActionPressed == InputManager.InputAction.Sinistra)
                        {
                            Vector2 tmp = this.next_location_;
                            this.next_location_ = this.starting_location_;
                            this.starting_location_ = tmp;
                            this.starting_lerp_value_ = 1f - this.lerp_amount_;
                            this.lerp_amount_ = 0f;
                            this.current_action_ = ActionPressed;
                        }
                        break;
                    case InputManager.InputAction.Sinistra:
                        if (ActionPressed == InputManager.InputAction.Destra)
                        {
                            Vector2 tmp = this.next_location_;
                            this.next_location_ = this.starting_location_;
                            this.starting_location_ = tmp;
                            this.starting_lerp_value_ = 1f - this.lerp_amount_;
                            this.lerp_amount_ = 0f;
                            this.current_action_ = ActionPressed;
                        }
                        break;
                }

                #endregion
            }

            #region Calcolo della posizione

            if (new_animation_ != this.current_animation_)
            {
                base.PlayAnimation(new_animation_);
            }

            if (this.IsMoving)
            {
                this.lerp_amount_ = MathHelper.Clamp(this.lerp_amount_ + this.speed_ * elapsed, this.starting_lerp_value_, 1f);
                this.world_location_ = Vector2.Lerp(this.starting_location_, this.next_location_, this.lerp_amount_);
                if (this.lerp_amount_ >= 1f)
                {
                    this.world_location_ = new Vector2((float)((int)this.world_location_.X), (float)((int)this.world_location_.Y));
                }
            }

            this.light_.WorldLocation = Vector2.Lerp(this.light_.WorldLocation, base.WorldCenter + this.light_position_traslation_, 0.2f);
            this.pixel_light_.WorldLocation = base.WorldCenter + this.light_position_traslation_; 
            this.light_.Update(elapsed);
            this.pixel_light_.Update(elapsed);
            base.Update(elapsed);

            #endregion
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            // Disattivo il lumino alla morte 
            if (!dead_)
                pixel_light_.Draw(spriteBatch);
        }

        #endregion
    }
}
