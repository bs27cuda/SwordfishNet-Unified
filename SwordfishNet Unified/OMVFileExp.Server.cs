using Renci.SshNet.Sftp;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SwordfishNet_Unified
{
    public partial class OMVFileExp
    {
        private void LoadServerRoots()
        {
            if (_sftpClient == null || !_sftpClient.IsConnected) return;
            ServerTree.Items.Clear();

            try
            {
                TreeViewItem rootItem = new()
                {
                    Header = UserCredentials.Instance.ServerPath,
                    Tag = "/"
                };

                rootItem.Items.Add(null);
                rootItem.Expanded += ServerTreeItem_Expanded;
                ServerTree.Items.Add(rootItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accessing network. Check server status and network permissions. Error: {ex.Message}");
            }
        }
        private void ServerTreeItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem tvi)
            {
                try
                {
                    tvi.IsSelected = true;
                    tvi.Focus();
                    _lastActive = ActivePane.Server;
                    e.Handled = true;
                }
                catch {/* ignore focus/selection focus*/}
            }
        }
        private void ServerTree_SelItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItem)
            {
                if (selectedItem?.Tag is string path)
                {
                    _currentServerPath = path;
                    DisplayServerFiles(path, Get_sftpClient());

                    if (selectedItem?.Items.Count == 1 && selectedItem.Items[0] == null)
                    {
                        selectedItem.Items.Clear();
                        LoadServerSubdirectories(selectedItem, path);
                    }
                }
            }
        }
        private void LoadServerSubdirectories(TreeViewItem parentItem, string parentPath)
        {
            if (_sftpClient == null || !_sftpClient.IsConnected) return;

            try
            {
                var items = _sftpClient.ListDirectory(parentPath);
                foreach (SftpFile sftpItem in items.Cast<SftpFile>())
                {
                    if (sftpItem.Name == "." || sftpItem.Name == "..") continue;

                    if (sftpItem.IsDirectory)
                    {
                        TreeViewItem item = new()
                        {
                            Header = sftpItem.Name,
                            Tag = sftpItem.FullName
                        };
                        item.Items.Add(null);
                        item.Expanded += ServerTreeItem_Expanded;
                        parentItem.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accessing directory {parentPath}: {ex.Message}", "sFTP error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private Renci.SshNet.SftpClient? Get_sftpClient()
        {
            return _sftpClient;
        }
        private void DisplayServerFiles(string path, Renci.SshNet.SftpClient? _sftpClient)
        {
            ServerList.Items.Clear();
            try
            {
                if (_sftpClient == null)
                {
                    MessageBox.Show($"SFTP client is not connected.", "sFTP error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                IEnumerable<ISftpFile> items = _sftpClient.ListDirectory(path);
                foreach (SftpFile sftpItem in items.Cast<SftpFile>())
                {
                    if (sftpItem.IsRegularFile)
                    {
                        ServerList.Items.Add(new
                        {
                            sftpItem.Name,
                            Size = (sftpItem.Length / 1024).ToString("N0") + " kb",
                            Extension = Path.GetExtension(sftpItem.Name),
                            Modified = sftpItem.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accessing files in {path}: {ex.Message}", "sFTP error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        private bool TryOpenServer()
        {
            if (ServerList.SelectedItem == null) return false;
            if (_sftpClient == null || !_sftpClient.IsConnected) return false;
            if (string.IsNullOrEmpty(_currentServerPath)) return false;

            dynamic selectedItem = ServerList.SelectedItem;
            string? fileName = selectedItem?.Name as string;
            if (string.IsNullOrEmpty(fileName)) return false;

            string remotePath = (_currentServerPath == "/")
                ? "/" + fileName
                : _currentServerPath.TrimEnd('/') + "/" + fileName;

            string safeName = fileName;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                safeName = safeName.Replace(c, '_');
            }
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}_{safeName}");

            try
            {
                using (var fs = File.Create(tempFilePath))
                {
                    _sftpClient.DownloadFile(remotePath, fs);
                }
                Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool TryDeleteServer()
        {
            if (ServerList.SelectedItem == null) return false;
            if (_sftpClient == null || !_sftpClient.IsConnected) return false;
            if (string.IsNullOrEmpty(_currentServerPath)) return false;

            dynamic selectedItem = ServerList.SelectedItem;
            string? fileName = selectedItem?.Name as string;
            if (string.IsNullOrEmpty(fileName)) return false;

            string remotePath = (_currentServerPath == "/")
                ? "/" + fileName
                : _currentServerPath.TrimEnd('/') + "/" + fileName;

            var result = MessageBox.Show($"Are you sure you want to delete '{fileName}' from the server?", "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return false;

            try
            {
                _sftpClient.DeleteFile(remotePath);

                try
                {
                    DisplayServerFiles(_currentServerPath, Get_sftpClient());
                }
                catch
                {
                    // ignore UI refresh errors
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting server file '{fileName}': {ex.Message}", "sFTP delete error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true; // user confirmed action; report handled even if deletion failed
            }
        }
    }
}