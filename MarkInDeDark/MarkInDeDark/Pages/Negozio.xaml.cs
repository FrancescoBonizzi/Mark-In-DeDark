using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Threading;
using System.Windows.Media;
using Microsoft.Phone.Tasks;

namespace MarkInDeDark.Pages
{
    public partial class Negozio : PhoneApplicationPage
    {
        public Negozio()
        {
            InitializeComponent();
        }

        private void Acquista_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceSearchTask marketplaceSearchTask = new MarketplaceSearchTask();
            marketplaceSearchTask.SearchTerms = "Mark in DeDark PRO";
            marketplaceSearchTask.Show();
        }
    }
}