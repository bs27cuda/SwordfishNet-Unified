using System;
using System.Windows;

namespace SwordfishNet_Unified
{
    public partial class OMVFileExp
    {
        // File and folder operation button handlers
        private void OpenFile_Click(object sender, RoutedEventArgs e) // Open file button handler
        {
            try
            {
                // Determine which pane was last active and try to open the file accordingly
                if (_lastActive == ActivePane.Server) // If last active pane was server
                {
                    if (TryOpenServer()) return; // Try to open from server pane first
                    if (TryOpenLocal()) return; // If that fails, try to open from local pane
                }
                else if (_lastActive == ActivePane.Local) // If last active pane was local
                {
                    if (TryOpenLocal()) return; // Try to open from local pane first
                    if (TryOpenServer()) return; // If that fails, try to open from server pane
                }

                // If neither pane was last active or no file was opened yet, check focus and selection
                if (ServerList.IsKeyboardFocusWithin) // If server pane has focus
                {
                    if (TryOpenServer()) return;
                }
                if (LocalList.IsKeyboardFocusWithin) // If local pane has focus
                {
                    if (TryOpenLocal()) return;
                }

                // Finally, check if any file is selected in either pane
                if (ServerList.SelectedItem != null) // If a file is selected in server pane
                {
                    if (TryOpenServer()) return;
                }
                if (LocalList.SelectedItem != null) // If a file is selected in local pane
                {
                    if (TryOpenLocal()) return;
                }

                // If no file was opened, inform the user
                MessageBox.Show("No file selected to open.", "Open file", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) // Catch any exceptions that occur during the process
            {
                MessageBox.Show($"Failed to open file: {ex.Message}", "Open file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OpenFolder_Click(object sender, RoutedEventArgs e) // Open folder button handler
        {

        }
        private void DeleteFile_Click(object sender, RoutedEventArgs e) // Delete file button handler
        {
            bool handled = false; // Flag to track if the delete action was handled
            if (_lastActive == ActivePane.Server) // Check if the last active pane was the server pane
            {
                handled = TryDeleteServer();
            }
            else if (_lastActive == ActivePane.Local) // Check if the last active pane was the local pane
            {
                handled = TryDeleteLocal();
            }
            if (!handled) // If not handled yet, check which pane has keyboard focus
            {
                if (ServerList.IsKeyboardFocusWithin) handled = TryDeleteServer(); // Try to delete from server pane
                else if (LocalList.IsKeyboardFocusWithin) handled = TryDeleteLocal(); // Try to delete from local pane
            }

            // If still not handled, check if any file is selected in either pane
            if (!handled)
            {
                MessageBox.Show("No file selected to delete.", "Delete file",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void DeleteFolder_Click(object obj, RoutedEventArgs e)
        {

        }
        private void RenameFile_Click(object sender, RoutedEventArgs e)
        {

        }
        private void RenameFolder_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CopyFileToServer_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CopyFileToLocal_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CopyFolderToServer_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CopyFolderToLocal_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}