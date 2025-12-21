using System.Windows;
using System.Windows.Controls;

namespace SwordfishNet_Unified
{
    public partial class OMVBrowser : Page
    {
        private static OMVBrowser? _instance;
        public static OMVBrowser Instance => _instance ??= new OMVBrowser();
        public OMVBrowser()
        {
            InitializeComponent();
            this.Loaded += OMVBrowser_Loaded;
        }
        private async void OMVBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await OMVDashboardSlab.EnsureCoreWebView2Async(null);
                string ipAddress = UserCredentials.Instance.ServerPath;

                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    MessageBox.Show("No server path has been indicated. Please enter information on landing pad.", "No server path found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string targetURL = $"http://{ipAddress.Trim()}";
                OMVDashboardSlab.CoreWebView2.Navigate(targetURL);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 Error: {ex.Message}");
            }
        }
        private void OMVBrowser_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }
    }
}
