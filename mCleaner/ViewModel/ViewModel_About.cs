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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octokit;
using Application = System.Windows.Application;

namespace mCleaner.ViewModel
{
    public class ViewModel_About : ViewModelBase
    {
        #region vars
        #endregion

        
        #region properties
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


        private bool _updateButtonEnable = true;
        public bool UpdateButtonEnable
        {
            get { return _updateButtonEnable; }
            set
            {
                if (_updateButtonEnable != value)
                {
                    _updateButtonEnable = value;
                    base.RaisePropertyChanged("UpdateButtonEnable");
                }
            }
        }
        private string _updateAvailableText = string.Empty;
        public string UpdateAvailableText
        {
            get { return _updateAvailableText; }
            set
            {
                if (_updateAvailableText != value)
                {
                    _updateAvailableText = value;
                    base.RaisePropertyChanged("UpdateAvailableText");
                }
            }
        }

        private string _checkForUpdatesButtonText = "Check For Updates";
        public string CheckForUpdatesButtonText
        {
            get { return _checkForUpdatesButtonText; }
            set
            {
                if (_checkForUpdatesButtonText != value)
                {
                    _checkForUpdatesButtonText = value;
                    base.RaisePropertyChanged("CheckForUpdatesButtonText");
                }
            }
        }

        private int _commandParameter = 1;
        public int CommandParameter
        {
            get { return _commandParameter; }
            set
            {
                if (_commandParameter != value)
                {
                    _commandParameter = value;
                    base.RaisePropertyChanged("CommandParameter");
                }
            }
        }

        private bool _isDownloadingRunning = false;
        public bool IsDownloadingRunning
        {
            get { return _isDownloadingRunning; }
            set
            {
                if (_isDownloadingRunning != value)
                {
                    _isDownloadingRunning = value;
                    base.RaisePropertyChanged("IsDownloadingRunning");
                }
            }
        }


        public string mCleanerTitle
        {
            get
            {
                return "mCleaner " + Assembly.GetExecutingAssembly().GetName().Version.Major + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor + "." + Assembly.GetExecutingAssembly().GetName().Version.Build;
            }
        }

        #endregion

        #region commands
        public ICommand Command_OK { get; internal set; }
        public ICommand Command_ShowWindow { get; internal set; }
        public ICommand Command_StartDownloading { get; internal set; }
        public ICommand Command_CheckForUpdates { get; internal set; }
        #endregion

        #region ctor
        public ViewModel_About()
        {
            if (base.IsInDesignMode)
            {
                //ShowWindow = true;
            }
            else
            {
                Command_ShowWindow = new RelayCommand(Command_ShowWindow_Click);
                Command_StartDownloading = new RelayCommand(Command_StartDownloading_Click);
                Command_OK = new RelayCommand(Command_OK_Click);
                Command_CheckForUpdates = new RelayCommand<int>(Command_Menu_CheckForUpdates_Click);
            }
        }
        #endregion

        #region command methods
        void Command_OK_Click()
        {
            this.ShowWindow = false;
        }

        void Command_ShowWindow_Click()
        {
            this.ShowWindow = true;
        }

        void Command_StartDownloading_Click()
        {
            this.ShowWindow = true;
            CommandParameter = 2;
            if (!IsDownloadingRunning)
            {
                DownloadUpdate();
            }

        }

        void Command_Menu_CheckForUpdates_Click(int i)
        {
            if(i==1)
        #pragma warning disable 4014
                CheckForUpdates();
        #pragma warning restore 4014
            else if (i==2)
        #pragma warning disable 4014
                DownloadUpdate();
        #pragma warning restore 4014
                
        }

        async Task CheckForUpdates()
        {
            try
            {
                UpdateAvailableText = "Checking for updates...";
                var client = new GitHubClient(new ProductHeaderValue("mCleaner"));
                var releasess = await client.Release.GetAll("MicroHealthLLC", "mCleaner");
                var latest = releasess[0];

                if (new Version(latest.TagName.Replace("v", string.Empty)) > Assembly.GetExecutingAssembly().GetName().Version)
                {
                    UpdateAvailableText = "mCleaner update available version : " + latest.TagName.Replace("v", string.Empty);
                    CheckForUpdatesButtonText = "Download Update";
                    CommandParameter = 2;
                }
                else
                {
                    UpdateAvailableText = "mCleaner is up to date";
                }
            }
            catch (Exception ex)
            {
                UpdateAvailableText = "An error occurred. Please check internet connection";
            }
        }

        #endregion

        async Task DownloadUpdate()
        {
            try
            {
                IsDownloadingRunning = true;
                UpdateAvailableText = "Started Downloading...";
                CheckForUpdatesButtonText = "Downloading..";
                UpdateButtonEnable = false;
                var client = new GitHubClient(new ProductHeaderValue("mCleaner"));
                var releasess = await client.Release.GetAll("MicroHealthLLC", "mCleaner");
                var latest = releasess[0];
                var assets = await client.Release.GetAllAssets("MicroHealthLLC", "mCleaner", latest.Id);
                var myAsset = assets[0];
                var strZipDownloadingPath = Path.Combine(Path.GetTempPath(), "mCleaner.zip");
                var strmCleanerExePath = Path.Combine(Path.GetTempPath(), "mCleaner.exe");
                if (File.Exists(strZipDownloadingPath))
                {
                    File.Delete(strZipDownloadingPath);
                }
                if (File.Exists(strmCleanerExePath))
                {
                    File.Delete(strmCleanerExePath);
                }
                    WebClient webclient = new WebClient();
                    webclient.DownloadProgressChanged +=
                        new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    webclient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    webclient.DownloadFileAsync(new Uri(myAsset.BrowserDownloadUrl),
                        Path.Combine(Path.GetTempPath(), "mCleaner.zip"));
            }
            catch (Exception ex)
            {
                UpdateAvailableText = "An error occured while downloading update. Please check your internet connection";
                UpdateButtonEnable = true;
            }
        }

        public void ExtractZipAndUpdate(string zipFilePath)
        {
            string strTempPath = Path.GetTempPath();
            ZipFile.ExtractToDirectory(zipFilePath, strTempPath);
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process {StartInfo = startInfo};

            process.Start();
            process.StandardInput.WriteLine(Path.Combine(strTempPath, "mCleaner.exe"));
            process.StandardInput.WriteLine("exit");

            process.WaitForExit();
            Application.Current.Shutdown();

        }


        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            UpdateAvailableText = "Downloading Update " + String.Format("{0:0.00}", percentage) +"% Complete";
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                UpdateAvailableText = "Downloading Complete.";
                UpdateButtonEnable = true;
                var strZipDownloadingPath = Path.Combine(Path.GetTempPath(), "mCleaner.zip");
                ExtractZipAndUpdate(strZipDownloadingPath);
            }
            else
            {
                UpdateAvailableText = "An error occured while downloading. Please try again.";
                UpdateButtonEnable = true;
            }
            IsDownloadingRunning = false;
        }

        #region methods

        #endregion
    }
}
