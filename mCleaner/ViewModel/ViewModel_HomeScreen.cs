using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using CodeBureau;
using GalaSoft.MvvmLight;
using mCleaner.Helpers.Controls;
using mCleaner.Logics.Commands;
using mCleaner.Logics.Enumerations;
using mCleaner.Model;
using MahApps.Metro.Controls;
using Microsoft.Practices.ServiceLocation;
using PieControls;

namespace mCleaner.ViewModel
{
    public class ViewModel_HomeScreen : ViewModelBase
    {
        #region properties
        
        ObservableCollection<PieSegment> _safeCleaningData =new ObservableCollection<PieSegment>();

        public ObservableCollection<PieSegment> SafeCleaningData
        {
            get { return _safeCleaningData; }
            set
            {
                if (_safeCleaningData != value)
                {
                    _safeCleaningData = value;
                    base.RaisePropertyChanged("SafeCleaningData");
                }
            }
        }

        private string _totalSpaceCanBeRecoveredAggressive = "..";

        public String TotalSpaceCanBeRecoveredAggressive
        {
            get { return _totalSpaceCanBeRecoveredAggressive; }
            set
            {
                if (_totalSpaceCanBeRecoveredAggressive != value)
                {
                    _totalSpaceCanBeRecoveredAggressive = value;
                    base.RaisePropertyChanged("TotalSpaceCanBeRecoveredAggressive");
                }
            }
        }
        private string _totalSpaceCanBeRecoveredMorderate = "..";

        public String TotalSpaceCanBeRecoveredMorderate
        {
            get { return _totalSpaceCanBeRecoveredMorderate; }
            set
            {
                if (_totalSpaceCanBeRecoveredMorderate != value)
                {
                    _totalSpaceCanBeRecoveredMorderate = value;
                    base.RaisePropertyChanged("TotalSpaceCanBeRecoveredMorderate");
                }
            }
        }
        private string _totalSpaceCanBeRecoveredSafe = "..";

        public String TotalSpaceCanBeRecoveredSafe
        {
            get { return _totalSpaceCanBeRecoveredSafe; }
            set
            {
                if (_totalSpaceCanBeRecoveredSafe != value)
                {
                    _totalSpaceCanBeRecoveredSafe = value;
                    base.RaisePropertyChanged("TotalSpaceCanBeRecoveredSafe");
                }
            }
        }

        private bool _isScanningCompleted = false;

        public bool IsScanningCompleted
        {
            get { return _isScanningCompleted; }
            set
            {
                if (_isScanningCompleted != value)
                {
                    _isScanningCompleted = value;
                    base.RaisePropertyChanged("IsScanningCompleted");
                }
            }
        }

        private ObservableCollection<Model_HomeScreenGrid> _aggressiveCleaningCollection = new ObservableCollection<Model_HomeScreenGrid>();
        public ObservableCollection<Model_HomeScreenGrid> AggressiveCleaningCollection
        {
            get { return _aggressiveCleaningCollection; }
            set
            {
                if (_aggressiveCleaningCollection != value)
                {
                    _aggressiveCleaningCollection = value;
                    base.RaisePropertyChanged("AggressiveCleaningCollection");
                }
            }
        }

        public ViewModel_CleanerML CleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }

        #endregion



        #region ctor

        public ViewModel_HomeScreen()
        {
            AggressiveCleaningCollection.Add(new Model_HomeScreenGrid()
            {
                Details =
                    new Model_HomeScreenGrid_FileDetails()
                    {
                        FileCount = "Calculating..",
                        Name = "Browser Cache",
                        Size = "Calculating..",
                        ImagePath = "Assets/Browser.png"
                    },
            });
            AggressiveCleaningCollection.Add(new Model_HomeScreenGrid()
            {
                Details =
                    new Model_HomeScreenGrid_FileDetails()
                    {
                        FileCount = "Calculating..",
                        Name = "Applicaiton Cache",
                        Size = "Calculating..",
                        ImagePath = "Assets/ApplicationCache.png"
                    },
            });
            AggressiveCleaningCollection.Add(new Model_HomeScreenGrid()
            {
                Details =
                    new Model_HomeScreenGrid_FileDetails()
                    {
                        FileCount = "Calculating..",
                        Name = "Windows Temp and  Cache",
                        Size = "Calculating..",
                        ImagePath = "Assets/WindowsCache.png"
                    },
            });
            AggressiveCleaningCollection.Add(new Model_HomeScreenGrid()
            {
                Details =
                    new Model_HomeScreenGrid_FileDetails()
                    {
                        FileCount = "Calculating..",
                        Name = "RecycleBin",
                        Size = "Calculating..",
                        ImagePath = "Assets/SelectRecycleBin.png"
                    },
            });


            SafeCleaningData.Add(new PieSegment {Color = Color.FromRgb(255,152,0) , Value = 5, Name = "Calculating"});
            SafeCleaningData.Add(new PieSegment { Color = Color.FromRgb(75, 91, 136), Value = 12, Name = "Calculating" });
            SafeCleaningData.Add(new PieSegment { Color = Color.FromRgb(76, 175, 80), Value = 22, Name = "Calculating" });
            SafeCleaningData.Add(new PieSegment {Color = Color.FromRgb(143, 113, 165),Value = 20, Name = "Calculating"});
   
        }

        #endregion

        #region command methods
        public BackgroundWorker bgWorker = null;
        public async Task<bool> Start()
        {

            CommandLogic_Delete_CalculateSpace.ApplicationCache_SafeCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.ApplicationCache_SafeCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.WindowsTemp_SafeCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.WindowsTemp_SafeCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.BrowserCache_SafeCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.BrowserCache_SafeCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.ApplicationCache_MordrateCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.ApplicationCache_MordrateCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.WindowsTemp_MordrateCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.WindowsTemp_MordrateCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.BrowserCache_MordrateCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.BrowserCache_MordrateCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.ApplicationCache_AggressiveCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.ApplicationCache_AggressiveCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.WindowsTemp_AggressiveCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.WindowsTemp_AggressiveCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.BrowserCache_AggressiveCleaning_Space = 0;
            CommandLogic_Delete_CalculateSpace.BrowserCache_AggressiveCleaning_FilesCount = 0;
            CommandLogic_Delete_CalculateSpace.RecycleBin_FileCount = 0;
            CommandLogic_Delete_CalculateSpace.RecycleBin_Space = 0;

            foreach (string file in Directory.EnumerateFiles("C:\\$RECYCLE.BIN", "*.*", SearchOption.AllDirectories))
            {
                FileInfo fi =new FileInfo(file);
                CommandLogic_Delete_CalculateSpace.RecycleBin_Space += fi.Length;
                CommandLogic_Delete_CalculateSpace.RecycleBin_FileCount++;
                bgWorker.ReportProgress(1);
            }

            foreach (TreeNode node in CleanerML.CleanersCollection)
            { 
              if(node.Name=="Deep Scan")
                  continue;

                foreach (TreeNode child in node.Children)
                {
                    option o = (option)child.Tag;

                    string last_Log = string.Empty;

                    foreach (action _a in o.action)
                    {
                       
                        COMMANDS cmd = (COMMANDS)StringEnum.Parse(typeof(COMMANDS), _a.command);

                        iActions axn = null;

                        switch (cmd)
                        {
                            case COMMANDS.delete:
                                axn = new CommandLogic_Delete_CalculateSpace();
                                break;
                        }

                        if (axn != null)
                        {
                            axn.Action = _a;
                            axn.Enqueue(); // execute for queueing 
                        }
                        bgWorker.ReportProgress(1);
                    }

                }
            }

            return true;

        }

        #endregion
    }
}
