using System;
using System.Windows;
using System.Windows.Threading;

namespace mCleaner.Helpers
{
    public static class Help
    {
        static int last_s = 0;
        public static void RunInBackground(Action callback, bool timed_update = false, Action callback_no_affected_by_time_update = null)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (callback_no_affected_by_time_update != null)
                {
                    callback_no_affected_by_time_update();
                }

                bool go = false;

                if (timed_update)
                {
                    if (DateTime.Now.Second != last_s)
                    {
                        last_s = DateTime.Now.Second;
                        go = true;
                    }
                }
                else
                {
                    go = true;
                }

                if(go) callback();
            }), DispatcherPriority.Background, null);   
        }
    }
}
