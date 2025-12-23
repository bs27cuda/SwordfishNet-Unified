using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace SwordfishNet_Unified
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
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
            // Open the Legal window
        }
    }
}
