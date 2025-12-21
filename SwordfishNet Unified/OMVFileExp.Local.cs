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
        private void LoadDriveRoots()
        {
            foreach (string drive in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new()
                {
                    Header = drive,
                    Tag = drive
                };
                item.Items.Add(null);
                item.Expanded += LocalTreeItem_Expanded;
                LocalTree.Items.Add(item);
            }
        }
        private void LocalTreeItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem tvi)
            {
                try
                {
                    tvi.IsSelected = true;
                    tvi.Focus();
                    _lastActive = ActivePane.Local;
                    e.Handled = true;
                }
                catch {/* ignore focus/selection focus*/}
            }
        }
        private void LocalTree_SelItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItem)
            {
                if (selectedItem.Tag is string path)
                {
                    _currentLocalPath = path;
                    DisplayLocalFiles(path);

                    if (selectedItem.Items.Count == 1 && selectedItem.Items[0] == null)
                    {
                        selectedItem.Items.Clear();
                        LoadLocalSubdirectories(selectedItem, path);
                    }
                }
            }
        }
        private void LoadLocalSubdirectories(TreeViewItem parentItem, string parentPath)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(parentPath))
                {
                    TreeViewItem item = new()
                    {
                        Header = new DirectoryInfo(dir).Name,
                        Tag = dir
                    };
                    item.Items.Add(null);
                    item.Expanded += LocalTreeItem_Expanded;
                    parentItem.Items.Add(item);
                }
            }
            catch { /*ignore access denied errors*/ }
        }
        private void DisplayLocalFiles(string path)
        {
            LocalList.Items.Clear();
            try
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    FileInfo info = new(file);
                    LocalList.Items.Add(new
                    {
                        info.Name,
                        Size = (info.Length / 1024).ToString("N0") + " kb",
                        info.Extension,
                        Modified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
            }
            catch { /*ignore access denied errors*/}
        }
        private bool TryOpenLocal()
        {
            if (LocalList.SelectedItem == null) return false;
            if (string.IsNullOrEmpty(_currentLocalPath)) return false;

            dynamic selectedItem = LocalList.SelectedItem;
            string? fileName = selectedItem?.Name as string;
            if (string.IsNullOrEmpty(fileName)) return false;

            string fullPath = Path.Combine(_currentLocalPath, fileName);
            if (!File.Exists(fullPath)) return false;

            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
            return true;
        }
        private bool TryDeleteLocal()
        {
            if (LocalList.SelectedItem == null) return false;
            if (string.IsNullOrEmpty(_currentLocalPath)) return false;

            dynamic selectedItem = LocalList.SelectedItem;
            string? fileName = selectedItem?.Name as string;
            if (string.IsNullOrEmpty(fileName)) return false;

            string fullPath = Path.Combine(_currentLocalPath, fileName);
            if (!File.Exists(fullPath) && !Directory.Exists(fullPath)) return false;

            var result = MessageBox.Show($"Are you sure you want to send '{fileName}' to the Recycle Bin?",
                                 "Confirm Delete",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return false;

            try
            {
                FileSystem.DeleteFile(
                    fullPath,
                    UIOption.OnlyErrorDialogs,
                    RecycleOption.SendToRecycleBin,
                    UICancelOption.ThrowException
                );
                DisplayLocalFiles(_currentLocalPath);
                return true;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving '{fileName}' to Recycle Bin: {ex.Message}",
                                "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
        }
    }
}