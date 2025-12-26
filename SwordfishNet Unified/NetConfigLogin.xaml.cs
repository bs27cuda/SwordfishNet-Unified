using System.Windows;
using System.Windows.Input;

namespace SwordfishNet_Unified;

public partial class NetConfigLogin : Window
{
    private static NetConfigLogin? _instance; // Singleton instance
    public string? EnteredPassword { get; private set; } // Property to hold the entered password

    public NetConfigLogin()
    {
        InitializeComponent();

        // Set focus to the password box when the window loads
        this.Loaded += (s, e) => NetConfigPasswordBox.Focus();
        this.Closed += (s, e) => _instance = null;
    }
    public static NetConfigLogin Instance => _instance ??= new NetConfigLogin(); // Singleton instance accessor
    private void NetConfigPasswordBox_KeyDown(object sender, KeyEventArgs e) // Handle Enter key press
    {
        if (e.Key == Key.Enter)
        {
            HandleSubmit();
        }
    }
    private void GoButton_Click(object sender, RoutedEventArgs e) => HandleSubmit(); // Handle Go button click
    private void HandleSubmit() // Process the entered password and close the dialog
    {
        EnteredPassword = NetConfigPasswordBox.Password;

        this.DialogResult = true;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e) // Clear password if dialog is canceled
    {
        base.OnClosing(e);
        if (this.DialogResult != true)
        {
            EnteredPassword = null;
        }
    }
}