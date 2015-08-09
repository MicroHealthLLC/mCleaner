using System;
using System.Collections.Generic;
using System.ComponentModel;
using mCleaner.ViewModel;
using Microsoft.Practices.ServiceLocation;

namespace mCleaner.Logics
{
    public class ProgressWorker
    {
        BackgroundWorker _bgWorker = new BackgroundWorker();
        Queue<string> _q = new Queue<string>();

        int _interval_s = 0; // interval when to show the progress text
        DateTime _start = DateTime.Now;
        DateTime _end = DateTime.Now;
        int _progress_type = 0;

        public ViewModel_CleanerML CleanerML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_CleanerML>();
            }
        }

        public ViewModel_DuplicateChecker DupChecker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewModel_DuplicateChecker>();
            }
        }

        public ProgressWorker()
        {
            this._bgWorker.WorkerReportsProgress = true;
            this._bgWorker.WorkerSupportsCancellation = true;

            this._bgWorker.RunWorkerCompleted += _bgWorker_RunWorkerCompleted;
            this._bgWorker.ProgressChanged += _bgWorker_ProgressChanged;
            this._bgWorker.DoWork += _bgWorker_DoWork;
        }
        static ProgressWorker _i = new ProgressWorker();
        public static ProgressWorker I { get { return _i; } }

        void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (this._bgWorker.CancellationPending == false)
            {
                //if (this._q.Count > -1 )
                if (this._q.Count == 0) break;

                string log = this._q.Dequeue();

                if (this._interval_s == 0)
                {
                    this._bgWorker.ReportProgress(-1, log);
                }
                else
                {
                    TimeSpan timespan = this._end - this._start;
                    int total_sec = ((int)timespan.TotalSeconds);

                    if (total_sec >= this._interval_s)
                    {
                        this._bgWorker.ReportProgress(-1, log);

                        this._start = DateTime.Now;
                    }
                }

                this._end = DateTime.Now;
            }
        }

        void _bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                if (e.UserState != null)
                {
                    string status = e.UserState.ToString();

                    string[] s = status.Split('|');

                    if (s.Length > 1)
                    {
                        this._progress_type = int.Parse(s[1]);
                    }
                    else
                    {
                        this._progress_type = 0;
                    }

                    switch (this._progress_type)
                    {
                        case 0:
                            this.CleanerML.ProgressText = s[0];
                            break;
                        case 1:
                            this.DupChecker.ProgressText = s[0];
                            break;
                    }
                }
            }
        }

        void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        public void EnQ(string log)
        {
            _q.Enqueue(log);
            Start();
        }

        public void Start()
        {
            if (!this._bgWorker.IsBusy)
            {
                this._start = DateTime.Now;
                this._bgWorker.RunWorkerAsync();
            }
        }

        public void Stop()
        {
            this._bgWorker.CancelAsync();
        }
    }
}
