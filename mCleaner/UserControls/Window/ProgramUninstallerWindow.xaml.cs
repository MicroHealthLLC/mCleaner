using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mCleaner.UserControls.Window
{
    /// <summary>
    /// Interaction logic for DuplicateScannerWindow.xaml
    /// </summary>
    public partial class ProgramUninstallerWindow : UserControl
    {
        public ProgramUninstallerWindow()
        {
            InitializeComponent();
        }

        private void AzureDataGrid_Selected(object sender, RoutedEventArgs e)
        {
            btnUninstall.IsEnabled = true;
        }
    }
}
