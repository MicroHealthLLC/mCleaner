using mCleaner.Helpers.Controls;
using mCleaner.Logics;
using mCleaner.Logics.Clam;
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
            //CleanerML.TreeNodeSelected
             
            this.Loaded += MainWindow_Loaded;
            
        }
        public static MainWindow I { get { return new MainWindow(); } }

        public void ClearClipboard()
        {
            Clipboard.Clear();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ClamWin_UpdateDBAtStartup)
            {
                CommandLogic_Clam.I.LaunchUpdater();
            }

            ProgressWorker.I.Start();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            ProgressWorker.I.Stop();
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

                //foreach (option o in c.option)
                //{

                //}
            }
        }
    }
}
