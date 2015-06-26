using mCleaner.Helpers.Controls;
using mCleaner.Model;
using mCleaner.Properties;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System.Windows;

namespace mCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModel_CleanerML CleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            //System.Windows.Input.ApplicationCommands.Close

            CleanerML.TreeNodeSelected += CleanerML_TreeNodeSelected;
            this.Loaded += MainWindow_Loaded;
            
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ClamWin_UpdateAtStartup)
            {
                //CommandLogic_Clam.I.LaunchUpdater();
            }
        }

        void CleanerML_TreeNodeSelected(object sender, System.EventArgs e)
        {
            TreeNode node = sender as TreeNode;
            //if (node.Parent != null)
            {
                CleanerML.Run = false;

                rtbCleanerDetails.Document = CleanerML.BuildCleanerDetails(
                    node.Parent != null ? (cleaner)node.Parent.Tag : (cleaner)node.Tag
                );
            }
        }
    }
}
