using Org.BouncyCastle.Asn1.Mozilla;

namespace SwordfishNet_Unified
{
    // Singleton class to store user credentials
    // Ensures only one instance exists throughout the application
    // Provides methods to set, clear, and check credentials
    public sealed class UserCredentials
    {
        // Static instance of the singleton
        private static readonly UserCredentials instance = new UserCredentials();

        // Properties to hold user credentials
        public string Username { get; internal set; } = string.Empty;
        public string Password { get; internal set; } = string.Empty;
        public string ServerPath { get; internal set; } = string.Empty;
        public string SshPort { get; internal set; } = "22";
        public string SftpPort { get; internal set; } = "22" ;
        public string HttpPort { get; internal set; } = "80";
        public string HttpsPort { get; internal set; } = "443";
        public string ConfigPassword { get; internal set; } = string.Empty;
        public static string unicorn = "We are all pink!"; // Easter egg

        private UserCredentials() // Private constructor to prevent instantiation and set default port values to avoid null references
        {
            this.SshPort = "22";
            this.SftpPort = "22";
            this.HttpPort = "80";
            this.HttpsPort = "443";
        }
        public static UserCredentials Instance // Public property to access the singleton instance
        {
            get { return instance; } // Return the single instance
        }

        // Method to set user credentials
        public void SetCredentials(string serverPath, string username, string password, string sshPort, string sftpPort, string httpPort, string httpsPort)
        {
            this.ServerPath = serverPath;
            this.Username = username;
            this.Password = password;
            this.SshPort = sshPort;
            this.SftpPort = sftpPort;
            this.HttpPort = httpPort;
            this.HttpsPort = httpsPort;
            this.ConfigPassword = password;
        }
        
        public void ClearCredentials() // Method to clear user credentials
        {
            this.ServerPath = string.Empty;
            this.Username = string.Empty;
            this.Password = string.Empty;
            this.SshPort = "0";
            this.SftpPort = "0";
            this.HttpPort = "0";
            this.HttpsPort = "0";
            this.ConfigPassword = string.Empty;
        }

        public bool AreCredentialsSetBrowser() // Method to check if all credentials are set for browser connections
        {
            return !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password) &&
                   !string.IsNullOrEmpty(ServerPath) &&
                   !string.IsNullOrEmpty(SftpPort);
        }

        public bool AreCredentialsSetSsh() // Method to check if all credentials are set for SSH connections
        {
            return !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password) &&
                   !string.IsNullOrEmpty(ServerPath) &&
                   !string.IsNullOrEmpty(SshPort);
        }

        public bool AreCredentialsSetWeb() // Method to check if all credentials are set for web connections
        {
            return !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password) &&
                   !string.IsNullOrEmpty(ServerPath) &&
                   !string.IsNullOrEmpty(HttpPort) &&
                   !string.IsNullOrEmpty(HttpsPort);
        }
    }
}