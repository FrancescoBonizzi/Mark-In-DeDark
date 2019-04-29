using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Phone.Tasks;

/*
 * TODO TRADUZIONE IN
 *  - ITALIANO
 *  - ENG
 *  - RUMENO
 *  - RUSSO
 *  - SPANGOLO
 *  - FRANCESE
 *  - 
 */

/* TODO RATE APP REMINDER!!!!!!!!!!!
 * 
 */

namespace MarkInDeDark.Pages
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();

#if LITE_VERSION
            
#elif FULL_VERSION
            textblockFullVersion.Visibility = System.Windows.Visibility.Visible;
            buttonShop.Visibility = System.Windows.Visibility.Collapsed;
#endif
        }

        private void buttonInviaMail_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.Subject = "MarkInDeDark - Informazioni";
            emailComposeTask.Body = "";
            emailComposeTask.To = "fbonizzi@hotmail.it";
            emailComposeTask.Show();
        }

        private void buttonVota_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void buttonShop_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Negozio.xaml", UriKind.Relative));
        }

        private void buttonAltreApp_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceSearchTask marketplaceSearchTask = new MarketplaceSearchTask();
            marketplaceSearchTask.SearchTerms = "Francesco Bonizzi";
            marketplaceSearchTask.Show();
        }
    }
}