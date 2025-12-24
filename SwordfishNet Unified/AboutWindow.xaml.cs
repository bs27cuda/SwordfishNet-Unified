using System.IO;
using System.Windows;
using System.Windows.Resources;

namespace SwordfishNet_Unified
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LoadAboutText();
        }
        private void LoadAboutText()
        {
            try
            {
                Uri resourceUri = new Uri("pack://application:,,,/about.txt");
                StreamResourceInfo resource = Application.GetResourceStream(resourceUri);
                if (resource != null)
                {
                    using (StreamReader reader = new StreamReader(resource.Stream))
                    {
                        Aboutblock.Text = reader.ReadToEnd();
                    }
                }
                else
                {
                    Aboutblock.Text = "Resource not found. Please check Build Action.";
                }
            }
            catch (Exception ex)
            {
                Aboutblock.Text = $"Error: {ex.Message}";
            }
        }
        private void ShowLegal(object sender, RoutedEventArgs e)
        {
            Uri legalUri = new Uri("pack://application:,,,/legal.txt");
            StreamResourceInfo legal = Application.GetResourceStream(legalUri);
            if (legal != null)
            {
                using (StreamReader reader = new StreamReader(legal.Stream))
                {
                    string content = reader.ReadToEnd();
                    MessageBox.Show(content, "Legal disclaimer", MessageBoxButton.OK, MessageBoxImage.None);
                }
            }
        }
    }
}
