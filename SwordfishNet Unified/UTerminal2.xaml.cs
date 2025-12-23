using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SwordfishNet_Unified
{
    public partial class UTerminal2 : Page
    {
        private static UTerminal? _instance;
        public static UTerminal Instance => _instance ??= new UTerminal();
        public UTerminal2()
        {
            InitializeComponent();
            this.Loaded += UTerminal2_Loaded;
            this.Unloaded += UTerminal2_Unloaded;
        }
        private async void UTerminal2_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UserCredentials.Instance.AreCredentialsSetSsh())
            {
                MessageBox.Show("Please set your SSH credentials in the Login window.", "Credentials Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //await UTerminal_ConnectAsync();
        }
        private void UTerminal2_Unloaded(object sender, RoutedEventArgs e)
        {
            // Intentionally empty to keep terminal connected when navigating tabs
        }

    }
}
