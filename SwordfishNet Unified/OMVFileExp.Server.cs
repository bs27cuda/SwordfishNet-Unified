using Renci.SshNet.Sftp;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SwordfishNet_Unified
{
    public partial class OMVFileExp
    {
        private void LoadServerRoots() // loads the root directories into the server tree view
        {
            if (_sftpClient == null || !_sftpClient.IsConnected) return; // ensure SFTP client is connected
            ServerTree.Items.Clear(); // clear existing items

            // Add root server item
            try
            {
                TreeViewItem rootItem = new()
                {
                    Header = UserCredentials.Instance.ServerPath,
                    Tag = "/" // root path
                };

                rootItem.Items.Add(null); // placeholder for lazy loading
                rootItem.Expanded += ServerTreeItem_Expanded; // attach expanded event handler
                ServerTree.Items.Add(rootItem); // add root item to tree
            }
            catch (Exception ex) // handle exceptions
            {
                MessageBox.Show($"Error accessing network. Check server status and network permissions. Error: {ex.Message}");
            }
        }
        private void ServerTreeItem_Expanded(object sender, RoutedEventArgs e) // handles expansion of tree view items
        {
            if (sender is TreeViewItem tvi) // ensure sender is a TreeViewItem
            {

                // select and focus the expanded item
                try
                {
                    tvi.IsSelected = true;
                    tvi.Focus();
                    _lastActive = ActivePane.Server;
                    e.Handled = true;
                }
                catch {/* ignore focus/selection focus*/} // ignore focus/selection errors
            }
        }
        private void ServerTree_SelItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) // handles selection changes in the server tree view
        {
            if (e.NewValue is TreeViewItem selectedItem) // ensure new value is a TreeViewItem
            {

                // update current server path and display files
                if (selectedItem?.Tag is string path)
                {
                    _currentServerPath = path; // update current path
                    DisplayServerFiles(path, Get_sftpClient()); // display files in the selected directory

                    if (selectedItem?.Items.Count == 1 && selectedItem.Items[0] == null) // check for placeholder
                    {
                        selectedItem.Items.Clear(); // clear placeholder
                        LoadServerSubdirectories(selectedItem, path); // load subdirectories
                    }
                }
            }
        }
        private void LoadServerSubdirectories(TreeViewItem parentItem, string parentPath) // loads subdirectories for a given tree view item
        {
            if (_sftpClient == null || !_sftpClient.IsConnected) return; // ensure SFTP client is connected

            // fetch and add subdirectories
            try
            {
                var items = _sftpClient.ListDirectory(parentPath); // list directory contents
                foreach (SftpFile sftpItem in items.Cast<SftpFile>()) // iterate through items
                {
                    if (sftpItem.Name == "." || sftpItem.Name == "..") continue; // skip current and parent directory entries

                    // add directory items to the tree
                    if (sftpItem.IsDirectory)
                    {
                        TreeViewItem item = new()
                        {
                            Header = sftpItem.Name,
                            Tag = sftpItem.FullName
                        };
                        item.Items.Add(null); // placeholder for lazy loading
                        item.Expanded += ServerTreeItem_Expanded; // attach expanded event handler
                        parentItem.Items.Add(item); // add item to parent
                    }
                }
            }
            catch (Exception ex) // handle exceptions
            {
                MessageBox.Show($"Error accessing directory {parentPath}: {ex.Message}", "sFTP error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private Renci.SshNet.SftpClient? Get_sftpClient() // retrieves the SFTP client instance
        {
            return _sftpClient;
        }
        private void DisplayServerFiles(string path, Renci.SshNet.SftpClient? _sftpClient) // displays files in the server list for a given path
        {
            ServerList.Items.Clear(); // clear existing items

            // fetch and display files
            try
            {
                if (_sftpClient == null) // ensure SFTP client is valid
                {
                    MessageBox.Show($"SFTP client is not connected.", "sFTP error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                // get directory items
                IEnumerable<ISftpFile> items = _sftpClient.ListDirectory(path); // list directory contents
                foreach (SftpFile sftpItem in items.Cast<SftpFile>()) // iterate through items
                {
                    if (sftpItem.IsRegularFile) // only process regular files
                    {
                        ServerList.Items.Add(new
                        {
                            sftpItem.Name,
                            Size = (sftpItem.Length / 1024).ToString("N0") + " kb",
                            Extension = Path.GetExtension(sftpItem.Name),
                            Modified = sftpItem.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                        }); // add file details to the list
                    }
                }
            }
            catch (Exception ex) // handle exceptions
            {
                MessageBox.Show($"Error accessing files in {path}: {ex.Message}", "sFTP error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        private bool TryOpenServer() // attempts to open the selected server file
        {
            if (ServerList.SelectedItem == null) return false; // ensure an item is selected
            if (_sftpClient == null || !_sftpClient.IsConnected) return false; // ensure SFTP client is connected
            if (string.IsNullOrEmpty(_currentServerPath)) return false; // ensure current path is valid

            // get selected file details
            dynamic selectedItem = ServerList.SelectedItem;
            string? fileName = selectedItem?.Name as string;
            if (string.IsNullOrEmpty(fileName)) return false;

            // construct remote file path
            string remotePath = (_currentServerPath == "/")
                ? "/" + fileName
                : _currentServerPath.TrimEnd('/') + "/" + fileName;

            // create a safe temporary file path
            string safeName = fileName;

            // sanitize file name
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                safeName = safeName.Replace(c, '_');
            }
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}_{safeName}");

            // download and open the file
            try
            {
                using (var fs = File.Create(tempFilePath))
                {
                    _sftpClient.DownloadFile(remotePath, fs);
                }
                Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true }); // open the file with the default application
                return true;
            }
            catch // handle exceptions
            {
                MessageBox.Show($"Error opening server file '{fileName}'.", "sFTP open error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private bool TryDeleteServer() // attempts to delete the selected server file
        {
            if (ServerList.SelectedItem == null) return false; // ensure an item is selected
            if (_sftpClient == null || !_sftpClient.IsConnected) return false; // ensure SFTP client is connected
            if (string.IsNullOrEmpty(_currentServerPath)) return false; // ensure current path is valid

            // get selected file details
            dynamic selectedItem = ServerList.SelectedItem;
            string? fileName = selectedItem?.Name as string;
            if (string.IsNullOrEmpty(fileName)) return false;

            // construct remote file path
            string remotePath = (_currentServerPath == "/")
                ? "/" + fileName
                : _currentServerPath.TrimEnd('/') + "/" + fileName;

            // confirm deletion with the user
            var result = MessageBox.Show($"Are you sure you want to delete '{fileName}' from the server?", "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return false;

            try
            {
                _sftpClient.DeleteFile(remotePath); // delete the file

                try
                {
                    DisplayServerFiles(_currentServerPath, Get_sftpClient()); // refresh the server file list
                }
                catch {/* Ignore */ } // ignore UI refresh errors
                return true;
            }
            catch (Exception ex) // handle exceptions
            {
                MessageBox.Show($"Error deleting server file '{fileName}': {ex.Message}", "sFTP delete error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true; // user confirmed action; report handled even if deletion failed
            }
        }
    }
}