/* 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Model;
using Microsoft.Practices.ServiceLocation;
using Octokit;
using Application = System.Windows.Application;

namespace mCleaner.ViewModel
{
    public class ViewModel_RestoreRegistryOptions : ViewModelBase
    {
        #region vars
        #endregion

        
        #region properties


        public ViewModel_CleanerML CleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }
        private bool _showWindow = false;
        public bool ShowWindow
        {
            get { return _showWindow; }
            set
            {
                if (_showWindow != value)
                {
                    _showWindow = value;
                    base.RaisePropertyChanged("ShowWindow");
                }
            }
        }


        private bool _okButtonEnabled = false;
        public bool OkButtonEnabled
        {
            get { return _okButtonEnabled; }
            set
            {
                if (_okButtonEnabled != value)
                {
                    _okButtonEnabled = value;
                    base.RaisePropertyChanged("OkButtonEnabled");
                }
            }
        }

        private ObservableCollection<Model_RestoreRegistry> _restoreRegistryCollection = new ObservableCollection<Model_RestoreRegistry>();

        public ObservableCollection<Model_RestoreRegistry> RestoreRegistryCollection
        {
            get { return _restoreRegistryCollection; }
            set
            {
                if (_restoreRegistryCollection != value)
                {
                    _restoreRegistryCollection = value;
                    base.RaisePropertyChanged("RestoreRegistryCollection");
                }
            }
        }


        private Model_RestoreRegistry _SelectedRestoreFile = null;

        public Model_RestoreRegistry SelectedRestoreFile
        {
            get { return _SelectedRestoreFile; }
            set
            {
                if (_SelectedRestoreFile != value)
                {
                    _SelectedRestoreFile = value;
                    base.RaisePropertyChanged("SelectedRestoreFile");
                }
                if (_SelectedRestoreFile != null)
                    OkButtonEnabled = true;
                else
                    OkButtonEnabled = false;
            }
        }

        #endregion

        #region commands
        public ICommand Command_OK { get; internal set; }
        public ICommand Command_ShowWindow { get; internal set; }
        public ICommand Command_Cancel { get; internal set; }
        #endregion

        #region ctor
        public ViewModel_RestoreRegistryOptions()
        {
            if (base.IsInDesignMode)
            {
                //ShowWindow = true;
            }
            else
            {
                Command_ShowWindow = new RelayCommand(Command_ShowWindow_Click);
                Command_OK = new RelayCommand(Command_OK_Click);
                Command_Cancel = new RelayCommand(Command_Menu_Cancel_Click);
            }
        }
        #endregion

        #region command methods
        void Command_OK_Click()
        {
            this.ShowWindow = false;
            Thread.Sleep(25);
            CleanerML.Command_RestoreRegistrySelection_Click(SelectedRestoreFile.FullFilePath);
        }

        void Command_ShowWindow_Click()
        {
            FillListViewWithOptions();
            this.ShowWindow = true;
        }

        void FillListViewWithOptions()
        {
            RestoreRegistryCollection.Clear();
            string strRegistryFolderPath = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "mCleaner\\RegistryBackups\\XMLInfo");
            if (Directory.Exists(strRegistryFolderPath))
            {
                DirectoryInfo di = new DirectoryInfo(strRegistryFolderPath);
                FileSystemInfo[] files = di.GetFileSystemInfos();
                var orderedFiles = files.OrderBy(f => f.Name);
                string formatString = "yyyyMMddHHmmss";

                foreach (var fileName in orderedFiles)
                {
                    RestoreRegistryCollection.Add(new Model_RestoreRegistry() { RegistryOption ="Backup Taken on "  + DateTime.ParseExact(Path.GetFileNameWithoutExtension(fileName.Name), formatString, null).ToString(), FullFilePath = fileName.FullName});
                }
            }
        }


        void Command_StartDownloading_Click()
        {
            this.ShowWindow = true;
            
        }

        void Command_Menu_Cancel_Click()
        {
            this.ShowWindow = false;
        }
#endregion

        #region methods

        #endregion
    }
}
