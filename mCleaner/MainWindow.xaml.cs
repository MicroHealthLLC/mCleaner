using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using mCleaner.Helpers.Controls;
using mCleaner.Logics;
using mCleaner.Logics.Clam;
using mCleaner.Model;
using mCleaner.Properties;
using mCleaner.ViewModel;
using MahApps.Metro.Controls;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

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

        public ViewModel_Shred Shred
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Shred>();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            CleanerML.TreeNodeSelected += CleanerML_TreeNodeSelected;
            tvCleaners.SelectedItemChanged += tvCleaners_SelectedItemChanged;
            this.Loaded += MainWindow_Loaded;
            
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
            Trace.WriteLine("Application Loaded");
            CleanerML.CommandCollapseAllClick();
            BackgroundWorker bgCheckforUpdatesWorker=new BackgroundWorker();
            bgCheckforUpdatesWorker.DoWork += new DoWorkEventHandler(bgCheckforUpdatesWorker_DoWork);
            bgCheckforUpdatesWorker.RunWorkerAsync();
            // check if an update is required after being prechecked at App.xaml.cs
            if (StaticResources.strShredLocation == string.Empty)
            {
                if (Settings.Default.ClamWin_Update)
                {
                    CommandLogic_Clam.I.LaunchUpdater();
                }
                else // none
                {
                    // then check if it requires an update at startup
                    if (Settings.Default.ClamWin_UpdateDBAtStartup)
                    {
                        CommandLogic_Clam.I.LaunchUpdater();
                    }
                }

            }
            else
            {
                Shred.Command_ShowWindow_Click();
                Shred.BlnIsApplicationNeedtoClose = true;
                Shred.ShredFilesCollection.Add(new Model_Shred() {FilePath = StaticResources.strShredLocation});
                Shred.Command_ShredStart_Click();
                FileAttributes attr = File.GetAttributes(StaticResources.strShredLocation);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    MessageBox.Show(
                        "Shredding is done successfully for the folder: " + StaticResources.strShredLocation, "mCleaner",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                    MessageBox.Show("Shredding is done successfully for file : " + StaticResources.strShredLocation,
                        "mCleaner", MessageBoxButton.OK, MessageBoxImage.Information);

                Application.Current.Shutdown();
            }

            ProgressWorker.I.Start();
        }



        protected void bgCheckforUpdatesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CleanerML.CheckForUpdates();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            ProgressWorker.I.Stop();
        }

        void CleanerML_TreeNodeSelected(object sender, EventArgs e)
        {
            TreeNode node = sender as TreeNode;
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

        public ViewModel_Uninstaller Uninstaller
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_Uninstaller>();
            }
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
           
            if (this.WindowState == WindowState.Maximized)
            {
                TileSafeCleaning.Width = TileMordrateCleaning.Width = TileAggressiveCleaning.Width = TIleCustomSelection.Width = TileRegistrySelection.Width = TilePreview.Width = TileCleanNow.Width = TileClearSelection.Width = TileUninstall.Width = TileShredFileFolder.Width = TileScanMemory.Width =  197;
                TileCleanDuplicates.Width = TileScanPC.Width = 400;
                TileSafeCleaning.Height = TileMordrateCleaning.Height = TileAggressiveCleaning.Height = TileCleanDuplicates.Height = TIleCustomSelection.Height = TileRegistrySelection.Height = TilePreview.Height = TileCleanNow.Height = TileClearSelection.Height = TileUninstall.Height = TileShredFileFolder.Height = TileScanMemory.Height = TileScanPC.Height = 145;
               
            }
            else if (this.WindowState == WindowState.Normal)
            {
                TileSafeCleaning.Width = TileMordrateCleaning.Width = TileAggressiveCleaning.Width = TIleCustomSelection.Width = TileRegistrySelection.Width = TilePreview.Width = TileCleanNow.Width = TileClearSelection.Width = TileUninstall.Width = TileShredFileFolder.Width = TileScanMemory.Width =148;
                TileCleanDuplicates.Width = TileScanPC.Width = 300;
                TileSafeCleaning.Height = TileMordrateCleaning.Height = TileAggressiveCleaning.Height = TileCleanDuplicates.Height = TIleCustomSelection.Height = TileRegistrySelection.Height = TilePreview.Height = TileCleanNow.Height = TileClearSelection.Height = TileUninstall.Height = TileShredFileFolder.Height = TileScanMemory.Height = TileScanPC.Height = 125;
               
            }
        }
        private void btnScanMemoryVirusMenu_Click(object sender, RoutedEventArgs e)
        {
            (sender as ToggleButton).ContextMenu.IsEnabled = true;
            (sender as ToggleButton).ContextMenu.PlacementTarget = (sender as ToggleButton);
            (sender as ToggleButton).ContextMenu.Placement = PlacementMode.Bottom;
            (sender as ToggleButton).ContextMenu.IsOpen = true;
            (sender as ToggleButton).ContextMenu.Closed += ContextMenu_Closed;
        }

        void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            btnScanMemoryVirusMenu.IsChecked = false;
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            Uninstaller.Command_UninstallProgram_Click();
        }

        private void AzureDataGrid_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (btnUninstall != null)
                btnUninstall.IsEnabled = true;
        }

        private void BtnRemoveShred_OnClick(object sender, RoutedEventArgs e)
        {
            List<Model_Shred> lstSelectedItems = new List<Model_Shred>();

            foreach (Model_Shred item in lvShredFolderPath.SelectedItems)
            {
                lstSelectedItems.Add(item);
            }

            Shred.Command_RemoveFolder_Click(lstSelectedItems);
        }
    }
}
