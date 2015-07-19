using mCleaner.Helpers.Controls;
using mCleaner.Logics;
using mCleaner.Logics.Clam;
using mCleaner.Model;
using mCleaner.Properties;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace mCleaner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

            //System.Windows.Input.ApplicationCommands.Close

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
            //((TreeNode)e.NewValue).IsExpanded = !((TreeNode)e.NewValue).IsExpanded;
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
            logo.Visibility = System.Windows.Visibility.Collapsed;

            TreeNode node = sender as TreeNode;
            //node.IsExpanded = !node.IsExpanded;

            CleanerML.Run = false;

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
