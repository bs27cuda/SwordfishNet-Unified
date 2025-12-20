using System.Windows;

namespace SwordfishNet_Unified
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ServerConfigFrame.Navigate(ServerConfigPage.Instance);
            OMVBrowserFrame.Navigate(OMVBrowser.Instance);
            OMVFileExpFrame.Navigate(OMVFileExp.Instance);
            TerminalFrame.Navigate(UTerminal.Instance);
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            UTerminal.Instance?.Shutdown();

            base.OnClosing(e);
        }
        private void OpenTerminal_Click(object sender, RoutedEventArgs e)
        {
            TerminalFrame.Navigate(UTerminal.Instance);
        }

    }
}