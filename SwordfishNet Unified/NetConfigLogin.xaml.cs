using System.Windows;
using System.Windows.Input;

namespace SwordfishNet_Unified;

public partial class NetConfigLogin : Window
{
    private static NetConfigLogin? _instance;
    public string? EnteredPassword { get; private set; }

    public NetConfigLogin()
    {
        InitializeComponent();
        this.Loaded += (s, e) => NetConfigPasswordBox.Focus();
        this.Closed += (s, e) => _instance = null;
    }
    public static NetConfigLogin Instance => _instance ??= new NetConfigLogin();
    private void NetConfigPasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            HandleSubmit();
        }
    }
    private void GoButton_Click(object sender, RoutedEventArgs e) => HandleSubmit();
        private void HandleSubmit()
    {
        EnteredPassword = NetConfigPasswordBox.Password;

        this.DialogResult = true;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        if (this.DialogResult != true)
        {
            EnteredPassword = null;
        }
    }
}