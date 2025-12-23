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
        private static ServerConfigPage? _instance;
        public static ServerConfigPage Instance => _instance ??= new ServerConfigPage();

        private const string netConfigFileName = "netconfig.dat";
        private const string ConfigEncryptionKey = "5kf93m-d94k1-ad69wcr-m9348uv-w34pet-prm0t-9uwv4e-9t8y-nwgpe-5wvm";

        public ServerConfigPage()
        {
            InitializeComponent();
            LogoffButton.IsEnabled = false;
        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string user = UsernameTextBox.Text.Trim();
            string pass = PasswordBox.Password;
            string serverPath = UserCredentials.Instance.ServerPath;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Please supply user name and password.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(serverPath))
            {
                MessageBox.Show("Server path is not set. Please configure the server settings before connecting.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                PasswordBox.Clear();
                LogoffButton.IsEnabled = true;
                LoginButton.IsEnabled = false;
                MainWindow.Instance.SetEnabledTabs(true);
            }
            else
            {
                MessageBox.Show("Failed to connect to the server. Credentials refused.", "Connection Refused", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void LogoffButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogoffButton.IsEnabled = false;

                UserCredentials.Instance.ClearCredentials();
                OMVFileExp.Instance.CloseSftpConnection();
                UTerminal.Instance.Shutdown();

                var slab = OMVBrowser.Instance?.OMVDashboardSlab;
                if (slab != null)
                {
                    if (slab.CoreWebView2 != null)
                    {
                        await slab.CoreWebView2.Profile.ClearBrowsingDataAsync();
                    }
                    slab.Source = new Uri("about:blank");
                }

                MainWindow.Instance.SetEnabledTabs(false);

                UsernameTextBox.Clear();
                ServAddrBox.Clear();
                ServSshPortBox.Clear();
                ServSftpPortBox.Clear();
                ServHttpPortBox.Clear();
                ServHttpsPortBox.Clear();

                LoginButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logoff Crash Prevented: {ex.Message}");
            }
        }
        private static bool TestSftpConnection(string serverPath, string user, string pass)
        {
            if (!int.TryParse(UserCredentials.Instance.SftpPort, out int sftpPort))
            {
                MessageBox.Show("Invalid SFTP port number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using var client = new SftpClient(serverPath, sftpPort, user, pass);
            try
            {
                client.Connect();
                if (client.IsConnected)
                {
                    client.Disconnect();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show($"Connection failed: {Ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private void GotoPass(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
                PasswordBox.Focus();
            }
        }
        private void DoLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;
                ConnectButton_Click(this, new RoutedEventArgs());
            }
        }
        public void SaveServerConfiguration(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServAddrBox.Text) ||
                string.IsNullOrWhiteSpace(ServSshPortBox.Text) ||
                string.IsNullOrWhiteSpace(ServSftpPortBox.Text) ||
                string.IsNullOrWhiteSpace(ServHttpPortBox.Text) ||
                string.IsNullOrWhiteSpace(ServHttpsPortBox.Text))
            {
                MessageBox.Show("All fields must be completed.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var netConfigLogin = NetConfigLogin.Instance;
            bool? dialogResult = netConfigLogin.ShowDialog();

            string encryptionPassword = string.Empty;

            if (dialogResult == true)
            {
                if (netConfigLogin.EnteredPassword != null)
                {
                    encryptionPassword = netConfigLogin.EnteredPassword;
                }
            }
            else
            {
                MessageBox.Show("Configuration save canceled. No encryption password provided.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            UserCredentials.Instance.SetCredentials(
                ServAddrBox.Text.Trim(),
                UserCredentials.Instance.Username,
                UserCredentials.Instance.Password,
                ServSshPortBox.Text.Trim(),
                ServSftpPortBox.Text.Trim(),
                ServHttpPortBox.Text.Trim(),
                ServHttpsPortBox.Text.Trim()
            );

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

                string jsonToSave = JsonSerializer.Serialize(serverConfig);

                EncryptionHelper.EncryptAndSave(encryptionPassword, jsonToSave, filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save encrypted configuration: {ex.Message}", "Encryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SaveServerConfig.Background = Brushes.LightGreen;

            DispatcherTimer timer = new()
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, args) =>
            {
                SaveServerConfig.Background = Brushes.LightGray;
                timer.Stop();
            };
            timer.Start();
            UsernameTextBox.Focus();
        }
        private void LoadServerConfiguration(object sender, RoutedEventArgs e)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, netConfigFileName);
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Server configuration file not found.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var netConfigLogin = NetConfigLogin.Instance;
            bool? dialogResult = netConfigLogin.ShowDialog();

            string decryptionPassword = string.Empty;

            if (dialogResult == true)
            {
                if (netConfigLogin.EnteredPassword != null)
                {
                    decryptionPassword = netConfigLogin.EnteredPassword;
                }
            }
            else
            {
                MessageBox.Show("Configuration load canceled. No decryption password provided.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                string? jsonString = EncryptionHelper.DecryptAndLoad(decryptionPassword, filePath);

                if (jsonString == null)
                {
                    MessageBox.Show("Data not loaded. Decryption error.", "Decryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var configData = JsonSerializer.Deserialize<ServerConfigFile>(jsonString);
                if (configData == null)
                {
                    MessageBox.Show("Failed to parse configuration data.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ServAddrBox.Text = configData.ServerPath ?? "0.0.0.0";
                ServSshPortBox.Text = configData.SshPort ?? "22";
                ServSftpPortBox.Text = configData.SftpPort ?? "22";
                ServHttpPortBox.Text = configData.HttpPort ?? "80";
                ServHttpsPortBox.Text = configData.HttpsPort ?? "443";

                UserCredentials.Instance.SetCredentials(
                ServAddrBox.Text.Trim(),
                UserCredentials.Instance.Username,
                UserCredentials.Instance.Password,
                ServSshPortBox.Text.Trim(),
                ServSftpPortBox.Text.Trim(),
                ServHttpPortBox.Text.Trim(),
                ServHttpsPortBox.Text.Trim()
            );

                MessageBox.Show("Configuration loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UsernameTextBox.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UTerminal.Instance?.Shutdown();
                OMVFileExp.Instance?.CloseSftpConnection();
                OMVBrowser.Instance?.OMVDashboardSlab?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"App Close Crash Prevented: {ex.Message}", "App close exception", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            finally
            {
                Application.Current.Shutdown();
            }
        }
    }
}
