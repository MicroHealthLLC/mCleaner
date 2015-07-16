using GalaSoft.MvvmLight;
using mCleaner.Model;
using System.Collections.ObjectModel;

namespace mCleaner.ViewModel
{
    public class ViewModel_DuplicateChecker : ViewModelBase
    {
        #region properties
        private ObservableCollection<Model_DuplicateChecker> _DupplicationCollection = new ObservableCollection<Model_DuplicateChecker>();
        public ObservableCollection<Model_DuplicateChecker> DupplicationCollection
        {
            get { return _DupplicationCollection; }
            set
            {
                if (_DupplicationCollection != value)
                {
                    _DupplicationCollection = value;
                    base.RaisePropertyChanged("DupplicationCollection");
                }
            }
        }
        #endregion

        #region commands

        #endregion

        #region ctor
        public ViewModel_DuplicateChecker()
        {
            if (base.IsInDesignMode)
            {

            }
            else
            {

            }
        }
        #endregion

        #region command methods

        #endregion

        #region methods

        #endregion
    }
}
