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
            LoadAboutText(); // Calls the class that loads the about text upon creating the window
        }
        private void LoadAboutText()
        {
            try
            {
                Uri resourceUri = new Uri("pack://application:,,,/about.txt"); // Path to the embedded resource
                StreamResourceInfo resource = Application.GetResourceStream(resourceUri); // Get the resource stream
                if (resource != null) // Check if the resource was found, and read it
                {
                    using (StreamReader reader = new StreamReader(resource.Stream))
                    {
                        Aboutblock.Text = reader.ReadToEnd();
                    }
                }
                else // Resource not found
                {
                    Aboutblock.Text = "Resource not found. Please check Build Action.";
                }
            }
            catch (Exception ex) // Catch any exceptions and display the error message
            {
                Aboutblock.Text = $"Error: {ex.Message}";
            }
        }
        private void ShowLegal(object sender, RoutedEventArgs e)
        {
            Uri legalUri = new Uri("pack://application:,,,/legal.txt"); // Path to the legal disclaimer resource
            StreamResourceInfo legal = Application.GetResourceStream(legalUri); // Get the resource stream
            if (legal != null) // Check if the resource was found, and read it
            {
                using (StreamReader reader = new StreamReader(legal.Stream))
                {
                    string content = reader.ReadToEnd();
                    MessageBox.Show(content, "Legal disclaimer", MessageBoxButton.OK, MessageBoxImage.None); // Show the legal disclaimer in a message box
                }
            }
        }
        private void CloseWindow (object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
