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
            // check if an update is required after being prechecked at App.xaml.cs
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
    }
}
