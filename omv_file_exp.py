import os
import paramiko
from PyQt6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QListWidget, 
                             QLabel, QMessageBox, QSplitter, QAbstractItemView)
from PyQt6.QtCore import Qt

# Import the Config Page to access credentials
from server_config_page import ServerConfigPage

class OMVFileExp(QWidget):
    _instance = None
    
    def __init__(self):
        super().__init__()
        if OMVFileExp._instance is not None:
            raise Exception("Singleton cannot be instantiated more than once.")
        OMVFileExp._instance = self

        # --- Data Members ---
        self.sftp_client = None
        self.transport = None
        self.current_local_path = None
        self.current_server_path = None
        self.last_active_pane = "None"  # "Local" or "Server"

        # --- UI Layout ---
        self.init_ui()

        # --- Auto-Connect on Load (simulated by checking visibility or explicit call) ---
        # In PyQt, we usually call this explicitly or when the tab is selected.
        # For now, we will add a public method to trigger it.

    @staticmethod
    def get_instance():
        if OMVFileExp._instance is None:
            OMVFileExp._instance = OMVFileExp()
        return OMVFileExp._instance

    def init_ui(self):
        """Builds the 2-pane File Explorer UI"""
        main_layout = QVBoxLayout()
        self.setLayout(main_layout)

        # 1. Status / Header
        self.lbl_status = QLabel("Status: Disconnected")
        main_layout.addWidget(self.lbl_status)

        # 2. Main Splitter (Left: Local, Right: Server)
        splitter = QSplitter(Qt.Orientation.Horizontal)
        
        # --- Local Pane ---
        local_widget = QWidget()
        local_layout = QVBoxLayout()
        local_widget.setLayout(local_layout)
        local_layout.addWidget(QLabel("<b>Local Files</b>"))
        
        self.local_list = QListWidget()
        self.local_list.setSelectionMode(QAbstractItemView.SelectionMode.SingleSelection)
        # Event: Track Focus
        self.local_list.itemClicked.connect(lambda: self.set_active_pane("Local"))
        local_layout.addWidget(self.local_list)
        
        splitter.addWidget(local_widget)

        # --- Server Pane ---
        server_widget = QWidget()
        server_layout = QVBoxLayout()
        server_widget.setLayout(server_layout)
        server_layout.addWidget(QLabel("<b>Server Files (SFTP)</b>"))
        
        self.server_list = QListWidget()
        self.server_list.setSelectionMode(QAbstractItemView.SelectionMode.SingleSelection)
        # Event: Track Focus
        self.server_list.itemClicked.connect(lambda: self.set_active_pane("Server"))
        server_layout.addWidget(self.server_list)

        splitter.addWidget(server_widget)

        # Add splitter to main layout
        main_layout.addWidget(splitter)

        # Set initial splitter ratio (50/50)
        splitter.setSizes([400, 400])

    def set_active_pane(self, pane_name):
        self._last_active = pane_name
        # Optional: Visual feedback (highlight border, etc.)
        # print(f"Active Pane: {pane_name}")

    # --- Connection Logic (Ported from C# InitializeSftpConnection) ---
    def initialize_sftp_connection(self):
        """Reads credentials from ServerConfigPage and connects via Paramiko."""
        
        # 1. Get Credentials
        config = ServerConfigPage.get_instance()
        
        # Note: We assume these attributes exist on your ServerConfigPage. 
        # If you named them differently (e.g., self.input_ip.text()), we might need to adjust this.
        host = config.get_hostname()
        user = config.get_username()
        pwd = config.get_password()
        port = 22 # Default SFTP port

        if not host or not user:
            QMessageBox.warning(self, "Credentials Required", "Please set your credentials in the Server Config tab.")
            return

        # 2. Connect
        if self.transport and self.transport.is_active():
            return # Already connected

        try:
            self.lbl_status.setText(f"Status: Connecting to {host}...")
            
            # Paramiko Transport Setup
            self.transport = paramiko.Transport((host, port))
            self.transport.connect(username=user, password=pwd)
            self.sftp_client = paramiko.SFTPClient.from_transport(self.transport)

            self.lbl_status.setText(f"Status: Connected to {host} as {user}")
            
            # 3. Load Initial Paths
            self.load_server_roots()
            self.load_drive_roots()

        except Exception as e:
            self.lbl_status.setText("Status: Connection Failed")
            QMessageBox.critical(self, "Connection Error", f"Failed to connect: {str(e)}")
            self.close_sftp_connection()

    def close_sftp_connection(self):
        """Closes the SFTP connection safely."""
        if self.sftp_client:
            self.sftp_client.close()
            self.sftp_client = None
        
        if self.transport:
            self.transport.close()
            self.transport = None
            
        self.lbl_status.setText("Status: Disconnected")

    # --- Placeholders for Logic files (Local.cs / Server.cs) ---
    def load_server_roots(self):
        """
        Placeholder: Will populate self.server_list with remote folders.
        We will implement this when we convert OMVFileExp.Server.cs
        """
        self.server_list.clear()
        if self.sftp_client:
            try:
                # Simple test: List root directory
                files = self.sftp_client.listdir('.')
                for f in files:
                    self.server_list.addItem(f)
            except Exception as e:
                self.server_list.addItem(f"Error listing files: {e}")

    def load_drive_roots(self):
        """
        Placeholder: Will populate self.local_list with local drives.
        We will implement this when we convert OMVFileExp.Local.cs
        """
        self.local_list.clear()
        # Simple test: List current working directory
        try:
            files = os.listdir('.')
            for f in files:
                self.local_list.addItem(f)
        except Exception as e:
            self.local_list.addItem(f"Error listing local files: {e}")