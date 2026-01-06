from PyQt6.QtWidgets import QWidget, QVBoxLayout, QLabel, QMessageBox
from PyQt6.QtCore import QUrl

# Try to import WebEngine, but provide a fallback if it's not installed
try:
    from PyQt6.QtWebEngineWidgets import QWebEngineView
    HAS_WEBENGINE = True
except ImportError:
    HAS_WEBENGINE = False

class OMVBrowser(QWidget):
    _instance = None

    def __init__(self):
        super().__init__()
        if OMVBrowser._instance is not None:
            raise Exception("Singleton cannot be instantiated more than once.")
        OMVBrowser._instance = self

        self.layout = QVBoxLayout()
        self.setLayout(self.layout)
        
        self.browser = None
        self.omv_dashboard_slab = None # Matches C# property name

        self.init_ui()

    @staticmethod
    def get_instance():
        if OMVBrowser._instance is None:
            OMVBrowser._instance = OMVBrowser()
        return OMVBrowser._instance

    def init_ui(self):
        if HAS_WEBENGINE:
            self.browser = QWebEngineView()
            self.omv_dashboard_slab = self.browser # Alias for C# compatibility
            
            # Default to a blank page or a "Not Connected" page
            self.browser.setHtml("<h1>Please connect in Server Config tab</h1>")
            self.layout.addWidget(self.browser)
        else:
            self.layout.addWidget(QLabel("Error: PyQt6-WebEngine is not installed."))
            self.layout.addWidget(QLabel("Run: pip install PyQt6-WebEngine"))

    def load_url(self, url_string):
        """Loads a URL if the browser is active."""
        if self.browser:
            self.browser.setUrl(QUrl(url_string))