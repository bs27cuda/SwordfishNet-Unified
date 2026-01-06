import os
import json
import paramiko
from PyQt6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QLabel, 
                             QLineEdit, QPushButton, QMessageBox, QGridLayout, QGroupBox)
from PyQt6.QtCore import Qt, QTimer
from PyQt6.QtGui import QColor

# Import our helper classes
from user_credentials import UserCredentials
from encryption_helper import EncryptionHelper
from server_config_file import ServerConfigFile
from net_config_login import NetConfigLogin

class ServerConfigPage(QWidget):
    _instance = None

    NET_CONFIG_FILENAME = "netconfig.dat"

    def __init__(self):
        super().__init__()
        ServerConfigPage._instance = self
        self.init_ui()

    @staticmethod
    def get_instance():
        if ServerConfigPage._instance is None:
            ServerConfigPage._instance = ServerConfigPage()
        return ServerConfigPage._instance

    def init_ui(self):
        # --- Main Layout ---
        main_layout = QVBoxLayout()
        self.setLayout(main_layout)

        # --- 1. Connection Details Group ---
        conn_group = QGroupBox("Connection Settings")
        conn_layout = QGridLayout()

        # Server Address
        conn_layout.addWidget(QLabel("Server Address:"), 0, 0)
        self.serv_addr_box = QLineEdit()
        self.serv_addr_box.setPlaceholderText("192.168.1.x")
        conn_layout.addWidget(self.serv_addr_box, 0, 1)

        # Ports (SSH, SFTP, HTTP, HTTPS)
        conn_layout.addWidget(QLabel("SSH Port:"), 1, 0)
        self.ssh_port_box = QLineEdit("22")
        conn_layout.addWidget(self.ssh_port_box, 1, 1)

        conn_layout.addWidget(QLabel("SFTP Port:"), 1, 2)
        self.sftp_port_box = QLineEdit("22")
        conn_layout.addWidget(self.sftp_port_box, 1, 3)

        conn_layout.addWidget(QLabel("HTTP Port:"), 2, 0)
        self.http_port_box = QLineEdit("80")
        conn_layout.addWidget(self.http_port_box, 2, 1)

        conn_layout.addWidget(QLabel("HTTPS Port:"), 2, 2)
        self.https_port_box = QLineEdit("443")
        conn_layout.addWidget(self.https_port_box, 2, 3)

        conn_group.setLayout(conn_layout)
        main_layout.addWidget(conn_group)

        # --- 2. Credentials Group ---
        cred_group = QGroupBox("User Credentials")
        cred_layout = QGridLayout()

        cred_layout.addWidget(QLabel("Username:"), 0, 0)
        self.username_box = QLineEdit()
        cred_layout.addWidget(self.username_box, 0, 1)

        cred_layout.addWidget(QLabel("Password:"), 1, 0)
        self.password_box = QLineEdit()
        self.password_box.setEchoMode(QLineEdit.EchoMode.Password)
        self.password_box.returnPressed.connect(self.connect_click) # Enter key triggers connect
        cred_layout.addWidget(self.password_box, 1, 1)

        cred_group.setLayout(cred_layout)
        main_layout.addWidget(cred_group)

        # --- 3. Action Buttons ---
        btn_layout = QHBoxLayout()

        self.save_btn = QPushButton("Save Config")
        self.save_btn.clicked.connect(self.save_server_configuration)
        btn_layout.addWidget(self.save_btn)

        self.load_btn = QPushButton("Load Config")
        self.load_btn.clicked.connect(self.load_server_configuration)
        btn_layout.addWidget(self.load_btn)

        self.connect_btn = QPushButton("Connect")
        self.connect_btn.clicked.connect(self.connect_click)
        btn_layout.addWidget(self.connect_btn)

        self.logoff_btn = QPushButton("Logoff")
        self.logoff_btn.setEnabled(False)
        self.logoff_btn.clicked.connect(self.logoff_click)
        btn_layout.addWidget(self.logoff_btn)

        self.close_btn = QPushButton("Close App")
        self.close_btn.setStyleSheet("background-color: #ffcccc;") # Light red
        self.close_btn.clicked.connect(self.close_app_click)
        btn_layout.addWidget(self.close_btn)

        main_layout.addLayout(btn_layout)
        
        # Spacer to push everything to top
        main_layout.addStretch()

    # --- Logic Methods ---

    def test_sftp_connection(self, host, user, password, port):
        """Equivalent to TestSftpConnection using Paramiko"""
        try:
            transport = paramiko.Transport((host, int(port)))
            transport.connect(username=user, password=password)
            transport.close()
            return True
        except Exception as e:
            print(f"SFTP Connection Failed: {e}")
            return False

    def connect_click(self):
        user = self.username_box.text().strip()
        pwd = self.password_box.text()
        server_path = self.serv_addr_box.text().strip()
        sftp_port = self.sftp_port_box.text().strip()

        if not user or not pwd:
            QMessageBox.warning(self, "Input Error", "Please supply user name and password.")
            return

        if not server_path:
            QMessageBox.warning(self, "Config Error", "Server path is not set.")
            return

        # Test Connection
        if self.test_sftp_connection(server_path, user, pwd, sftp_port):
            # Update Singleton
            creds = UserCredentials.get_instance()
            creds.set_credentials(
                server_path, user, pwd,
                self.ssh_port_box.text(),
                sftp_port,
                self.http_port_box.text(),
                self.https_port_box.text()
            )

            self.password_box.clear()
            self.logoff_btn.setEnabled(True)
            self.connect_btn.setEnabled(False)
            
            # Enable Tabs in Main Window
            # We need to import MainWindow locally to avoid circular imports
            from main_window import MainWindow
            if MainWindow.get_instance():
                MainWindow.get_instance().set_enabled_tabs(True)
            
            QMessageBox.information(self, "Connected", "Connection successful!")
        else:
            QMessageBox.critical(self, "Connection Refused", "Failed to connect to the server.")

    def logoff_click(self):
        try:
            self.logoff_btn.setEnabled(False)
            
            # Clear Credentials
            UserCredentials.get_instance().clear_credentials()

            # TODO: Close SFTP/Terminal connections here when we implement those classes
            # OMVFileExp.Instance.CloseSftpConnection()
            # UTerminal.Instance.Shutdown()

            # Disable Tabs
            from main_window import MainWindow
            if MainWindow.get_instance():
                MainWindow.get_instance().set_enabled_tabs(False)

            self.username_box.clear()
            self.serv_addr_box.clear()
            self.ssh_port_box.clear()
            self.sftp_port_box.clear()
            self.http_port_box.clear()
            self.https_port_box.clear()
            
            self.connect_btn.setEnabled(True)

        except Exception as ex:
            print(f"Logoff Error: {ex}")

    def save_server_configuration(self):
        # Validate Inputs
        if not all([self.serv_addr_box.text(), self.ssh_port_box.text(), 
                    self.sftp_port_box.text(), self.http_port_box.text(), 
                    self.https_port_box.text()]):
            QMessageBox.warning(self, "Error", "All fields must be completed.")
            return

        # Show Login Dialog to get Encryption Password
        login_dialog = NetConfigLogin.get_instance()
        if login_dialog.exec():
            enc_pass = login_dialog.entered_password
        else:
            QMessageBox.information(self, "Canceled", "Save canceled.")
            return

        # Prepare Data
        config = ServerConfigFile(
            server_path=self.serv_addr_box.text(),
            ssh_port=self.ssh_port_box.text(),
            sftp_port=self.sftp_port_box.text(),
            http_port=self.http_port_box.text(),
            https_port=self.https_port_box.text()
        )

        # Serialize & Encrypt
        try:
            # Convert dataclass to dict, then to JSON string
            json_str = json.dumps(config.__dict__)
            EncryptionHelper.encrypt_and_save(enc_pass, json_str, self.NET_CONFIG_FILENAME)
            
            # Visual Feedback (Green Flash)
            self.save_btn.setStyleSheet("background-color: lightgreen;")
            QTimer.singleShot(2000, lambda: self.save_btn.setStyleSheet(""))
            
        except Exception as ex:
            QMessageBox.critical(self, "Encryption Error", str(ex))

    def load_server_configuration(self):
        if not os.path.exists(self.NET_CONFIG_FILENAME):
            QMessageBox.warning(self, "Error", "Configuration file not found.")
            return

        login_dialog = NetConfigLogin.get_instance()
        if login_dialog.exec():
            dec_pass = login_dialog.entered_password
        else:
            return

        try:
            json_str = EncryptionHelper.decrypt_and_load(dec_pass, self.NET_CONFIG_FILENAME)
            if not json_str:
                raise Exception("Decryption failed or data is null.")

            data = json.loads(json_str)
            
            # Populate UI
            self.serv_addr_box.setText(data.get("server_path", ""))
            self.ssh_port_box.setText(data.get("ssh_port", "22"))
            self.sftp_port_box.setText(data.get("sftp_port", "22"))
            self.http_port_box.setText(data.get("http_port", "80"))
            self.https_port_box.setText(data.get("https_port", "443"))

            QMessageBox.information(self, "Success", "Configuration loaded successfully.")

        except Exception as ex:
            QMessageBox.critical(self, "Load Error", str(ex))

    def close_app_click(self):
        # Shutdown logic
        from PyQt6.QtWidgets import QApplication
        QApplication.quit()