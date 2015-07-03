using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace mCleaner.ViewModel
{
    public class ViewModel_Shred : ViewModelBase
    {
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
            if (base.IsInDesignMode)
            {
                this.WindowTitle = "Shred File\\Folder";
                this.Log = "Starting to shred\r\nC:\\Windows\\win.com";
            }
            else
            {
                Command_ShredFile = new RelayCommand(Command_ShredFile_Click);
                Command_ShredFolder = new RelayCommand(Command_ShredFolder_Click);
            }
        }

        public void Command_ShredFile_Click()
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

                }
            };
        }

        public void Command_ShredFolder_Click()
        {

        }

        void StartShredding()
        {

        }
    }
}
