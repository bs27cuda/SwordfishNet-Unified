using System.Windows;
using System.Windows.Controls;

namespace SwordfishNet_Unified
{
    public partial class OMVBrowser : Page
    {
        private static OMVBrowser? _instance; // Singleton instance
        public static OMVBrowser Instance => _instance ??= new OMVBrowser(); // Lazy initialization
        public OMVBrowser()
        {
            InitializeComponent();

            // Attach the Loaded event handler
            this.Loaded += OMVBrowser_Loaded;
        }
        private async void OMVBrowser_Loaded(object sender, RoutedEventArgs e) // Event handler for Loaded event
        {
            try
            {
                await OMVDashboardSlab.EnsureCoreWebView2Async(null); // Ensure WebView2 is initialized
                string ipAddress = UserCredentials.Instance.ServerPath; // Retrieve server path from UserCredentials
                if (string.IsNullOrWhiteSpace(ipAddress)) // Check if server path is empty
                {
                    MessageBox.Show("No server path has been indicated. Please enter information on landing pad.", "No server path found", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string targetURL = $"http://{ipAddress.Trim()}"; // Construct the target URL

                if (OMVDashboardSlab != null && OMVDashboardSlab.CoreWebView2 != null) // Check if WebView2 is available
                {
                    OMVDashboardSlab.CoreWebView2.Navigate(targetURL); // Navigate to the target URL
                }
                else // Handle the case where WebView2 is not available
                {
                    System.Diagnostics.Debug.WriteLine("OMVDashboardSlab or CoreWebView2 is not available after EnsureCoreWebView2Async.");
                }
            }
            catch (Exception ex) // Catch any exceptions that occur during navigation
            {
                System.Diagnostics.Debug.WriteLine($"WebView2 Error: {ex.Message}");
            }
        }
        private void OMVBrowser_Closed(object sender, EventArgs e) // Event handler for Closed event
        {
            _instance = null;
        }
    }
}
