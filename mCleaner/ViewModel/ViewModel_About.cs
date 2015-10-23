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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace mCleaner.ViewModel
{
    public class ViewModel_About : ViewModelBase
    {
        #region vars
        #endregion

        #region properties
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

        #endregion

        #region commands
        public ICommand Command_OK { get; internal set; }
        public ICommand Command_ShowWindow { get; internal set; }
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
                Command_OK = new RelayCommand(Command_OK_Click);
                Command_CheckForUpdates = new RelayCommand(Command_Menu_CheckForUpdates_Click);
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

        void Command_Menu_CheckForUpdates_Click()
        {
            
        }
        #endregion

        #region methods

        #endregion
    }
}
