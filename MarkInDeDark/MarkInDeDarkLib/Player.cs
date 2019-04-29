#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using Mark.Engine.Particles;

#endregion

namespace Mark.Engine
{
    public class Player : Character
    {
        #region Dichiarazioni

        private bool input_enabled_;
        private Vector2 camera_offset_ = new Vector2(200, 240);

        private PlayerRagDool ragdoll_ = null;
        private ParticleSystemAnimation effects_ = null;
        private bool won_ = false;

        #endregion

        #region Costruttore

        public Player(GameGrid g)
            : base(g.PlayerStartingPosition)
        {
            this.speed_ = 1.5f;
            this.input_enabled_ = false;
            this.light_.WorldLocation = g.PlayerStartingPosition;
            this.light_.Active = false;
            this.pixel_light_.Active = false;

            this.light_position_traslation_down_ = new Vector2(16, -20);
            this.light_position_traslation_up_ = new Vector2(-16, -20);
            this.light_position_traslation_right_ = new Vector2(20, -20);
            this.light_position_traslation_left_ = new Vector2(-20, -20);
            this.light_position_traslation_ = light_position_traslation_down_;

            RepositionCamera(); // Non togliere! Se no si sballa la posizione delle scintille
            this.effects_ = new ParticleSystemAnimation(
                base.WorldCenter + light_position_traslation_down_,
                new SparkleParticleSystem());
        }

        protected override void LoadEveryAnimation()
        {
            this.frame_width_ = Loader.p_easy_up_[0].Width;
            this.frame_heigth_ = Loader.p_easy_up_[0].Height;
            this.frame_center_ = new Vector2(this.frame_width_ / 2, this.frame_heigth_ / 2);

            LoadAnimation("su", Loader.p_easy_up_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giu", Loader.p_easy_down_, true, 0.2f, Loader.atlas_);
            LoadAnimation("lato", Loader.p_easy_side_, true, 0.15f, Loader.atlas_);
            LoadAnimation("suF", Loader.p_easy_up_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("latoF", Loader.p_easy_side_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("giuF", Loader.p_easy_down_idle_, true, 0.2f, Loader.atlas_);
            LoadAnimation("die", Loader.p_easy_dying_, false, 0.2f, Loader.atlas_);
            LoadAnimation("rotate", Loader.p_rotating_, true, 0.2f, Loader.atlas_);

            PlayAnimation("giuF");
        }

        #endregion

        #region Get/Set

        public bool InputEnabled
        {
            get { return input_enabled_; }
            set { input_enabled_ = value; }
        }

        #endregion

        #region Metodi

        public override void Die()
        {
            if (effects_ == null)
                effects_ = new ParticleSystemAnimation(base.WorldCenter, new BloodParticleSystem());
            this.lerp_amount_ = 1.1f; // Così blocco qualunque animazione in corso

            base.Die();
        }

        public void Win()
        {
            input_enabled_ = false;
            won_ = true;
        }

        private void RepositionCamera()
        {
            Camera.Position = world_location_ - camera_offset_;
        }

        private void RepositionCamera(Vector2 Position)
        {
            Camera.Position = Position - camera_offset_;
        }

        public bool EffectActive()
        {
            if (effects_ != null)
                return !effects_.Finished;

            return false;
        }

        #endregion

        #region Update / Draw

        public override void Update(float elapsed, GameGrid g, InputManager.InputAction InputActionPressed)
        {
            if (InputActionPressed == InputManager.InputAction.Candela)
            {
                // Non più di una nello stesso posto
                Vector2 cell_for_candle = g.GetCellAtPixel(WorldCenter).WorldLocation;
                GameReferencesLocator.CandlesReference.AddNewCandle(cell_for_candle);
            }

            #region Effects

            if (effects_ != null)
            {
                RepositionCamera();
                effects_.Update(elapsed);

                if (effects_.Finished)
                {
                    if (!dead_)
                    {
                        light_.WorldLocation = base.WorldCenter + this.light_position_traslation_;
                        pixel_light_.WorldLocation = base.WorldCenter + this.light_position_traslation_;
                        light_.Animate(new Color(255, 195, 163), new Color(255, 247, 207));
                        pixel_light_.Animate(Color.White, new Color(255, 195, 163));
                        light_.Active = true;
                        pixel_light_.Active = true;
                        input_enabled_ = true;
                        GameReferencesLocator.SoundManagerReference.PlayPlayerTorch();
                    }

                    effects_ = null;
                }

                return;
            }

            #endregion

            #region Ragdoll

            if (ragdoll_ != null)
            {
                RepositionCamera();
                light_.Active = false;
                pixel_light_.Active = false;
                ragdoll_.Update(elapsed);

                return;
            }

            #endregion

            #region Update normale del player

            // L'ho messo qua per farlo partire proprio quando si è fermato dal camminare, altrimenti
            // parte subito appena mette un piede nel buco
            if (won_ && lerp_amount_ >= 1.0f)
            {
                ragdoll_ = new PlayerRagDool(world_location_);
                GameReferencesLocator.SoundManagerReference.PlayWin();
            }

            // Quando il nemico mi becca, fermo il player
            if (!input_enabled_)
                InputActionPressed = InputManager.InputAction.Fermo;

            base.Update(elapsed, g, InputActionPressed);

            if (IsMoving)
                GameReferencesLocator.SoundManagerReference.PlayPlayerWalking();
            else
                GameReferencesLocator.SoundManagerReference.StopPlayerWalking();

            RepositionCamera();

            #endregion
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (ragdoll_ != null)
            {
                ragdoll_.Draw(spriteBatch);
                return;
            }

            if (effects_ != null)
                effects_.Draw(spriteBatch);

            base.Draw(spriteBatch);
        }

        public void DrawSparklesLightmap(SpriteBatch spriteBatch)
        {
            effects_.DrawSparkleLightmap(spriteBatch);
        }

        #endregion
    }
}
