using Renci.SshNet;
using System.Windows;
using System.Windows.Controls;

namespace SwordfishNet_Unified
{
    public partial class OMVFileExp : Page
    {
        private static OMVFileExp? _instance; // Singleton instance
        public static OMVFileExp Instance => _instance ??= new OMVFileExp(); // Lazy initialization

        private static int SftpPort => int.TryParse(UserCredentials.Instance.SftpPort, out int port) ? port : 22; // Standard SFTP is 22, to be overridden by user

        private SftpClient? _sftpClient; // SFTP client instance

        private string? _currentLocalPath; // Track current local path
        private string? _currentServerPath; // Track current server path

        private enum ActivePane { None, Local, Server } // Enum to track active pane
        private ActivePane _lastActive = ActivePane.None; // Last active pane tracker

        private OMVFileExp()
        {
            InitializeComponent();
            this.Loaded += OMVFileExp_Loaded; // Loaded event handler

            // LocalList and ServerList focus logic
            LocalList.GotKeyboardFocus += (s, e) => { _lastActive = ActivePane.Local; };
            ServerList.GotKeyboardFocus += (s, e) => { _lastActive = ActivePane.Server; };
        }

        private void OMVFileExp_Loaded(object sender, RoutedEventArgs e) // On page load
        {
            if (!UserCredentials.Instance.AreCredentialsSetBrowser()) // Check if credentials are set
            {
                MessageBox.Show("Please set your credentials in the Login window.", "Credentials Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            InitializeSftpConnection(); // Initialize SFTP connection
        }

        private void InitializeSftpConnection() // Initialize SFTP connection method
        {
            if (_sftpClient?.IsConnected == true) return; // Already connected

            // Establish SFTP connection
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
            catch (Exception ex) // Handle connection errors
            {
                MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CloseSftpConnection() // Close SFTP connection method
        {
            if (_sftpClient != null) // If SFTP client exists
            {
                if (_sftpClient.IsConnected) _sftpClient.Disconnect(); // Disconnect if connected
                _sftpClient.Dispose(); // Dispose the client
                _sftpClient = null; // Remove the reference so it can be garbage collected
            }
        }
    }
}