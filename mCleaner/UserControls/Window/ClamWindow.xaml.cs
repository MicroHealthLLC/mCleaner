using System.Windows.Controls;

namespace mCleaner.UserControls.Window
{
    /// <summary>
    /// Interaction logic for ClamWindow.xaml
    /// </summary>
    public partial class ClamWindow : UserControl
    {
        public ClamWindow()
        {
            InitializeComponent();
            scrollviewer.ScrollChanged += scrollviewer_ScrollChanged;
        }

        void scrollviewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            scrollviewer.ScrollToBottom();
        }
    }
}
