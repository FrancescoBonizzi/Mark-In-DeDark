#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Threading;
using Mark.Engine;
using Mark.Engine.SoundEngine;
using Microsoft.Phone.Tasks;
using MarkInDeDark.ViewModel;
using System.IO.IsolatedStorage;
using MarkInDeDark.Resources;

#endregion

namespace MarkInDeDark.Pages
{
    public partial class Menu : PhoneApplicationPage
    {
        private bool _rateFunctionAlreadyFired = false;
        private MediaElement _backgroundMusicPlayer;

        public Menu()
        {
            GameReferencesLocator.SoundManagerReference = new SoundManager();
            ViewModelLocator.MainViewModelIstance = new MainViewModel();

            _backgroundMusicPlayer = App.BackgroundMusicPlayer;

            InitializeComponent();

#if LITE_VERSION
            TextBlockVersione.Text = Traduzioni.MenuLiteVersion;
#elif FULL_VERSION
            TextBlockVersione.Text =  Traduzioni.MenuProVersion;
            Advertisement.IsEnabled = false;
            Advertisement.Visibility = System.Windows.Visibility.Collapsed;
            buttonShop.Visibility = System.Windows.Visibility.Collapsed;
#endif

            Loaded += (s, e) =>
            {
                if (!_rateFunctionAlreadyFired)
                    RateMyApp();
            };

            if (!ViewModelLocator.MainViewModelIstance.SoundEnabled.Value)
            {
                ToggleSoundSwitch.Content = Traduzioni.MenuSuoniNo;
            }
            else
            {
                _backgroundMusicPlayer.Play();
            }
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Livelli.xaml", UriKind.Relative));
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/About.xaml", UriKind.Relative));
        }

        private void buttonShop_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Negozio.xaml", UriKind.Relative));
        }

        public void RateMyApp()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

#if LITE_VERSION
            string Appname = "Mark in DeDark LITE";
#elif FULL_VERSION
            string Appname = "Mark in DeDark PRO";
#endif

            if (!settings.Contains("review"))
            {
                settings.Add("review", 1);
                settings.Add("rcheck", 0);
            }
            else
            {
                int no = Convert.ToInt32(settings["review"]);
                int check = Convert.ToInt32(settings["rcheck"]);
                no++;
                if ((no == 4 || no == 7 || no % 10 == 0) && check == 0)
                {
                    settings["review"] = no;
                    MessageBoxResult mm = MessageBox.Show(Traduzioni.MenuRateMyApp, Appname, MessageBoxButton.OKCancel);
                    if (mm == MessageBoxResult.OK)
                    {
                        settings["rcheck"] = 1;
                        settings.Save();
                        MarketplaceReviewTask rr = new MarketplaceReviewTask();
                        rr.Show();
                    }
                }
                else
                {
                    settings["review"] = no;
                }
            }
            settings.Save();

            _rateFunctionAlreadyFired = true;
        }

        private void ToggleSoundSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ToggleSoundSwitch.Content = Traduzioni.MenuSuoniSi;
            _backgroundMusicPlayer.Play();
        }

        private void ToggleSoundSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleSoundSwitch.Content = Traduzioni.MenuSuoniNo;
            _backgroundMusicPlayer.Stop();
        }
    }
}