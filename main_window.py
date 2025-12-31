import sys
from PyQt6.QtWidgets import (QApplication, QMainWindow, QTabWidget, 
                             QWidget, QVBoxLayout, QLabel, QTextEdit)

# --- REAL MODULES (The ones we have converted) ---
from server_config_page import ServerConfigPage
from omv_browser import OMVBrowser

# --- PLACEHOLDERS (We still need to convert these C# files) ---
class OMVFileExp(QWidget):
    _instance = None
    def __init__(self):
        super().__init__()
        OMVFileExp._instance = self
        layout = QVBoxLayout()
        layout.addWidget(QLabel("Placeholder: File Explorer (OMVFileExp.cs needed)"))
        self.setLayout(layout)
    
    @staticmethod
    def get_instance():
        if OMVFileExp._instance is None:
            OMVFileExp._instance = OMVFileExp()
        return OMVFileExp._instance

    def CloseSftpConnection(self):
        print("Stub: SFTP Connection Closed")

class UTerminal(QWidget):
    _instance = None

    def __init__(self, parent=None):
        super().__init__(parent)
        # Ensure the Singleton reference is set
        if UTerminal._instance is not None:
            raise Exception("UTerminal is a singleton! Use UTerminal.get_instance()")
        UTerminal._instance = self

        # Layout setup
        layout = QVBoxLayout()
        self.setLayout(layout)

        # Replaced QLabel with QTextEdit for scrolling logs
        self.text_output = QTextEdit()
        self.text_output.setReadOnly(True)  # Make it read-only so users can't delete logs
        self.text_output.setStyleSheet("background-color: black; color: lime; font-family: Consolas;") 
        layout.addWidget(self.text_output)

    @staticmethod
    def get_instance():
        """Static access method."""
        if UTerminal._instance is None:
            UTerminal() # This calls __init__, which sets _instance
        return UTerminal._instance

    def log(self, message: str):
        """Appends a new line to the terminal."""
        if self.text_output:
            self.text_output.append(message)
            # Auto-scroll to bottom
            self.text_output.verticalScrollBar().setValue(
                self.text_output.verticalScrollBar().maximum()
            )

    def Shutdown(self):
        print("Stub: Terminal Shutdown")
        self.log("System: Terminal Shutdown initiated...")

# --------------------------------------------------------------------------

class MainWindow(QMainWindow):
    _instance = None

    def __init__(self):
        super().__init__()
        MainWindow._instance = self
        
        self.setWindowTitle("SwordfishNet Unified")
        self.resize(1000, 700) # Made it slightly larger for the browser

        # Main Central Widget
        self.central_widget = QTabWidget()
        self.setCentralWidget(self.central_widget)

        # --- Instantiate Pages ---
        # Note: We use get_instance() to mimic your C# Singletons
        self.server_config_page = ServerConfigPage.get_instance()
        self.omv_browser = OMVBrowser.get_instance()
        self.omv_file_exp = OMVFileExp.get_instance()
        self.u_terminal = UTerminal.get_instance()

        # --- Add Tabs ---
        self.central_widget.addTab(self.server_config_page, "Server Config") # Index 0
        self.central_widget.addTab(self.omv_browser, "NAS Portal")           # Index 1
        self.central_widget.addTab(self.omv_file_exp, "File Explorer")       # Index 2
        self.central_widget.addTab(self.u_terminal, "Terminal")              # Index 3

        # Store index references
        self.tab_indices = {
            "ServTab": 0,
            "NASPortalTab": 1,
            "FileExpTab": 2,
            "TermTab": 3
        }

        # --- Initial State ---
        self.set_enabled_tabs(False)
        self.central_widget.setCurrentIndex(self.tab_indices["ServTab"])

    @staticmethod
    def get_instance():
        return MainWindow._instance

    def set_enabled_tabs(self, is_enabled: bool):
        """
        Enables or disables tabs based on login state.
        Matches C# SetEnabledTabs(bool)
        """
        self.central_widget.setTabEnabled(self.tab_indices["NASPortalTab"], is_enabled)
        self.central_widget.setTabEnabled(self.tab_indices["FileExpTab"], is_enabled)
        self.central_widget.setTabEnabled(self.tab_indices["TermTab"], is_enabled)

        if not is_enabled:
            self.central_widget.setCurrentIndex(self.tab_indices["ServTab"])

# --- App Entry Point ---
if __name__ == "__main__":
    app = QApplication(sys.argv)
    
    # Create and show the main window
    window = MainWindow()
    window.show()
    
    sys.exit(app.exec())