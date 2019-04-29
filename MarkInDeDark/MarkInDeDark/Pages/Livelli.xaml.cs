using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Mark.Engine;
using MarkInDeDark.ViewModel;


namespace MarkInDeDark.Pages
{
    public partial class Livelli : PhoneApplicationPage
    {
        public Livelli()
        {
            InitializeComponent();

#if LITE_VERSION

#elif FULL_VERSION
            Advertisement.IsEnabled = false;
            Advertisement.Visibility = System.Windows.Visibility.Collapsed;
#endif
        }

        private void listBoxLevels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Se niente è selezionato, return
            if (listBoxLevels.SelectedIndex == -1)
                return;

            Level level_selected = listBoxLevels.SelectedItem as Level;

            // Resetta la selezione a -1
            listBoxLevels.SelectedIndex = -1;

            (App.Current as App).CurrentLevel = level_selected;
            NavigationService.Navigate(new Uri("/Pages/GamePage.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (ViewModel.ViewModelLocator.MainViewModelIstance.SoundEnabled.Value)
                App.BackgroundMusicPlayer.Play();

            (App.Current as App).CurrentLevel = null;
            GC.Collect(); // Così quando esco dal livello pulisco la memoria

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

    }
}