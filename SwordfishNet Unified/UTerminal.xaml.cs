using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace SwordfishNet_Unified
{
    public partial class UTerminal : Page
    {
        private static UTerminal? _instance;
        public static UTerminal Instance => _instance ??= new UTerminal();

        private SshClient _sshClient;
        private ShellStream _shellStream;
        private List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;
        private string CurrentPrompt => GetPrompt();
        private int PromptLength => CurrentPrompt.Length;

        public UTerminal()
        {
            InitializeComponent();
            TerminalInput.IsEnabled = false;
            this.Loaded += UTerminal_Loaded;
            this.Unloaded += UTerminal_Unloaded;
        }
        private async void UTerminal_Loaded(object sender, RoutedEventArgs e)
        {
            if (!UserCredentials.Instance.AreCredentialsSetSsh())
            {
                MessageBox.Show("Please set your SSH credentials in the Login window.", "Credentials Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            await UTerminal_ConnectAsync();
        }
        public void Shutdown()
        {
            if (_shellStream != null)
            {
                _shellStream.DataReceived -= ShellStream_DataReceived;
            }
            Task.Run(() =>
            {
                try
                {
                    if (_shellStream != null)
                    {
                        _shellStream.Close();
                        _shellStream.Dispose();
                        _shellStream = null;
                    }

                    if (_sshClient != null)
                    {
                        if (_sshClient.IsConnected)
                        {
                            _sshClient.Disconnect();
                        }
                        _sshClient.Dispose();
                        _sshClient = null;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Network Cleanup Error: {ex.Message}");
                }
            });
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    TerminalInput.IsEnabled = false;
                    TerminalInput.Clear();
                    TerminalOutput.Clear();
                    TerminalOutput.AppendText("--- Disconnected ---");
                }
                catch { /* Page might already be unloaded */ }
            }));
        }
        private async Task UTerminal_ConnectAsync()
        {
            string host = UserCredentials.Instance.ServerPath;
            string user = UserCredentials.Instance.Username;
            string pass = UserCredentials.Instance.Password;

            if (!int.TryParse(UserCredentials.Instance.SshPort, out int sshPort))
            {
                AppendToTerminal("## FATAL ## Invalid SSH port configuration.");
                return;
            }

            AppendToTerminal($"Attempting to connect to {host}...");

            try
            {
                await Task.Run(() =>
                {
                    _sshClient = new SshClient(host, sshPort, user, pass);
                    _sshClient.Connect();
                });

                if (_sshClient.IsConnected)
                {
                    _shellStream = _sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                    _shellStream.DataReceived += ShellStream_DataReceived;

                    AppendToTerminal("Connected successfully.");
                    AppendToTerminal("Type commands below and press Enter.");

                    TerminalInput.IsEnabled = true;
                    TerminalInput.Text = CurrentPrompt;
                    TerminalInput.CaretIndex = TerminalInput.Text.Length;
                    TerminalInput.Focus();
                }
            }
            catch (Exception ex)
            {
                AppendToTerminal($"Connection Failed: {ex.Message}");
            }
        }
        private void ShellStream_DataReceived(object sender, ShellDataEventArgs e)
        {
            string output = Encoding.UTF8.GetString(e.Data);
            string cleanOutput = Regex.Replace(output, @"\x1B\[[0-?]*[ -/]*[@-~]", string.Empty);
            cleanOutput = Regex.Replace(cleanOutput, @"\x1B\][^\x07\x0A\x0D]*[\x07\x0A\x0D]", string.Empty);
            cleanOutput = Regex.Replace(cleanOutput, @"\u0000", string.Empty);
            Dispatcher.Invoke(() =>
            {
                TerminalOutput.AppendText(cleanOutput);
                TerminalOutput.ScrollToEnd();
            });
        }
        private void ExecuteCommand()
        {
            if (_shellStream == null || !_shellStream.CanWrite)
            {
                AppendToTerminal("[ERROR] Session disconnected.");
                return;
            }

            string fullInput = TerminalInput.Text;
            string command = fullInput.StartsWith(CurrentPrompt)
                             ? fullInput.Substring(PromptLength).Trim()
                             : fullInput.Trim();

            if (!string.IsNullOrEmpty(command))
            {
                if (!_commandHistory.Contains(command)) _commandHistory.Add(command);
                _historyIndex = _commandHistory.Count;
                _shellStream.WriteLine(command);
            }
            else
            {
                _shellStream.WriteLine(""); // Send empty enter
            }

            TerminalInput.Text = CurrentPrompt;
            TerminalInput.CaretIndex = TerminalInput.Text.Length;
        }
        private void AppendToTerminal(string text)
        {
            TerminalOutput.AppendText($"{text}\n");
            TerminalOutput.ScrollToEnd();
        }
        private string GetPrompt()
        {
            return "$ ";
        }
        private void UTerminal_Unloaded(object sender, RoutedEventArgs e)
        {
            // Left empty to keep terminal connected when navigating tabs
        }
        private void TerminalInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ExecuteCommand();
            }
            else if (e.Key == Key.Up && _commandHistory.Count > 0)
            {
                e.Handled = true;
                _historyIndex = Math.Max(0, _historyIndex - 1);
                TerminalInput.Text = CurrentPrompt + _commandHistory[_historyIndex];
                TerminalInput.CaretIndex = TerminalInput.Text.Length;
            }
            else if (e.Key == Key.Down && _commandHistory.Count > 0)
            {
                e.Handled = true;
                _historyIndex = Math.Min(_commandHistory.Count - 1, _historyIndex + 1);
                TerminalInput.Text = CurrentPrompt + _commandHistory[_historyIndex];
                TerminalInput.CaretIndex = TerminalInput.Text.Length;
            }
        }
        private void TerminalInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Block backspace/delete from eating the prompt
            if (e.Key == Key.Back && TerminalInput.CaretIndex <= PromptLength) e.Handled = true;
            if (e.Key == Key.Delete && TerminalInput.CaretIndex < PromptLength) e.Handled = true;
        }
        private void TerminalInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!TerminalInput.IsEnabled || string.IsNullOrEmpty(TerminalInput.Text))
                return;
            if (TerminalInput.CaretIndex < PromptLength)
            {
                TerminalInput.CaretIndex = PromptLength;
            }
        }
        private void TerminalInput_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Prevent cutting or pasting over the prompt
            if (e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                if (TerminalInput.SelectionStart < PromptLength) e.Handled = true;
            }
        }
    }
}