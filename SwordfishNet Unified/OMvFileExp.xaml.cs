using Renci.SshNet;
using System.Windows;
using System.Windows.Controls;

namespace SwordfishNet_Unified
{
    public partial class OMVFileExp : Page
    {
        private static OMVFileExp? _instance;
        public static OMVFileExp Instance => _instance ??= new OMVFileExp();

        private static int SftpPort => int.TryParse(UserCredentials.Instance.SftpPort, out int port) ? port : 22; // Standard SFTP is 22

        private SftpClient? _sftpClient; // Fixed CS8618

        private string? _currentLocalPath;
        private string? _currentServerPath;

        private enum ActivePane { None, Local, Server }
        private ActivePane _lastActive = ActivePane.None;

        private OMVFileExp()
        {
            InitializeComponent();
            this.Loaded += OMVFileExp_Loaded;

            // LocalList and ServerList focus logic
            LocalList.GotKeyboardFocus += (s, e) => { _lastActive = ActivePane.Local; };
            ServerList.GotKeyboardFocus += (s, e) => { _lastActive = ActivePane.Server; };
        }

        private void OMVFileExp_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UserCredentials.Instance.AreCredentialsSetBrowser())
            {
                MessageBox.Show("Please set your credentials in the Login window.", "Credentials Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            InitializeSftpConnection();
        }

        private void InitializeSftpConnection()
        {
            if (_sftpClient?.IsConnected == true) return;

            try
            {
                _sftpClient = new SftpClient(
                    UserCredentials.Instance.ServerPath,
                    SftpPort,
                    UserCredentials.Instance.Username,
                    UserCredentials.Instance.Password);

                _sftpClient.Connect();

                // Ensure these methods handle a null _sftpClient or use the field directly
                LoadServerRoots();
                LoadDriveRoots();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CloseSftpConnection()
        {
            if (_sftpClient != null)
            {
                if (_sftpClient.IsConnected) _sftpClient.Disconnect();
                _sftpClient.Dispose();
                _sftpClient = null;
            }
        }
    }
}