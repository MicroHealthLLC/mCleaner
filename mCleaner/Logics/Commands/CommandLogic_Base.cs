using mCleaner.Model;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner.Logics.Commands
{
    public class CommandLogic_Base
    {
        public string _preview_log = string.Empty;

        ViewModel_CleanerML VMCleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }

        private action _Action = new action();
        public action Action
        {
            get { return _Action; }
            set
            {
                if (_Action != value)
                {
                    _Action = value;
                }
            }
        }

        public void UpdateProgressLog(string text, bool update_progress_text = true)
        {
            if (update_progress_text) ProgressWorker.I.EnQ(text);

            VMCleanerML.TextLog = _preview_log;
            //VMCleanerML.MaxProgress = this.TTD.Count;
            //VMCleanerML.ProgressIndex++;
        }
    }
}
