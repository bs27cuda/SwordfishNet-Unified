using System.Windows.Controls;

namespace SwordfishNet_Unified
{
    public partial class OMVBrowser : Page
    {
        private static OMVBrowser _instance;
        public static OMVBrowser Instance => _instance ??= new OMVBrowser();
        public OMVBrowser()
        {
            InitializeComponent();
        }
    }
}
