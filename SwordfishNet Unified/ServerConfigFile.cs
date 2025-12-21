namespace SwordfishNet
{
    internal class ServerConfigFile
    {
        public string? ServerPath { get; set; }
        public string? SshPort { get; set; }
        public string? SftpPort { get; set; }
        public string? HttpPort { get; set; }
        public string? HttpsPort { get; set; }
    }
}
