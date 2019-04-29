#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Mark.Engine;
using Mark.Engine.SoundEngine;
using MarkInDeDark.ViewModel;
using Microsoft.Phone.Shell;
using System.Text;

#endregion

namespace MarkInDeDark.Pages
{
    public partial class GamePage : PhoneApplicationPage
    {
        #region Dichiarazioni

        GameTimer timer;
        SpriteBatch spriteBatch;

        LevelManager game_;
        InputManager input_manager_;

        string currentLevel;

        enum GameStates { Playing, Paused, Finished };
        GameStates current_game_state_ = GameStates.Playing;

        UIElementRenderer elementRenderer_;

        private bool _playagain = false;

        /*
#if DEBUG
        FPScounter fps_;
#endif
        */

        #endregion

        #region Costruttore/Inizializzazione

        public GamePage()
        {
            InitializeComponent();

            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(166667); //60fps
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;

            PhoneApplicationService.Current.Deactivated += PhoneApplicationService_Deactivated;
            //            LayoutUpdated += new EventHandler(GamePage_LayoutUpdated);
        }

        private void InitializeLevel()
        {
            PauseOverlay.Visibility = Visibility.Collapsed;
            GameOverOverlay.Visibility = Visibility.Collapsed;
            WinOverlay.Visibility = Visibility.Collapsed;

            if (game_ == null || _playagain)
            {
                Loader.LoadFiles((App.Current as App).Content);
                GameReferencesLocator.SoundManagerReference.Initialize();

                game_ = new LevelManager(SharedGraphicsDeviceManager.Current.GraphicsDevice, (App.Current as App).CurrentLevel, Loader.font_);
                currentLevel = (App.Current as App).CurrentLevel.Filename;
                input_manager_ = new TouchInput();
                /*
    #if DEBUG
                fps_ = new FPScounter(font_);
    #endif
                */
                current_game_state_ = GameStates.Playing;
                _playagain = false;

                if (elementRenderer_ == null)
                {
                   elementRenderer_ = new UIElementRenderer(this, 800, 480);
                }


            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.BackgroundMusicPlayer.Stop();

            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);
            spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            InitializeLevel();
            if (timer != null)
                timer.Start();

            base.OnNavigatedTo(e);
        }

        #endregion

        #region Altri eventi

        private void PauseGame()
        {
            game_.Paused = true;
            try
            {
                PauseAnimationFadeIn.Begin();
                GC.Collect();
            }
            catch
            { }
        }

        /*
        void GamePage_LayoutUpdated(object sender, EventArgs e)
        {
            // Create the UIElementRenderer to draw the XAML page to a texture.

            // Check for 0 because when we navigate away the LayoutUpdate event
            // is raised but ActualWidth and ActualHeight will be 0 in that case.
            if (ActualWidth > 0 && ActualHeight > 0 && elementRenderer_ == null)
            {
                elementRenderer_ = new UIElementRenderer(this, 800, 480);
            }
        }*/

        private void PhoneApplicationService_Deactivated(object sender, DeactivatedEventArgs e)
        {
            if (current_game_state_ == GameStates.Playing)
            {
                if (game_ != null)
                {
                    if (!game_.Paused)
                    {
                        PauseGame();
                    }
                }
            }
        }

        // Ci metto il bottone "ricomincia?"
        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            _playagain = true;
            InitializeLevel();
        }

        private void Livelli_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (current_game_state_ == GameStates.Playing)
            {
                // Se il gioco non è già in pausa...
                if (!game_.Paused)
                {
                    e.Cancel = true;
                    PauseGame();
                }
            }

            // Altrimenti come al solito
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Arrestare il timer
            timer.Stop();

            // Impostare la modalità di condivisione del dispositivo di grafica per disattivare il rendering XNA
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);
            GameReferencesLocator.SoundManagerReference.PauseAll();

            base.OnNavigatedFrom(e);
        }

        private void Exit_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Continue_Click_1(object sender, RoutedEventArgs e)
        {
            PauseAnimationFadeOut.Begin();
            game_.Paused = false;
        }

        #endregion

        #region Update

        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            float gameTime = (float)e.ElapsedTime.TotalMilliseconds / 1000.0f;

            input_manager_.Update(gameTime);

            /*
#if DEBUG
            fps_.Update(gameTime);
#endif
            */

            if (!game_.Paused)
            {

                switch (current_game_state_)
                {
                    case GameStates.Playing:

                        game_.Update(gameTime, input_manager_);



                        if (game_.finished_)
                        {
                            GameReferencesLocator.SoundManagerReference.StopSounds();
                            current_game_state_ = GameStates.Finished;
                            if (game_.won_)
                            {
                                ViewModelLocator.MainViewModelIstance.Levels.SetCompleted(currentLevel);
                                ViewModelLocator.MainViewModelIstance.Levels.SetRecordUsedCandles(currentLevel, GameReferencesLocator.CandlesReference.PlayerUsedCandles);
                                ViewModelLocator.MainViewModelIstance.Levels.SetRecordCompletionTime(currentLevel, game_.ElapsedGameTime);

                                if (game_.SecretTaken)
                                {
                                    ViewModelLocator.MainViewModelIstance.Levels.SetSecretTaken(currentLevel);
                                    ++ViewModelLocator.MainViewModelIstance.Levels.nSecretsFound.Value;
                                }

                                CandeleUsate.Text = " " + GameReferencesLocator.CandlesReference.PlayerUsedCandles.ToString();

                                TimeSpan s = TimeSpan.FromSeconds(game_.ElapsedGameTime);
                                TempoImpiegato.Text = " " + string.Format("{0}:{1:00}", (int)s.TotalMinutes, s.Seconds);

                                WinAnimationFadeIn.Begin();
                            }
                            else
                            {
                                GameOverAnimationFadeIn.Begin();
                                game_ = null;
                                GC.Collect();
                            }
                        }

                        break;

                    case GameStates.Finished:

                        elementRenderer_.Render();
                        input_manager_.Update(gameTime);

                        break;
                }

            }
        }

        #endregion

        #region Draw

        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            elementRenderer_.Render();

            switch (current_game_state_)
            {
                case GameStates.Playing:

                    game_.Draw(spriteBatch);

                    spriteBatch.Begin();
                    input_manager_.Draw(spriteBatch);
                    if (elementRenderer_ != null)
                        if (elementRenderer_.Texture != null)
                            spriteBatch.Draw(elementRenderer_.Texture, Vector2.Zero, Color.White);
                    /*
#if DEBUG
                    fps_.Draw(spriteBatch);
#endif
                    */


                    spriteBatch.End();

                    break;

                case GameStates.Finished:

                    spriteBatch.Begin();
                    /*
#if DEBUG
                    fps_.Draw(spriteBatch);
                        
#endif
                    */
                    if (elementRenderer_ != null)
                        if (elementRenderer_.Texture != null)
                            spriteBatch.Draw(elementRenderer_.Texture, Vector2.Zero, Color.White);
                    spriteBatch.End();

                    break;
            }
        }

        #endregion
    }
}