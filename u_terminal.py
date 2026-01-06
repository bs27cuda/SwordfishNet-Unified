import sys
import re
import threading
import time
import paramiko
from PyQt6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QTextEdit, 
                             QLineEdit, QLabel, QMessageBox)
from PyQt6.QtCore import Qt, pyqtSignal, QThread, QObject

# Import your custom data class
from user_credentials import UserCredentials

# --- Background Worker for Reading SSH Output ---
class SSHReader(QThread):
    data_received = pyqtSignal(str) # Signal to send text back to Main Thread

    def __init__(self, channel):
        super().__init__()
        self.channel = channel
        self.running = True
        
        # Regex to strip ANSI color codes (Matches your C# Regex)
        self.ansi_escape = re.compile(r'\x1B\[[0-?]*[ -/]*[@-~]')

    def run(self):
        while self.running:
            try:
                if self.channel.recv_ready():
                    # Read available data (up to 4096 bytes at a time)
                    data = self.channel.recv(4096)
                    if not data:
                        break
                    
                    # Decode and Clean
                    text = data.decode('utf-8', errors='ignore')
                    clean_text = self.ansi_escape.sub('', text)
                    
                    # Send to UI
                    self.data_received.emit(clean_text)
                else:
                    # Prevent CPU spiking by sleeping briefly if no data
                    time.sleep(0.1)
            except Exception:
                break

    def stop(self):
        self.running = False

# --- Main Terminal Widget ---
class UTerminal(QWidget):
    _instance = None
    
    def __init__(self):
        super().__init__()
        UTerminal._instance = self
        
        self.ssh_client = None
        self.channel = None
        self.reader_thread = None
        
        # Command History
        self.history = []
        self.history_index = -1
        
        self.init_ui()

    @staticmethod
    def get_instance():
        if UTerminal._instance is None:
            UTerminal._instance = UTerminal()
        return UTerminal._instance

    def init_ui(self):
        layout = QVBoxLayout()
        self.setLayout(layout)

        # 1. Output Area (Read Only)
        self.output_area = QTextEdit()
        self.output_area.setReadOnly(True)
        self.output_area.setStyleSheet("background-color: black; color: white; font-family: Consolas, Monospace;")
        layout.addWidget(self.output_area)

        # 2. Input Area
        # Note: Instead of fighting the cursor like in C# (locking the prompt),
        # we just put a Label "$ " next to the Input box. It's much cleaner.
        input_layout = QHBoxLayout()
        
        self.prompt_label = QLabel("$")
        self.prompt_label.setStyleSheet("font-weight: bold;")
        input_layout.addWidget(self.prompt_label)
        
        self.input_line = QLineEdit()
        self.input_line.setStyleSheet("font-family: Consolas, Monospace;")
        self.input_line.returnPressed.connect(self.execute_command)
        self.input_line.installEventFilter(self) # To catch Up/Down arrows
        self.input_line.setEnabled(False) # Disabled until connected
        
        input_layout.addWidget(self.input_line)
        layout.addLayout(input_layout)

    def showEvent(self, event):
        super().showEvent(event)
        # Auto-connect if credentials exist and we aren't connected
        if not self.ssh_client and UserCredentials.get_instance().are_credentials_set_ssh():
            # We run connection in a slight delay or thread to avoid UI hang on tab switch
            # For simplicity here, we call it directly, but typically you'd thread this too.
            self.connect_ssh()

    def connect_ssh(self):
        creds = UserCredentials.get_instance()
        self.output_area.append(f"Attempting to connect to {creds.server_path}...")
        
        try:
            port = int(creds.ssh_port) if creds.ssh_port.isdigit() else 22
            
            self.ssh_client = paramiko.SSHClient()
            self.ssh_client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
            
            self.ssh_client.connect(
                hostname=creds.server_path,
                port=port,
                username=creds.username,
                password=creds.password,
                timeout=10
            )

            # Invoke an interactive shell
            self.channel = self.ssh_client.invoke_shell()
            
            # Start Background Reader
            self.reader_thread = SSHReader(self.channel)
            self.reader_thread.data_received.connect(self.append_text)
            self.reader_thread.start()

            self.output_area.append("Connected successfully.\n")
            self.input_line.setEnabled(True)
            self.input_line.setFocus()

        except Exception as ex:
            self.output_area.append(f"Connection Failed: {ex}")

    def append_text(self, text):
        self.output_area.moveCursor(self.output_area.textCursor().MoveOperation.End)
        self.output_area.insertPlainText(text)
        self.output_area.ensureCursorVisible()

    def execute_command(self):
        if not self.channel: return

        cmd = self.input_line.text().strip()
        
        # History Logic
        if cmd and (not self.history or self.history[-1] != cmd):
            self.history.append(cmd)
        self.history_index = len(self.history)

        # Send to SSH Channel
        # Note: We add \n because enter was pressed
        try:
            self.channel.send(cmd + "\n")
        except:
            self.append_text("\n[Error: Disconnected]\n")
        
        self.input_line.clear()

    # --- Event Filter for Up/Down Arrows (History) ---
    def eventFilter(self, obj, event):
        if obj == self.input_line and event.type() == event.Type.KeyPress:
            key = event.key()
            if key == Qt.Key.Key_Up:
                if self.history and self.history_index > 0:
                    self.history_index -= 1
                    self.input_line.setText(self.history[self.history_index])
                return True
            elif key == Qt.Key.Key_Down:
                if self.history and self.history_index < len(self.history) - 1:
                    self.history_index += 1
                    self.input_line.setText(self.history[self.history_index])
                elif self.history_index == len(self.history) - 1:
                     self.history_index += 1
                     self.input_line.clear()
                return True
        return super().eventFilter(obj, event)

    def shutdown(self):
        """Clean cleanup method"""
        if self.reader_thread:
            self.reader_thread.stop()
            self.reader_thread.wait() # Wait for thread to finish
        
        if self.ssh_client:
            self.ssh_client.close()
            self.ssh_client = None
            
        self.output_area.append("\n--- Disconnected ---\n")
        self.input_line.setEnabled(False)