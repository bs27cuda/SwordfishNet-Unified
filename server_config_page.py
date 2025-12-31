import sys
import json
import base64
import paramiko
from PyQt6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QGridLayout, 
                             QLabel, QLineEdit, QPushButton, QMessageBox, 
                             QGroupBox, QFrame)
from PyQt6.QtCore import Qt, QTimer

# Constants
NET_CONFIG_FILE = "netconfig.json"

class ServerConfigPage(QWidget):
    _instance = None

    def __init__(self):
        super().__init__()
        if ServerConfigPage._instance is not None:
            raise Exception("Singleton cannot be instantiated more than once.")
        ServerConfigPage._instance = self

        # Internal State (mimicking UserCredentials Singleton)
        self._username = ""
        self._password = ""
        self._server_path = ""
        self._ssh_port = "22"
        self._sftp_port = "22"
        self._http_port = "80"
        self._https_port = "443"
        self._is_logged_in = False

        self.init_ui()

    @staticmethod
    def get_instance():
        if ServerConfigPage._instance is None:
            ServerConfigPage._instance = ServerConfigPage()
        return ServerConfigPage._instance

    def init_ui(self):
        main_layout = QVBoxLayout()
        self.setLayout(main_layout)

        # --- Section 1: Login Credentials ---
        login_group = QGroupBox("User Login")
        login_layout = QGridLayout()
        login_group.setLayout(login_layout)

        login_layout.addWidget(QLabel("Username:"), 0, 0)
        self.username_box = QLineEdit()
        login_layout.addWidget(self.username_box, 0, 1)

        login_layout.addWidget(QLabel("Password:"), 1, 0)
        self.password_box = QLineEdit()
        self.password_box.setEchoMode(QLineEdit.EchoMode.Password)
        self.password_box.returnPressed.connect(self.on_connect_click) # Enter key triggers login
        login_layout.addWidget(self.password_box, 1, 1)

        # Buttons
        btn_layout = QHBoxLayout()
        self.btn_connect = QPushButton("Connect")
        self.btn_connect.clicked.connect(self.on_connect_click)
        btn_layout.addWidget(self.btn_connect)

        self.btn_logoff = QPushButton("Logoff")
        self.btn_logoff.clicked.connect(self.on_logoff_click)
        self.btn_logoff.setEnabled(False)
        btn_layout.addWidget(self.btn_logoff)

        login_layout.addLayout(btn_layout, 2, 0, 1, 2)
        main_layout.addWidget(login_group)

        # --- Section 2: Server Configuration ---
        config_group = QGroupBox("Server Settings")
        config_layout = QGridLayout()
        config_group.setLayout(config_layout)

        # Address
        config_layout.addWidget(QLabel("Server Address (IP/URL):"), 0, 0)
        self.serv_addr_box = QLineEdit()
        config_layout.addWidget(self.serv_addr_box, 0, 1)

        # Ports
        config_layout.addWidget(QLabel("SSH Port:"), 1, 0)
        self.ssh_port_box = QLineEdit("22")
        config_layout.addWidget(self.ssh_port_box, 1, 1)

        config_layout.addWidget(QLabel("SFTP Port:"), 2, 0)
        self.sftp_port_box = QLineEdit("22")
        config_layout.addWidget(self.sftp_port_box, 2, 1)

        config_layout.addWidget(QLabel("HTTP Port:"), 3, 0)
        self.http_port_box = QLineEdit("80")
        config_layout.addWidget(self.http_port_box, 3, 1)

        config_layout.addWidget(QLabel("HTTPS Port:"), 4, 0)
        self.https_port_box = QLineEdit("443")
        config_layout.addWidget(self.https_port_box, 4, 1)

        # Config Buttons
        cfg_btn_layout = QHBoxLayout()
        self.btn_save = QPushButton("Save Config")
        self.btn_save.clicked.connect(self.save_configuration)
        cfg_btn_layout.addWidget(self.btn_save)

        self.btn_load = QPushButton("Load Config")
        self.btn_load.clicked.connect(self.load_configuration)
        cfg_btn_layout.addWidget(self.btn_load)

        config_layout.addLayout(cfg_btn_layout, 5, 0, 1, 2)
        main_layout.addWidget(config_group)
        
        main_layout.addStretch() # Push everything up

    # --- Getters for other pages ---
    def get_hostname(self): return self._server_path
    def get_username(self): return self._username
    def get_password(self): return self._password
    def get_sftp_port(self): return int(self._sftp_port) if self._sftp_port.isdigit() else 22

    # --- Logic Methods ---

    def on_connect_click(self):
        user = self.username_box.text().strip()
        pwd = self.password_box.text()
        addr = self.serv_addr_box.text().strip()

        if not user or not pwd:
            QMessageBox.warning(self, "Input Error", "Please supply user name and password.")
            return

        if not addr:
            QMessageBox.warning(self, "Configuration Error", "Server path is not set.")
            return

        # Attempt SFTP Connection to Verify
        if self.test_sftp_connection(addr, user, pwd):
            # Success: Store credentials
            self._username = user
            self._password = pwd
            self._server_path = addr
            self._ssh_port = self.ssh_port_box.text()
            self._sftp_port = self.sftp_port_box.text()
            self._http_port = self.http_port_box.text()
            self._https_port = self.https_port_box.text()
            
            self._is_logged_in = True
            
            # Update UI
            self.password_box.clear()
            self.btn_logoff.setEnabled(True)
            self.btn_connect.setEnabled(False)
            
            # Enable tabs in MainWindow
            from main_window import MainWindow
            MainWindow.get_instance().set_enabled_tabs(True)
            
            QMessageBox.information(self, "Connected", "Credentials Accepted. Access Granted.")
        else:
             QMessageBox.critical(self, "Connection Refused", "Failed to connect to the server. Credentials refused.")

    def on_logoff_click(self):
        # Clear Data
        self._username = ""
        self._password = ""
        self._is_logged_in = False
        
        # Reset UI
        self.btn_logoff.setEnabled(False)
        self.btn_connect.setEnabled(True)
        
        # Disable tabs
        from main_window import MainWindow
        MainWindow.get_instance().set_enabled_tabs(False)

        # Notify other components (Stub)
        print("System: Logoff sequence complete.")

    def test_sftp_connection(self, host, user, pwd):
        """Uses Paramiko to test if we can login."""
        try:
            port = int(self.sftp_port_box.text())
            transport = paramiko.Transport((host, port))
            transport.connect(username=user, password=pwd)
            transport.close()
            return True
        except Exception as e:
            print(f"Connection Test Failed: {e}")
            return False

    def save_configuration(self):
        data = {
            "server_path": self.serv_addr_box.text(),
            "ssh_port": self.ssh_port_box.text(),
            "sftp_port": self.sftp_port_box.text(),
            "http_port": self.http_port_box.text(),
            "https_port": self.https_port_box.text()
        }
        
        try:
            # Simple save (Base64 encoding acts as light obfuscation)
            json_str = json.dumps(data)
            encoded = base64.b64encode(json_str.encode("utf-8")).decode("utf-8")
            
            with open(NET_CONFIG_FILE, "w") as f:
                f.write(encoded)
            
            # Visual feedback (Green flash)
            self.btn_save.setStyleSheet("background-color: lightgreen")
            QTimer.singleShot(1000, lambda: self.btn_save.setStyleSheet(""))
            
        except Exception as e:
            QMessageBox.critical(self, "Save Error", str(e))

    def load_configuration(self):
        try:
            with open(NET_CONFIG_FILE, "r") as f:
                encoded = f.read()
                decoded = base64.b64decode(encoded).decode("utf-8")
                data = json.loads(decoded)
                
                self.serv_addr_box.setText(data.get("server_path", ""))
                self.ssh_port_box.setText(data.get("ssh_port", "22"))
                self.sftp_port_box.setText(data.get("sftp_port", "22"))
                self.http_port_box.setText(data.get("http_port", "80"))
                self.https_port_box.setText(data.get("https_port", "443"))
                
                QMessageBox.information(self, "Loaded", "Configuration loaded successfully.")
        except FileNotFoundError:
             QMessageBox.warning(self, "Load Error", "No configuration file found.")
        except Exception as e:
             QMessageBox.critical(self, "Load Error", f"Failed to load: {str(e)}")