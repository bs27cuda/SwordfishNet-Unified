using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SwordfishNet_Unified
{
    public partial class UTerminal : Page
    {
        [GeneratedRegex(@"\x1B\[[0-?]*[ -/]*[@-~]")] // ANSI escape codes
        private static partial Regex AnsiEscapeRegex(); // Matches ANSI escape sequences

        [GeneratedRegex(@"\x1B\][^\x07\x0A\x0D]*[\x07\x0A\x0D]")] // Control sequences
        private static partial Regex ControlSequenceRegex(); // Matches control sequences

        [GeneratedRegex(@"\u0000")] // Null characters
        private static partial Regex NullCharRegex(); // Matches null characters

        private static UTerminal? _instance; // Singleton instance
        public static UTerminal Instance => _instance ??= new UTerminal(); // Accessor for singleton instance

        // SSH client and shell stream
        private SshClient? _sshClient;
        private ShellStream? _shellStream;

        // Command history management
        private readonly List<string> _commandHistory = [];
        private int _historyIndex = -1;

        // Prompt management
        private static string CurrentPrompt => GetPrompt();
        private static int PromptLength => CurrentPrompt.Length;

        public UTerminal()
        {
            InitializeComponent();
            TerminalInput!.IsEnabled = false; // Disable input until connected
            this.Loaded += UTerminal_Loaded; // Event handler for page load
            this.Unloaded += UTerminal_Unloaded; // Event handler for page unload
        }

        private async void UTerminal_Loaded(object sender, RoutedEventArgs e) // Event handler for page load
        {
            if (!UserCredentials.Instance.AreCredentialsSetSsh()) // Check if SSH credentials are set
            {
                MessageBox.Show("Please set your SSH credentials in the Login window.", "Credentials Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            await UTerminal_ConnectAsync(); // Attempt to connect to SSH server
        }

        public void Shutdown() // Cleanup method to disconnect and dispose resources
        {
            if (_shellStream != null) // Unsubscribe from data received event
            {
                _shellStream.DataReceived -= ShellStream_DataReceived;
            }

            // Perform network cleanup on a separate task
            Task.Run(() =>
            {
                try
                {
                    if (_shellStream != null) // Dispose shell stream
                    {
                        _shellStream.Close();
                        _shellStream.Dispose();
                        _shellStream = null;
                    }

                    if (_sshClient != null) // Disconnect and dispose SSH client
                    {
                        if (_sshClient.IsConnected)
                        {
                            _sshClient.Disconnect();
                        }
                        _sshClient.Dispose();
                        _sshClient = null;
                    }
                }
                catch (Exception ex) // Log any errors during cleanup
                {
                    System.Diagnostics.Debug.WriteLine($"Network Cleanup Error: {ex.Message}");
                }
            });

            Dispatcher.BeginInvoke(new Action(() => // Update UI on the main thread
            {
                try
                {
                    TerminalInput!.IsEnabled = false; // Disable input
                    TerminalInput.Clear(); // Clear input box
                    TerminalOutput!.Clear(); // Clear output box
                    TerminalOutput.AppendText("--- Disconnected ---"); // Indicate disconnection
                }
                catch { /* Page might already be unloaded */ } // Ignore exceptions during UI update
            }));
        }

        private async Task UTerminal_ConnectAsync() // Asynchronous method to connect to SSH server
        {
            // Retrieve connection details from user credentials
            string host = UserCredentials.Instance.ServerPath;
            string user = UserCredentials.Instance.Username;
            string pass = UserCredentials.Instance.Password;

            if (!int.TryParse(UserCredentials.Instance.SshPort, out int sshPort)) // Validate SSH port
            {
                AppendToTerminal("## FATAL ## Invalid SSH port configuration.");
                return;
            }

            AppendToTerminal($"Attempting to connect to {host}..."); // Indicate connection attempt

            // Establish SSH connection on a separate task
            try
            {
                await Task.Run(() =>
                {
                    _sshClient = new SshClient(host, sshPort, user, pass); // Initialize SSH client
                    _sshClient.Connect(); // Connect to SSH server
                });

                if (_sshClient != null && _sshClient.IsConnected) // Check if connection was successful
                {
                    _shellStream = _sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024); // Create shell stream
                    _shellStream.DataReceived += ShellStream_DataReceived; // Subscribe to data received event

                    AppendToTerminal("Connected successfully.");
                    AppendToTerminal("Type commands below and press Enter.");

                    TerminalInput!.IsEnabled = true; // Enable input box
                    TerminalInput.Text = CurrentPrompt; // Set initial prompt
                    TerminalInput.CaretIndex = TerminalInput.Text.Length; // Move caret to end
                    TerminalInput.Focus(); // Focus input box
                }
            }
            catch (Exception ex) // Handle connection errors
            {
                AppendToTerminal($"Connection Failed: {ex.Message}");
            }
        }

        private void ShellStream_DataReceived(object? sender, ShellDataEventArgs e) // Event handler for data received from shell stream
        {
            string output = Encoding.UTF8.GetString(e.Data); // Decode received data
            string cleanOutput = AnsiEscapeRegex().Replace(output, string.Empty); // Remove ANSI escape codes
            cleanOutput = ControlSequenceRegex().Replace(cleanOutput, string.Empty); // Remove control sequences
            cleanOutput = NullCharRegex().Replace(cleanOutput, string.Empty); // Remove null characters
            Dispatcher.Invoke(() => // Update UI on the main thread
            {
                TerminalOutput!.AppendText(cleanOutput);
                TerminalOutput.ScrollToEnd();
            });
        }

        private void ExecuteCommand() // Method to execute command from input box
        {
            if (_shellStream == null || !_shellStream.CanWrite) // Check if shell stream is writable
            {
                AppendToTerminal("[ERROR] Session disconnected."); // Indicate error
                return;
            }

            // Extract command by removing prompt
            string fullInput = TerminalInput!.Text;
            string command = fullInput.StartsWith(CurrentPrompt)
                             ? fullInput[PromptLength..].Trim()
                             : fullInput.Trim();

            // Send command to shell stream
            if (!string.IsNullOrEmpty(command))
            {
                if (!_commandHistory.Contains(command)) _commandHistory.Add(command); // Add to history if not duplicate
                _historyIndex = _commandHistory.Count; // Reset history index
                _shellStream.WriteLine(command); // Send command
            }
            else // Handle empty command
            {
                _shellStream.WriteLine(string.Empty); // Send empty enter
            }

            TerminalInput.Text = CurrentPrompt; // Reset input box to prompt
            TerminalInput.CaretIndex = TerminalInput.Text.Length; // Move caret to end
        }

        private void AppendToTerminal(string text) // Method to append text to terminal output
        {
            TerminalOutput!.AppendText($"{text}\n"); // Append text with newline
            TerminalOutput.ScrollToEnd(); // Scroll to end
        }

        private static string GetPrompt() => "$ "; // Static method to get the terminal prompt

        private void UTerminal_Unloaded(object sender, RoutedEventArgs e) // Event handler for page unload
        {
            // Intentionally empty to keep terminal connected when navigating tabs
        }

        private void TerminalInput_KeyDown(object sender, KeyEventArgs e) // Event handler for key down in input box
        {
            if (e.Key == Key.Enter) // Execute command on Enter key
            {
                e.Handled = true;
                ExecuteCommand();
            }
            else if (e.Key == Key.Up && _commandHistory.Count > 0) // Navigate command history on Up key
            {
                e.Handled = true;
                _historyIndex = Math.Max(0, _historyIndex - 1);
                TerminalInput!.Text = CurrentPrompt + _commandHistory[_historyIndex];
                TerminalInput.CaretIndex = TerminalInput.Text.Length;
            }
            else if (e.Key == Key.Down && _commandHistory.Count > 0) // Navigate command history on Down key
            {
                e.Handled = true;
                _historyIndex = Math.Min(_commandHistory.Count - 1, _historyIndex + 1);
                TerminalInput!.Text = CurrentPrompt + _commandHistory[_historyIndex];
                TerminalInput.CaretIndex = TerminalInput.Text.Length;
            }
        }

        private void TerminalInput_PreviewKeyDown(object sender, KeyEventArgs e) // Event handler for preview key down in input box
        {
            // Block backspace/delete from eating the prompt
            if (e.Key == Key.Back && TerminalInput!.CaretIndex <= PromptLength) e.Handled = true;
            if (e.Key == Key.Delete && TerminalInput.CaretIndex < PromptLength) e.Handled = true;
        }

        private void TerminalInput_SelectionChanged(object sender, RoutedEventArgs e) // Event handler for selection change in input box
        {
            if (!TerminalInput!.IsEnabled || string.IsNullOrEmpty(TerminalInput.Text)) // No action if disabled or empty
                return;
            if (TerminalInput.CaretIndex < PromptLength) // Prevent caret from moving before prompt
            {
                TerminalInput.CaretIndex = PromptLength;
            }
        }

        private void TerminalInput_PreviewExecuted(object sender, ExecutedRoutedEventArgs e) // Event handler for preview executed commands in input box
        {
            // Prevent cutting or pasting over the prompt
            if (e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                if (TerminalInput!.SelectionStart < PromptLength) e.Handled = true; // Block if selection starts before prompt
            }
        }
    }
}