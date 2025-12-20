using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using Renci.SshNet;

namespace SwordfishNet_Unified
{
public partial class OMVFileExp : Page
    {
        private static OMVFileExp _instance;
        public static OMVFileExp Instance => _instance ??= new OMVFileExp();

        private int SftpPort => int.TryParse(UserCredentials.Instance.SftpPort, out int port) ? port : 20;

        private SftpClient _sftpClient;

        private string _currentLocalPath;
        private string _currentServerPath;

        private enum ActivePane { None, Local, Server }
        private ActivePane _lastActive = ActivePane.None;

        private OMVFileExp()
        {
            InitializeComponent();
            this.Loaded += OMVFileExp_Loaded;
            this.Unloaded += OMVFileExp_Unloaded;

            LocalList.GotKeyboardFocus += (s, e) => { _lastActive = ActivePane.Local; };
            ServerList.GotKeyboardFocus += (s, e) => { _lastActive = ActivePane.Server; };
        }
        private void OMVFileExp_Unloaded(object sender, RoutedEventArgs e)
        {
           // Keep loaded even when navigating away
        }
        private void OMVFileExp_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UserCredentials.Instance.AreCredentialsSetBrowser())
            {
                MessageBox.Show("Please set your file browser credentials in the Login window.", "Credentials Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            InitializeSftpConnection();
        }
        private void InitializeSftpConnection()
        {
            try
            {
                _sftpClient = new SftpClient(UserCredentials.Instance.ServerPath,
                                             SftpPort,
                                             UserCredentials.Instance.Username,
                                             UserCredentials.Instance.Password);
                _sftpClient.Connect();
                LoadServerRoots();
                LoadDriveRoots();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to server: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}