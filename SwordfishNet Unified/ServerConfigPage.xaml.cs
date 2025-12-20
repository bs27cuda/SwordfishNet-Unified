using SwordfishNet;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using Renci.SshNet;

namespace SwordfishNet_Unified
{
    public partial class ServerConfigPage : Page
    {
        private static ServerConfigPage _instance;
        public static ServerConfigPage Instance => _instance ??= new ServerConfigPage();
        private Brush _originalBrush;

        private const string netConfigFileName = "netconfig.dat";
        private const string ConfigEncryptionKey = "5kf93m-d94k1-ad69wcr-m9348uv-w34pet-prm0t-9uwv4e-9t8y-nwgpe-5wvm";

        public ServerConfigPage()
        {
            InitializeComponent();
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
                if (_originalBrush == null) _originalBrush = LoginButton.Background;
                LoginButton.Background = Brushes.LightGreen;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, args) =>
                {
                    LoginButton.Background = _originalBrush;
                    timer.Stop();
                };
                timer.Start();
                PasswordBox.Clear();
            }
            else
            {
                MessageBox.Show("Failed to connect to the server. Credentials refused.", "Connection Refused", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool TestSftpConnection(string serverPath, string user, string pass)
        {
            if (!int.TryParse(UserCredentials.Instance.SftpPort, out int sftpPort))
            {
                MessageBox.Show("Invalid SFTP port number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using (var client = new SftpClient(serverPath, sftpPort, user, pass))
            {
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
                encryptionPassword = netConfigLogin.EnteredPassword;
            }
            else
            {
                MessageBox.Show("Configuration save canceled. No encryption password provided.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (string.IsNullOrWhiteSpace(encryptionPassword))
            {
                MessageBox.Show("Configuration save canceled. Encryption password cannot be empty.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s, args) =>
            {
                SaveServerConfig.Background = Brushes.LightGray;
                timer.Stop();
            };
            timer.Start();
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
                decryptionPassword = netConfigLogin.EnteredPassword;
            }
            else
            {
                MessageBox.Show("Configuration load canceled. No decryption password provided.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(decryptionPassword))
            {
                MessageBox.Show("Configuration load canceled. Decryption password cannot be empty.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string jsonString = EncryptionHelper.DecryptAndLoad(decryptionPassword, filePath);

                if (jsonString == null)
                {
                    MessageBox.Show("Data not loaded. Decryption error.", "Decryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var configData = JsonSerializer.Deserialize<ServerConfigFile>(jsonString);

                ServAddrBox.Text = configData.ServerPath;
                ServSshPortBox.Text = configData.SshPort;
                ServSftpPortBox.Text = configData.SftpPort;
                ServHttpPortBox.Text = configData.HttpPort;
                ServHttpsPortBox.Text = configData.HttpsPort;

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
