using MahApps.Metro.Controls;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;
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

namespace mCleaner
{
    /// <summary>
    /// Interaction logic for DuplicateScannerWindow.xaml
    /// </summary>
    public partial class ProgramUninstallerWindow : MetroWindow
    {
        public ProgramUninstallerWindow()
        {
            InitializeComponent();
        }

        public ViewModel_Uninstaller Uninstaller
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Uninstaller>();
            }
        }


        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            Uninstaller.Command_UninstallProgram_Click();
        }

        private void AzureDataGrid_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (btnUninstall!=null)
                btnUninstall.IsEnabled = true;
        }
    }
}
