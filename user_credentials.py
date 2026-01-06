class UserCredentials:
    _instance = None
    
    # Static class variable (shared by all instances)
    unicorn = "We are all pink!"

    def __new__(cls):
        """
        This magic method ensures that whenever you call UserCredentials(),
        you get the SAME instance back. This implements the Singleton pattern.
        """
        if cls._instance is None:
            cls._instance = super(UserCredentials, cls).__new__(cls)
            cls._instance._initialize_defaults()
        return cls._instance

    def _initialize_defaults(self):
        """Sets the initial default values (Private method)"""
        self.username = ""
        self.password = ""
        self.server_path = ""
        self.ssh_port = "22"
        self.sftp_port = "22"
        self.http_port = "80"
        self.https_port = "443"
        self.config_password = ""

    @classmethod
    def get_instance(cls):
        """Helper to match C# .Instance style, though UserCredentials() works too."""
        return cls()

    def set_credentials(self, server_path, username, password, ssh_port, sftp_port, http_port, https_port):
        self.server_path = server_path
        self.username = username
        self.password = password
        self.ssh_port = ssh_port
        self.sftp_port = sftp_port
        self.http_port = http_port
        self.https_port = https_port
        # In your C# code, ConfigPassword was set to the same as Password here
        self.config_password = password

    def clear_credentials(self):
        self.server_path = ""
        self.username = ""
        self.password = ""
        # Your C# logic sets these to "0" on clear, but "22" on init. Preserved that here.
        self.ssh_port = "0"
        self.sftp_port = "0"
        self.http_port = "0"
        self.https_port = "0"
        self.config_password = ""

    def are_credentials_set_browser(self) -> bool:
        # Checks if all required fields have values (truthy check)
        return all([self.username, self.password, self.server_path, self.sftp_port])

    def are_credentials_set_ssh(self) -> bool:
        return all([self.username, self.password, self.server_path, self.ssh_port])

    def are_credentials_set_web(self) -> bool:
        return all([self.username, self.password, self.server_path, self.http_port, self.https_port])

# --- Usage Example ---
if __name__ == "__main__":
    # Test Singleton behavior
    creds1 = UserCredentials()
    creds2 = UserCredentials()

    print(f"Is creds1 the same object as creds2? {creds1 is creds2}")  # Should be True
    print(f"Static Unicorn check: {UserCredentials.unicorn}")
    
    creds1.set_credentials("192.168.1.1", "admin", "secret", "22", "22", "80", "443")
    print(f"Username in creds2: {creds2.username}")  # Should print "admin"