using Renci.SshNet;
using SwordfishNet;
using System.IO;
using System.Net.Mail;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SwordfishNet_Unified
{
    public partial class ServerConfigPage : Page
    {
        private static ServerConfigPage? _instance; // Singleton instance
        public static ServerConfigPage Instance => _instance ??= new ServerConfigPage(); // Lazy initialization

        private const string netConfigFileName = "netconfig.dat"; // Encrypted configuration file name
        private const string ConfigEncryptionKey = "5kf93m-d94k1-ad69wcr-m9348uv-w34pet-prm0t-9uwv4e-9t8y-nwgpe-5wvm"; // Key for encryption/decryption

        public ServerConfigPage()
        {
            InitializeComponent();

            // Initialize UI state for logoff button not available when no user is logged in
            LogoffButton.IsEnabled = false;
        }
        private void About_Click(object sender, RoutedEventArgs e) // About dialog
        {
            AboutWindow aboutWindow = new()
            {
                Owner = Application.Current.MainWindow
            };
            aboutWindow.ShowDialog();
        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e) // Handle user login
        {
            string user = UsernameTextBox.Text.Trim(); // Get username from input
            string pass = PasswordBox.Password; // Get password from input
            string serverPath = UserCredentials.Instance.ServerPath; // Get server path from stored credentials

            // Validate user input
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Please supply user name and password.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate server path
            if (string.IsNullOrWhiteSpace(serverPath))
            {
                MessageBox.Show("Server path is not set. Please configure the server settings before connecting.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Test SFTP connection with provided credentials
            if (TestSftpConnection(serverPath, user, pass))
            {
                UserCredentials.Instance.SetCredentials(
                     serverPath,
                     user,
                     pass,
                     UserCredentials.Instance.SshPort,
                     UserCredentials.Instance.SftpPort,
                     UserCredentials.Instance.HttpPort,
                     UserCredentials.Instance.HttpsPort
                 );
                PasswordBox.Clear(); // Clear password box for security
                LogoffButton.IsEnabled = true; // Enable logoff button
                LoginButton.IsEnabled = false; // Disable login button
                MainWindow.Instance.SetEnabledTabs(true); // Enable main application tabs
            }
            else // Connection failed
            {
                MessageBox.Show("Failed to connect to the server. Credentials refused.", "Connection Refused", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void LogoffButton_Click(object sender, RoutedEventArgs e) // Handle user logoff
        {
            try
            {
                LogoffButton.IsEnabled = false; // Disable logoff button during process

                UserCredentials.Instance.ClearCredentials(); // Clear stored user credentials
                OMVFileExp.Instance.CloseSftpConnection(); // Close any active SFTP connections
                UTerminal.Instance.Shutdown(); // Shutdown terminal sessions

                var slab = OMVBrowser.Instance?.OMVDashboardSlab; // Get the WebView2 slab
                if (slab != null) // Clear browsing data and reset to blank page
                {
                    if (slab.CoreWebView2 != null)
                    {
                        await slab.CoreWebView2.Profile.ClearBrowsingDataAsync();
                    }
                    slab.Source = new Uri("about:blank");
                }

                MainWindow.Instance.SetEnabledTabs(false); // Disable main application tabs

                UsernameTextBox.Clear();
                ServAddrBox.Clear();
                ServSshPortBox.Clear();
                ServSftpPortBox.Clear();
                ServHttpPortBox.Clear();
                ServHttpsPortBox.Clear();

                LoginButton.IsEnabled = true; // Re-enable login button
            }
            catch (Exception ex) // Logoff exception handling
            {
                System.Diagnostics.Debug.WriteLine($"Logoff Crash Prevented: {ex.Message}");
            }
        }
        private static bool TestSftpConnection(string serverPath, string user, string pass) // Test SFTP connection method
        {
            if (!int.TryParse(UserCredentials.Instance.SftpPort, out int sftpPort)) // Validate SFTP port
            {
                MessageBox.Show("Invalid SFTP port number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Attempt SFTP connection
            using var client = new SftpClient(serverPath, sftpPort, user, pass);
            try
            {
                client.Connect(); // Connect to server
                if (client.IsConnected) // Check connection status
                {
                    client.Disconnect(); // Disconnect after successful connection
                    return true;
                }
                else // Connection failed
                {
                    return false;
                }
            }
            catch (Exception Ex) // Handle connection exceptions
            {
                MessageBox.Show($"Connection failed: {Ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private void GotoPass(object sender, KeyEventArgs e) // Handle Enter key to move focus to password box
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
                PasswordBox.Focus();
            }
        }
        private void DoLogin(object sender, KeyEventArgs e) // Handle Enter key to trigger login
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
                ConnectButton_Click(this, new RoutedEventArgs());
            }
        }
        public void SaveServerConfiguration(object sender, RoutedEventArgs e) // Save server configuration method
        {
            if (string.IsNullOrWhiteSpace(ServAddrBox.Text) ||
                string.IsNullOrWhiteSpace(ServSshPortBox.Text) ||
                string.IsNullOrWhiteSpace(ServSftpPortBox.Text) ||
                string.IsNullOrWhiteSpace(ServHttpPortBox.Text) ||
                string.IsNullOrWhiteSpace(ServHttpsPortBox.Text))
            {
                MessageBox.Show("All fields must be completed.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning); // Validate all fields are filled
                return;
            }

            var netConfigLogin = NetConfigLogin.Instance; // Prompt for encryption password
            bool? dialogResult = netConfigLogin.ShowDialog(); // Show password dialog

            string encryptionPassword = string.Empty; // Variable to hold encryption password

            if (dialogResult == true) // If user provided password
            {
                if (netConfigLogin.EnteredPassword != null) // Get entered password
                {
                    encryptionPassword = netConfigLogin.EnteredPassword; // Store encryption password
                }
            }
            else // User canceled password entry or left password blank
            {
                MessageBox.Show("Configuration save canceled. No encryption password provided.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Update UserCredentials with current UI values
            UserCredentials.Instance.SetCredentials(
                ServAddrBox.Text.Trim(),
                UserCredentials.Instance.Username,
                UserCredentials.Instance.Password,
                ServSshPortBox.Text.Trim(),
                ServSftpPortBox.Text.Trim(),
                ServHttpPortBox.Text.Trim(),
                ServHttpsPortBox.Text.Trim()
            );

            // Save configuration to encrypted file
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, netConfigFileName);

                var serverConfig = new ServerConfigFile
                {
                    ServerPath = UserCredentials.Instance.ServerPath,
                    SshPort = UserCredentials.Instance.SshPort,
                    SftpPort = UserCredentials.Instance.SftpPort,
                    HttpPort = UserCredentials.Instance.HttpPort,
                    HttpsPort = UserCredentials.Instance.HttpsPort
                };

                string jsonToSave = JsonSerializer.Serialize(serverConfig); // Serialize configuration to JSON

                EncryptionHelper.EncryptAndSave(encryptionPassword, jsonToSave, filePath); // Encrypt and save to file
            }
            catch (Exception ex) // Handle save exceptions
            {
                MessageBox.Show($"Failed to save encrypted configuration: {ex.Message}", "Encryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SaveServerConfig.Background = Brushes.Green; // Visual feedback for successful save

            DispatcherTimer timer = new() // Timer to reset button color
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, args) =>
            {
                SaveServerConfig.Background = (Brush)Application.Current.FindResource("Background");
                
                timer.Stop();
            };
            timer.Start();
            UsernameTextBox.Focus(); // Set focus back to username box
        }
        private void LoadServerConfiguration(object sender, RoutedEventArgs e) // Load server configuration method
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, netConfigFileName); // Path to encrypted configuration file
            if (!File.Exists(filePath)) // Check if file exists
            {
                MessageBox.Show("Server configuration file not found.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var netConfigLogin = NetConfigLogin.Instance; // Prompt for decryption password
            bool? dialogResult = netConfigLogin.ShowDialog(); // Show password dialog

            string decryptionPassword = string.Empty; // Variable to hold decryption password

            if (dialogResult == true) // If user provided password
            {
                if (netConfigLogin.EnteredPassword != null) // Get entered password
                {
                    decryptionPassword = netConfigLogin.EnteredPassword; // Store decryption password
                }
            }
            else // User canceled password entry or left password blank
            {
                MessageBox.Show("Configuration load canceled. No decryption password provided.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Load and decrypt configuration file
            try
            {
                string? jsonString = EncryptionHelper.DecryptAndLoad(decryptionPassword, filePath); // Decrypt and load file

                if (jsonString == null) // Check if decryption was successful
                {
                    MessageBox.Show("Data not loaded. Decryption error.", "Decryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var configData = JsonSerializer.Deserialize<ServerConfigFile>(jsonString); // Deserialize JSON to configuration object
                if (configData == null) // Check if deserialization was successful
                {
                    MessageBox.Show("Failed to parse configuration data.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Populate UI fields with loaded configuration, using defaults to avoid nulls
                ServAddrBox.Text = configData.ServerPath ?? "0.0.0.0";
                ServSshPortBox.Text = configData.SshPort ?? "22";
                ServSftpPortBox.Text = configData.SftpPort ?? "22";
                ServHttpPortBox.Text = configData.HttpPort ?? "80";
                ServHttpsPortBox.Text = configData.HttpsPort ?? "443";

                // Update UserCredentials with loaded configuration
                UserCredentials.Instance.SetCredentials(
                ServAddrBox.Text.Trim(),
                UserCredentials.Instance.Username,
                UserCredentials.Instance.Password,
                ServSshPortBox.Text.Trim(),
                ServSftpPortBox.Text.Trim(),
                ServHttpPortBox.Text.Trim(),
                ServHttpsPortBox.Text.Trim()
            );

                MessageBox.Show("Configuration loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); // Notify user of successful load
                UsernameTextBox.Focus(); // Set focus back to username box
            }
            catch (Exception ex) // Handle load exceptions
            {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void CloseApp_Click(object sender, RoutedEventArgs e) // Handle application close
        {
            // Clean up resources before closing
            try
            {
                UTerminal.Instance?.Shutdown();
                OMVFileExp.Instance?.CloseSftpConnection();
                OMVBrowser.Instance?.OMVDashboardSlab?.Dispose();
            }
            catch (Exception ex) // Handle exceptions during cleanup
            {
                MessageBox.Show($"App Close Crash Prevented: {ex.Message}", "App close exception", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            finally // Ensure application shuts down
            {
                Application.Current.Shutdown();
            }
        }
    }
}
