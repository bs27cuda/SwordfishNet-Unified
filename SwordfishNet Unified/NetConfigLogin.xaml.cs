using System;
using System.Windows;
using System.Windows.Input;

namespace SwordfishNet_Unified
{
    public partial class NetConfigLogin : Window
    {
        private static NetConfigLogin _instance;
        public string EnteredPassword { get; private set; }

        public NetConfigLogin()
        {
            InitializeComponent();
            NetConfigPasswordBox.Focus();
            this.DataContext = this;
            GoButton.Click += GoButton_Click;
            NetConfigPasswordBox.KeyDown += NetConfigPasswordBox_KeyDown;
            this.Closed += NetConfigLogin_Closed;
        }
        public static NetConfigLogin Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NetConfigLogin();
                }
                return _instance;
            }
        }
        private void NetConfigPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GoButton_Click(sender, e);
            }
        }
        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            EnteredPassword = NetConfigPasswordBox.Password;
            NetConfigPasswordBox.Clear();
            this.DialogResult = true;
            this.Close();
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (this.DialogResult != true)
            {
                EnteredPassword = null;
            }
        }
        private void NetConfigLogin_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }
    }
}