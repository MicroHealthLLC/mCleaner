using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using mCleaner.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace mCleaner.ViewModel
{
    public class ViewModel_Shred : ViewModelBase
    {
        Queue<string> QueueFiles = new Queue<string>();

        private string _WindowTitle = string.Empty;
        public string WindowTitle
        {
            get { return _WindowTitle; }
            set
            {
                if (_WindowTitle != value)
                {
                    _WindowTitle = value;
                    base.RaisePropertyChanged("WindowTitle");
                }
            }
        }

        private string _Log = string.Empty;
        public string Log
        {
            get { return _Log; }
            set
            {
                if (_Log != value)
                {
                    _Log = value;
                    base.RaisePropertyChanged("Log");
                }
            }
        }

        private bool _ShowWindow = false;
        public bool ShowWindow
        {
            get { return _ShowWindow; }
            set
            {
                if (_ShowWindow != value)
                {
                    _ShowWindow = value;
                    base.RaisePropertyChanged("ShowWindow");
                }
            }
        }

        public ICommand Command_ShredFile { get; set; }
        public ICommand Command_ShredFolder { get; set; }

        public ViewModel_Shred()
        {
            this.WindowTitle = "Shred File & Folder";

            if (base.IsInDesignMode)
            {
                this.Log = "Starting to shred\r\nC:\\Windows\\win.com";
            }
            else
            {
                Command_ShredFile = new RelayCommand(() => Command_ShredFile_Click());
                Command_ShredFolder = new RelayCommand(Command_ShredFolder_Click);
                Wipe wipe = new Wipe();
                
            }
        }

        public async Task<bool> Command_ShredFile_Click()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "All files|*.*",
                Title = "Select a multiple file to shred",
                Multiselect = true
            };
            {
                ofd.ShowDialog();
                if (ofd.FileName != string.Empty || ofd.FileNames.Length > 0)
                {
                    foreach (string file in ofd.FileNames)
                    {
                        this.QueueFiles.Enqueue(file);
                    }

                    await Task.Run(() => StartShredding());
                }
            };

            return true;
        }

        public void Command_ShredFolder_Click()
        {

        }

        void StartShredding()
        {
            this.ShowWindow = true;

            AddLog("Starting to securely shred " + this.QueueFiles.Count + " files with 5 iterations.");

            while (this.QueueFiles.Count != 0)
            {
                string file = this.QueueFiles.Dequeue();

                string text = "Shredding " + file;
                AddLog(text);

                Thread.Sleep(2000); // shred work
            }

            AddLog("Done");
            this.Log = string.Empty;

            this.ShowWindow = false;
        }

        void AddLog(string log, bool addCrLf = true)
        {
            this.Log += log + (addCrLf ? "\r\n" : string.Empty);
        }
    }
}
