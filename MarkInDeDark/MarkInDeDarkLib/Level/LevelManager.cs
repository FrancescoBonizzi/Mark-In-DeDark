#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.Diagnostics;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class LevelManager : IDisposable
    {
        #region Dichiarazioni

        private GameGrid grid_;
        private Win win_entity_;
        private Player player_;
        private List<Enemy> enemies_;
        private CandlesManager candles_;

        private FloatingObject key_;
        private bool has_key_ = false;

        private FloatingObject secret_;
        private bool secret_taken_ = false;

        private bool paused_ = false;

        private AppearingString help_string_ = null;

        public bool finished_ = false;
        public bool won_ = false;

        private float finish_timer_ = 4.0f; //s
        private float elapsed_finish_ = -1.0f;

        private bool splatter_done_ = false;

        private float elapsed_;
        private TimeSpan elapsed_timespan_;
        private StringBuilder elapsed_string_;
        private Vector2 elased_time_position_ = new Vector2(10.0f, 10.0f);
        private Color elapsed_time_string_color_ = Color.White;

        private StringBuilder candles_string_; 
        private Vector2 candles_string_position_ = new Vector2(707.0f, 371.0f);
        private Color candles_string_color_ = Color.Brown;

        private SpriteFont font_;
        private Color background_color_ = new Color(20, 20, 20) * 0.92f;

        #region Rendering

        private GraphicsDevice graphics_device_;
        private RenderTarget2D main_scene_;
        private RenderTarget2D light_mask_;
        private BlendState multiply_blendstate_;

        #endregion

        #endregion

        #region Costruttore / Distruttore

        public LevelManager(GraphicsDevice GraphicsDevice, Level LevelToLoad, SpriteFont Font)
        {
            this.graphics_device_ = GraphicsDevice;

            var pp = GraphicsDevice.PresentationParameters;
            main_scene_ = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            light_mask_ = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);

            multiply_blendstate_ = new BlendState()
            {
                AlphaSourceBlend = Blend.DestinationAlpha,
                AlphaDestinationBlend = Blend.Zero,
                AlphaBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add
            };

            font_ = Font;

            LoadLevel(LevelToLoad);

            GameReferencesLocator.SoundManagerReference.PlayEnlightTorch();
        }

        public void Dispose()
        {
            GameReferencesLocator.GridReference = null;
            GameReferencesLocator.PlayerReference = null;
            GameReferencesLocator.CandlesReference = null;
            main_scene_.Dispose();
            light_mask_.Dispose();
        }

        #endregion

        #region Metodi

        public void LoadLevel(Level LevelToLoad)
        {
            #region Entità che vanno nel reference

            grid_ = new GameGrid(LevelToLoad);
            Camera.WorldRectangle = new Rectangle(0, 0, grid_.TotalWidth, grid_.TotalHeigth);
            Camera.ViewPortWidth = 800;
            Camera.ViewPortHeight = 480;

            player_ = new Player(grid_);
            candles_ = new CandlesManager(LevelToLoad.StartingCandlesNumber);

            #endregion

            GameReferencesLocator.CandlesReference = candles_;
            GameReferencesLocator.GridReference = grid_;
            GameReferencesLocator.PlayerReference = player_;

            win_entity_ = new Win(grid_.WinPosition, Loader.win_closed_);
            key_ = new FloatingObject(Loader.key_, grid_.KeyPosition);

            if (!LevelToLoad.HasSecret.Value)
                secret_ = new FloatingObject(LevelToLoad.SecretRectangle, grid_.SecretObjectPosition);

            enemies_ = new List<Enemy>();
            if (grid_.GoblinPosition != Vector2.Zero)
            {
                enemies_.Add(new Goblin(grid_.GoblinPosition));
            }

            if (grid_.WerewolfPosition != Vector2.Zero)
            {
                enemies_.Add(new Werewolf(grid_.WerewolfPosition));
            }

            foreach (var zombie in grid_.ZombiePath)
            {
                enemies_.Add(new Zombie(zombie));
            }

            foreach (var skeletron in grid_.SkeletronPath)
            {
                enemies_.Add(new Skeletron(skeletron));
            }

            Camera.Position = grid_.PlayerStartingPosition;

            elapsed_ = 0.0f;
            elapsed_string_ = new StringBuilder();

            candles_string_ = new StringBuilder();
            candles_string_.AppendFormat("&#x1f484; {0}", LevelToLoad.StartingCandlesNumber);
        }

        public bool Paused
        {
            get { return paused_; }
            set
            {
                paused_ = value;
                if (value)
                {
                    GameReferencesLocator.SoundManagerReference.PauseAll();
                }
                else
                {
                    GameReferencesLocator.SoundManagerReference.ResumeAll();
                }
            }
        }

        public bool SecretTaken
        {
            get { return secret_taken_; }
        }

        public int ElapsedGameTime
        {
            get { return (int)elapsed_; }
        }

        #endregion

        #region Update / Draw

        public void Update(float elapsed, InputManager inputManager)
        {
            if (!Paused)
            {
                elapsed_ += elapsed;
                elapsed_timespan_ = TimeSpan.FromSeconds((int)elapsed_);
                elapsed_string_.Clear();
                elapsed_string_.AppendFormat("⏰ {0}:{1:00}", (int)elapsed_timespan_.TotalMinutes, elapsed_timespan_.Seconds);

                candles_string_.Clear();
                candles_string_.AppendFormat("{0}", GameReferencesLocator.CandlesReference.CandlesLeftToUse);

                #region Finish game

                if (finished_)
                    return;

                if (elapsed_finish_ >= 0.0f)
                {
                    elapsed_finish_ += elapsed;

                    if (elapsed_finish_ >= 0.5f)
                    {
                        if (!won_)
                        {
                            if (!splatter_done_)
                            {
                                grid_.ApplyBloodDecal(player_.WorldCenter);
                                GameReferencesLocator.SoundManagerReference.PlayBlood();
                                splatter_done_ = true;
                            }
                        }
                    }

                    if (elapsed_finish_ >= finish_timer_)
                    {
                        finished_ = true;
                    }
                }

                #endregion

                #region Grid

                grid_.Update(elapsed);

                #endregion

                #region Player

                player_.Update(elapsed, grid_, inputManager.CurrentInputAction);

                #endregion

                #region Objects

                candles_.Update(elapsed);
                if (help_string_ != null)
                {
                    help_string_.Update(elapsed);
                    if (!help_string_.Active)
                        help_string_ = null;
                }

                #region Chiave/SecretObject

                if (key_ != null)
                {
                    key_.Update(elapsed);
                    if (!key_.Fading)
                    {
                        if (player_.CollisionRectangle.Intersects(key_.CollisionRectangle))
                        {
                            has_key_ = true;
                            key_.Taken();
                            win_entity_.TextureRectangle = Loader.win_;
                            GameReferencesLocator.SoundManagerReference.PlayKey();
                        }
                    }

                    if (!key_.Active)
                        key_ = null;
                }

                if (secret_ != null)
                {
                    secret_.Update(elapsed);
                    if (!secret_.Fading)
                    {
                        if (player_.CollisionRectangle.Intersects(secret_.CollisionRectangle))
                        {
                            secret_taken_ = true;
                            secret_.Taken();
                            GameReferencesLocator.SoundManagerReference.PlayKey();
                            if (help_string_ == null)
                                help_string_ = new AppearingString("❓❗", Color.White);
                        }
                    }

                    if (!secret_.Active)
                        secret_ = null;
                }

                #endregion

                #endregion

                #region Enemy

                for (int i = 0; i < enemies_.Count; ++i)
                {
                    Enemy e = enemies_[i];

                    e.Update(elapsed, grid_);

                    if (e.IsCollidingWith(player_.CollisionRectangle))
                    {
                        e.Attack();
                        if (!player_.Dead && !won_)
                        {
                            player_.Die();
                            player_.InputEnabled = false;
                            player_.Update(elapsed); // Gli sparo un update così fa più in fretta a disegnare il sangue

                            won_ = false;
                            e.HitPlayer();
                        }

                        // Piccolo timer per dare il tempo al mostro per mangiare l'omino prima di far finire il gioco
                        if (elapsed_finish_ <= 0.0f)
                            elapsed_finish_ = 0.0f;

                        break;
                    }
                }

                #endregion

                #region Win

                win_entity_.Update(elapsed);

                if (elapsed_finish_ < 0.0f)
                {
                    if (win_entity_.IsCollidingWith(player_.CollisionRectangle))
                    {
                        if (has_key_)
                        {
                            player_.Win();
                            won_ = true;
                            if (elapsed_finish_ <= 0.0f)
                                elapsed_finish_ = 0.0f;
                        }
                        else
                        {
                            if (help_string_ == null)
                                help_string_ = new AppearingString("...", Color.White);
                        }
                    }
                }

                #endregion
            }
        }

        private float RandomTimerForBackgroundMusic()
        {
            return Helper.RandomBetween(20.0f, 50.0f);
        }

        private void DrawLightMap(SpriteBatch spriteBatch)
        {
            graphics_device_.SetRenderTarget(light_mask_);
            graphics_device_.Clear(background_color_);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            player_.MyLight.Draw(spriteBatch);

            foreach (Enemy en in enemies_)
            {
                en.MyLight.Draw(spriteBatch);
                if (en is Werewolf)
                    (en as Werewolf).DrawSparkles(spriteBatch);
            }

            win_entity_.MyLight.Draw(spriteBatch);
            candles_.DrawLightmap(spriteBatch);

            if (player_.EffectActive())
                player_.DrawSparklesLightmap(spriteBatch);

            spriteBatch.End();

            graphics_device_.SetRenderTarget(null);
            graphics_device_.Clear(Color.Black);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawLightMap(spriteBatch);

            spriteBatch.Begin();
            graphics_device_.Clear(Color.Black);

            grid_.DrawCells(spriteBatch);
            win_entity_.Draw(spriteBatch);
            grid_.DrawWalls(spriteBatch);
            grid_.DrawAngles(spriteBatch);
            candles_.Draw(spriteBatch);

            if (key_ != null)
                key_.Draw(spriteBatch);
            if (secret_ != null)
                secret_.Draw(spriteBatch);

            player_.Draw(spriteBatch);
            candles_.DrawSmoke(spriteBatch);

            foreach (Enemy en in enemies_)
                en.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, multiply_blendstate_);
            spriteBatch.Draw(light_mask_, Vector2.Zero, Color.White);
            spriteBatch.End();

            spriteBatch.Begin();

            if (help_string_ != null)
            {
                help_string_.Draw(spriteBatch);
            }
            spriteBatch.DrawString(font_, elapsed_string_, elased_time_position_, elapsed_time_string_color_);
            spriteBatch.DrawString(font_, candles_string_, candles_string_position_, candles_string_color_);
            spriteBatch.End();
        }

        #endregion
    }
}
