using System.Windows;

namespace MarkInDeDark.LevelEditor
{
    public partial class InputDialog : Window
    {
        public string InputString { get; private set; }

        public InputDialog(
            string title,
            string text)
        {
            InitializeComponent();
            Title = title;
            txtText.Text = text;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
                return;

            InputString = txtInput.Text;
            DialogResult = true;
            Close();
        }
    }
}
