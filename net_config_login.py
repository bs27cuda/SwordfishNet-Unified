from dataclasses import dataclass
from typing import Optional

@dataclass
class ServerConfigFile:
    server_path: Optional[str] = None
    ssh_port: Optional[str] = None
    sftp_port: Optional[str] = None
    http_port: Optional[str] = None
    https_port: Optional[str] = None

# That's it! 
# In C#, you need { get; set; } syntax. 
# In Python, the @dataclass decorator handles all the setup for you.