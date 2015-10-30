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

using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace mCleaner.ViewModel
{
    public class ViewModel_PrivacyPolicy : ViewModelBase
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

        private int _SelectedIndex = 0;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                _SelectedIndex = value;
                base.RaisePropertyChanged("SelectedIndex");
            }
        }
        #endregion

        #region commands
        public ICommand Command_OK { get; internal set; }
        public ICommand Command_Menu_PrivacyPolicy { get; internal set; }
        public ICommand Command_Menu_TermsOfUse { get; internal set; }
        public ICommand Command_Menu_TermsOfService { get; internal set; }
        #endregion

        #region ctor
        public ViewModel_PrivacyPolicy()
        {
            if (base.IsInDesignMode)
            {
                //ShowWindow = true;
            }
            else
            {
                Command_OK = new RelayCommand(Command_OK_Click);
                Command_Menu_PrivacyPolicy = new RelayCommand(Command_Menu_Privacy_Policy_Click);
                Command_Menu_TermsOfService = new RelayCommand(Command_Menu_TermsOfService_Click);
                Command_Menu_TermsOfUse = new RelayCommand(Command_Menu_TermsOfUse_Click);
            }
        }
        #endregion

        #region command methods
        void Command_OK_Click()
        {
            this.ShowWindow = false;
        }

        void Command_Menu_Privacy_Policy_Click()
        {
            this.SelectedIndex = 0;
            this.ShowWindow = true;
        }
        void Command_Menu_TermsOfService_Click()
        {
            this.SelectedIndex = 2;
            this.ShowWindow = true;
        }

        void Command_Menu_TermsOfUse_Click()
        {
            this.SelectedIndex = 1;
            this.ShowWindow = true;
        }
        #endregion

        #region methods

        #endregion
    }
}
