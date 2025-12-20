using System;
using System.Windows;

namespace SwordfishNet_Unified
{
    public partial class OMVFileExp
    {
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_lastActive == ActivePane.Server)
                {
                    if (TryOpenServer()) return;
                    if (TryOpenLocal()) return;
                }
                else if (_lastActive == ActivePane.Local)
                {
                    if (TryOpenLocal()) return;
                    if (TryOpenServer()) return;
                }

                if (ServerList.IsKeyboardFocusWithin)
                {
                    if (TryOpenServer()) return;
                }
                if (LocalList.IsKeyboardFocusWithin)
                {
                    if (TryOpenLocal()) return;
                }

                if (ServerList.SelectedItem != null)
                {
                    if (TryOpenServer()) return;
                }
                if (LocalList.SelectedItem != null)
                {
                    if (TryOpenLocal()) return;
                }

                MessageBox.Show("No file selected to open.", "Open file", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open file: {ex.Message}", "Open file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {

        }
        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            bool handled = false;
            if (_lastActive == ActivePane.Server)
            {
                handled = TryDeleteServer();
            }
            else if (_lastActive == ActivePane.Local)
            {
                handled = TryDeleteLocal();
            }
            if (!handled)
            {
                if (ServerList.IsKeyboardFocusWithin) handled = TryDeleteServer();
                else if (LocalList.IsKeyboardFocusWithin) handled = TryDeleteLocal();
            }
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