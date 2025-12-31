using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace SwordfishNet_Unified
{
    public partial class OMVFileExp
    {
        private void DisplayLocalFiles(string? path)
        {
            try
            {
                LocalList.Items.Clear();

                if (string.IsNullOrEmpty(path))
                {
                    ShowThisPC(); // Helper to show drives
                    return;
                }

                _currentLocalPath = path;
                LocalPathBox.Text = path;

                DirectoryInfo di = new(path);

                // Load Folders
                foreach (var dir in di.GetDirectories())
                {
                    LocalList.Items.Add(new
                    {
                        Icon = "📁",
                        dir.Name,
                        Size = "<DIR>",
                        Modified = dir.LastWriteTime.ToString("g"),
                        IsFolder = true,
                        IsDrive = false,
                        FullPath = dir.FullName
                    });
                }

                // Load Files
                foreach (var file in di.GetFiles())
                {
                    LocalList.Items.Add(new
                    {
                        Icon = "📄",
                        file.Name,
                        Size = (file.Length / 1024).ToString("N0") + " KB",
                        Modified = file.LastWriteTime.ToString("g"),
                        IsFolder = false,
                        IsDrive = false,
                        FullPath = file.FullName
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access Denied to this folder.", "Security", MessageBoxButton.OK, MessageBoxImage.Stop);
                // Optional: Go back to This PC or parent
                DisplayLocalFiles(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ShowThisPC()
        {
            _currentLocalPath = null;
            LocalPathBox.Text = "This PC";
            foreach (string drive in Directory.GetLogicalDrives())
            {
                var di = new DriveInfo(drive);
                if (di.IsReady)
                {
                    LocalList.Items.Add(new
                    {
                        Icon = "💻",
                        Name = drive,
                        Size = di.DriveType.ToString(),
                        Modified = "",
                        IsFolder = true,
                        IsDrive = true,
                        FullPath = drive
                    });
                }
            }
        }
        private void LocalList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Use dynamic to access the properties of the anonymous object we added to the list
            dynamic selected = LocalList.SelectedItem;
            if (selected == null) return;

            // Check if the item is a folder or a drive
            if (selected.IsFolder)
            {
                string newPath = selected.FullPath;

                // Ensure the path ends with a backslash if it's a drive (e.g., "C:" -> "C:\")
                if (!(!selected.IsDrive || newPath.EndsWith(@"\\")))
                {
                    newPath += "\\";
                }

                DisplayLocalFiles(newPath);
            }
            else
            {
                TryOpenLocal(); // Existing logic for files
            }
        }
        private void LocalUp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentLocalPath)) return;

            DirectoryInfo parent = Directory.GetParent(_currentLocalPath);
            if (parent != null)
            {
                DisplayLocalFiles(parent.FullName);
            }
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