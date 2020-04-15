using System.Windows;

namespace MarkInDeDark.LevelEditor
{
    public partial class App : Application
    {
        public App()
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Oooops! Non doveva succedere!");
            e.Handled = true;
        }
    }
}
