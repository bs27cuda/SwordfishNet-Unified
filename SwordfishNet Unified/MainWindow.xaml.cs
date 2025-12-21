using System.Windows;

namespace SwordfishNet_Unified
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null!;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            ServerConfigFrame.Navigate(ServerConfigPage.Instance);
            OMVBrowserFrame.Navigate(OMVBrowser.Instance);
            OMVFileExpFrame.Navigate(OMVFileExp.Instance);
            TerminalFrame.Navigate(UTerminal.Instance);
            FileExpTab.IsEnabled = false;
            TermTab.IsEnabled = false;
            NASPortalTab.IsEnabled = false;
            ServTab.IsSelected = true;
            ServTab.IsEnabled = true;
            SetEnabledTabs(false);
        }
        public void SetEnabledTabs(bool isEnabled)
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