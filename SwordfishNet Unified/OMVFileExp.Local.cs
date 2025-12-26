using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic.FileIO;

namespace SwordfishNet_Unified
{
    public partial class OMVFileExp
    {
        private void LoadDriveRoots() // Load local drive roots into the tree view
        {
            foreach (string drive in Directory.GetLogicalDrives()) // Iterate through each logical drive
            {
                TreeViewItem item = new() // Create a new tree view item for the drive
                {
                    Header = drive,
                    Tag = drive
                };
                item.Items.Add(null); // Placeholder for lazy loading of subdirectories
                item.Expanded += LocalTreeItem_Expanded; // Attach expanded event handler
                LocalTree.Items.Add(item); // Add the drive item to the tree view
            }
        }
        private void LocalTreeItem_Expanded(object sender, RoutedEventArgs e) // Handle expansion of tree view items
        {
            if (sender is TreeViewItem tvi) // Check if the sender is a TreeViewItem
            {
                // If the item has only the placeholder, load its subdirectories
                try
                {
                    tvi.IsSelected = true;
                    tvi.Focus();
                    _lastActive = ActivePane.Local;
                    e.Handled = true;
                }
                catch {/*ignore error*/} // Ignore any exceptions that occur during selection/focus
            }
        }
        private void LocalTree_SelItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) // Handle selection changes in the local tree view
        {
            if (e.NewValue is TreeViewItem selectedItem) // Check if the new selected item is a TreeViewItem
            {
                if (selectedItem.Tag is string path) // Get the path from the item's tag
                {
                    _currentLocalPath = path;
                    DisplayLocalFiles(path);

                    // If the item has only the placeholder, load its subdirectories
                    if (selectedItem.Items.Count == 1 && selectedItem.Items[0] == null)
                    {
                        selectedItem.Items.Clear();
                        LoadLocalSubdirectories(selectedItem, path);
                    }
                }
            }
        }
        private void LoadLocalSubdirectories(TreeViewItem parentItem, string parentPath) // Load subdirectories for a given tree view item
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(parentPath)) // Iterate through each subdirectory
                {
                    TreeViewItem item = new() // Create a new tree view item for the subdirectory
                    {
                        Header = new DirectoryInfo(dir).Name,
                        Tag = dir
                    };
                    item.Items.Add(null); // Placeholder for lazy loading of further subdirectories
                    item.Expanded += LocalTreeItem_Expanded; // Attach expanded event handler
                    parentItem.Items.Add(item); // Add the subdirectory item to the parent item
                }
            }
            catch { /*ignore access denied errors*/ } // Ignore any exceptions that occur during directory access
        }
        private void DisplayLocalFiles(string path) // Display files in the local list view for a given path
        {
            LocalList.Items.Clear(); // Clear existing items in the list view

            // Load directories
            try
            {
                foreach (string file in Directory.GetFiles(path)) // Iterate through each file in the directory
                {
                    FileInfo info = new(file); // Get file information
                    LocalList.Items.Add(new
                    {
                        info.Name,
                        Size = (info.Length / 1024).ToString("N0") + " kb",
                        info.Extension,
                        Modified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                    }); // Add file details to the list view
                }
            }
            catch { /*ignore access denied errors*/} // Ignore any exceptions that occur during file access
        }
        private bool TryOpenLocal() // Try to open the selected file in the local list view
        {
            if (LocalList.SelectedItem == null) return false; // No item selected
            if (string.IsNullOrEmpty(_currentLocalPath)) return false; // No current path

            dynamic selectedItem = LocalList.SelectedItem; // Get the selected item
            string? fileName = selectedItem?.Name as string; // Get the file name from the selected item
            if (string.IsNullOrEmpty(fileName)) return false; // No valid file name

            string fullPath = Path.Combine(_currentLocalPath, fileName); // Construct the full file path
            if (!File.Exists(fullPath)) return false; // File does not exist

            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true }); // Open the file using the default application
            return true;
        }
        private bool TryDeleteLocal() // Try to delete the selected file in the local list view
        {
            if (LocalList.SelectedItem == null) return false; // No item selected
            if (string.IsNullOrEmpty(_currentLocalPath)) return false; // No current path

            dynamic selectedItem = LocalList.SelectedItem; // Get the selected item
            string? fileName = selectedItem?.Name as string; // Get the file name from the selected item
            if (string.IsNullOrEmpty(fileName)) return false; // No valid file name

            string fullPath = Path.Combine(_currentLocalPath, fileName); // Construct the full file path
            if (!File.Exists(fullPath) && !Directory.Exists(fullPath)) return false; // File or directory does not exist

            // Confirm deletion with the user
            var result = MessageBox.Show($"Are you sure you want to send '{fileName}' to the Recycle Bin?",
                                 "Confirm Delete",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

            // If user selects No, cancel the deletion
            if (result != MessageBoxResult.Yes) return false;

            try // Try to delete the file by sending it to the Recycle Bin
            {
                FileSystem.DeleteFile(
                    fullPath,
                    UIOption.OnlyErrorDialogs,
                    RecycleOption.SendToRecycleBin,
                    UICancelOption.ThrowException
                );
                DisplayLocalFiles(_currentLocalPath); // Refresh the local file list
                return true; // Deletion successful
            }
            catch (OperationCanceledException) // User canceled the operation
            {
                return true;
            }
            catch (Exception ex) // Handle any other exceptions that occur during deletion
            {
                MessageBox.Show($"Error moving '{fileName}' to Recycle Bin: {ex.Message}",
                                "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
        }
    }
}