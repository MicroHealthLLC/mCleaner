using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using mCleaner.Helpers.Controls;
using mCleaner.Logics;
using mCleaner.Logics.Clam;
using mCleaner.Model;
using mCleaner.Properties;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace mCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        TreeNode _SelectedNode;// = new TreeNode();
        

        public ViewModel_CleanerML CleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }

        public ViewModel_DuplicateChecker DupChecker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            CleanerML.TreeNodeSelected += CleanerML_TreeNodeSelected;
            tvCleaners.SelectedItemChanged += tvCleaners_SelectedItemChanged;
            tvCleaners.MouseDown += tvCleaners_MouseDown;

            //CleanerML.TreeNodeSelected

            this.Loaded += MainWindow_Loaded;
            
        }

        void tvCleaners_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        void tvCleaners_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeNode tn = (TreeNode)e.NewValue;

            if (tn != null)
            {
                tn.IsExpanded = !tn.IsExpanded;
            }
        }

        public static MainWindow I { get { return new MainWindow(); } }

        public void ClearClipboard()
        {
            Clipboard.Clear();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CleanerML.CommandCollapseAllClick();
            // check if an update is required after being prechecked at App.xaml.cs
            if (mCleaner.Logics.StaticResources.strShredLocation == string.Empty)
            {
                if (Settings.Default.ClamWin_Update)
                {
                    CommandLogic_Clam.I.LaunchUpdater();
                }
                else // none
                {   // then check if it requires an update at startup
                    if (Settings.Default.ClamWin_UpdateDBAtStartup)
                    {
                        CommandLogic_Clam.I.LaunchUpdater();
                    }
                }

            }
            else
            {
                ServiceLocator.Current.GetInstance<ViewModel.ViewModel_Shred>().Command_ShowWindow_Click();
                ServiceLocator.Current.GetInstance<ViewModel.ViewModel_Shred>().ShredFilesCollection.Add(new Model_Shred() { FilePath = mCleaner.Logics.StaticResources.strShredLocation });
                ServiceLocator.Current.GetInstance<ViewModel.ViewModel_Shred>().Command_ShredStart_Click();
                  FileAttributes attr = File.GetAttributes(mCleaner.Logics.StaticResources.strShredLocation);
                  if (attr.HasFlag(FileAttributes.Directory))
                  {
                      MessageBox.Show("Shredding is done successfully for the folder: " + mCleaner.Logics.StaticResources.strShredLocation,"mCleaner",MessageBoxButton.OK,MessageBoxImage.Information);
                  }
                  else
                      MessageBox.Show("Shredding is done successfully for file : " + mCleaner.Logics.StaticResources.strShredLocation, "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);
                      

                  Application.Current.Shutdown();
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
            //node.IsExpanded = !node.IsExpanded;

            CleanerML.Run = false;
            CleanerML.ShowFrontPage = false;
            CleanerML.ShowCleanerDescription = true;

            rtbCleanerDetails.Document = CleanerML.BuildCleanerDetails(
                node.Parent != null ? (cleaner)node.Parent.Tag : (cleaner)node.Tag
            );

            if (node.Key.Contains("duplicate_checker"))
            {
                if (DupChecker.DupplicateCollection.Count > 0)
                {
                    CleanerML.Run = true;
                }
            }
        }

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
           
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                TileSafeCleaning.Width = TileMordrateCleaning.Width = TileAggressiveCleaning.Width = TIleCustomSelection.Width = TileRegistrySelection.Width = TilePreview.Width = TileCleanNow.Width = TileClearSelection.Width = TileUninstall.Width = TileShredFileFolder.Width = TileScanMemory.Width =  197;
                TileCleanDuplicates.Width = TileScanPC.Width = 400;
                TileSafeCleaning.Height = TileMordrateCleaning.Height = TileAggressiveCleaning.Height = TileCleanDuplicates.Height = TIleCustomSelection.Height = TileRegistrySelection.Height = TilePreview.Height = TileCleanNow.Height = TileClearSelection.Height = TileUninstall.Height = TileShredFileFolder.Height = TileScanMemory.Height = TileScanPC.Height = 145;
               
            }
            else if (this.WindowState == System.Windows.WindowState.Normal)
            {
                TileSafeCleaning.Width = TileMordrateCleaning.Width = TileAggressiveCleaning.Width = TIleCustomSelection.Width = TileRegistrySelection.Width = TilePreview.Width = TileCleanNow.Width = TileClearSelection.Width = TileUninstall.Width = TileShredFileFolder.Width = TileScanMemory.Width =148;
                TileCleanDuplicates.Width = TileScanPC.Width = 300;
                TileSafeCleaning.Height = TileMordrateCleaning.Height = TileAggressiveCleaning.Height = TileCleanDuplicates.Height = TIleCustomSelection.Height = TileRegistrySelection.Height = TilePreview.Height = TileCleanNow.Height = TileClearSelection.Height = TileUninstall.Height = TileShredFileFolder.Height = TileScanMemory.Height = TileScanPC.Height = 125;
               
            }
        }
    }
}
