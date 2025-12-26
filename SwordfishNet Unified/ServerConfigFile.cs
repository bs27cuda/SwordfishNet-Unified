namespace SwordfishNet
{
    internal class ServerConfigFile
    {
        // Properties for server configuration stored internally and will not be retained after closing the application
        public string? ServerPath { get; set; }
        public string? SshPort { get; set; }
        public string? SftpPort { get; set; }
        public string? HttpPort { get; set; }
        public string? HttpsPort { get; set; }
    }
}
