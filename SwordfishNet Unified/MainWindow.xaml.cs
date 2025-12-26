using System.Windows;

namespace SwordfishNet_Unified
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null!; // Singleton instance
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            // Navigate to singleton instances of the pages
            ServerConfigFrame.Navigate(ServerConfigPage.Instance);
            OMVBrowserFrame.Navigate(OMVBrowser.Instance);
            OMVFileExpFrame.Navigate(OMVFileExp.Instance);
            TerminalFrame.Navigate(UTerminal.Instance);

            // Initially disable all tabs except the Services tab
            FileExpTab.IsEnabled = false; 
            TermTab.IsEnabled = false;
            NASPortalTab.IsEnabled = false;
            ServTab.IsSelected = true;
            ServTab.IsEnabled = true;

            // You must call SetEnabledTabs(true) to enable the tabs once a server is connected
            SetEnabledTabs(false);
        }
        public void SetEnabledTabs(bool isEnabled) // Enable or disable tabs based on server connection status
        {
            FileExpTab.IsEnabled = isEnabled;
            TermTab.IsEnabled = isEnabled;
            NASPortalTab.IsEnabled = isEnabled;
            
            if (!isEnabled)
            {
                FileExpTab.IsSelected = false;
                FileExpTab.IsEnabled = false;
                TermTab.IsSelected = false;
                TermTab.IsEnabled = false;
                NASPortalTab.IsSelected = false;
                NASPortalTab.IsEnabled = false;
                ServTab.IsSelected = true;
            }
        }
    }
}