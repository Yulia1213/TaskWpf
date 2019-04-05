using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskWpf
{
    class ProcessDetails : INotifyPropertyChanged
    {
        private ObservableCollection<Stream> _streams;
        private ObservableCollection<Module> _modules;
        private Stream _selStream;
        private Module _selModule;
        private Thread streamsThread;
        private Thread modulesThread;
        private int _streamCount = 0;
        private Int32 _Id;
        private bool _isEnabled;
       

        public ProcessDetails(ProcessEntity process)
        {
            this._Id = process.Process.Id;
            this._isEnabled = process.CanUserAccess;
            streamsThread = new Thread(updateStreams);
            streamsThread.Start();
            modulesThread  =  new Thread(readMoudeles);
            modulesThread.Start();
        }


        private void updateModules(ProcessModuleCollection mo)
        {
            
            lock (_modules)
            {
                for (int i = 0; i < mo.Count; i++)
                {
                    Module t = new Module(mo[i]);
                    _modules.Add(t);
                }
            }
            OnPropertyChanged("Modules");
        }


        private void readMoudeles()
        {
            while (true)
            {
                try
            {
                if (Process.GetProcessById(_Id).Threads != null)
                {
                    ProcessModuleCollection mo = Process.GetProcessById(_Id).Modules;
                    _modules = new ObservableCollection<Module>();
                    updateModules(mo);
                }
            }
            catch (Exception ex)
            {

            }
                Thread.Sleep(2000);

            }
        }

        private void updateStreams()
        {
            while (true)
            {
                try
                {
                    if (Process.GetProcessById(_Id).Threads != null)
                    {
                        ProcessThreadCollection c = Process.GetProcessById(_Id).Threads;
                        _streams = new ObservableCollection<Stream>();
                        addStreams(c);
                    }
                }
                catch
                {

                }
                Thread.Sleep(2000);

            }
        }

        private void addStreams(ProcessThreadCollection threads)
        {
            lock (_streams)
            {
                for (int i = 0; i < threads.Count; i++)
                {
                    Stream s = new Stream();
                    ProcessThread t = threads[i];
                    s.Id = t.Id;
                    switch (t.ThreadState)
                    {
                        case System.Diagnostics.ThreadState.Initialized: s.State = "Initialized"; break;
                        case System.Diagnostics.ThreadState.Ready: s.State = "Ready"; break;
                        case System.Diagnostics.ThreadState.Running: s.State = "Running"; break;
                        case System.Diagnostics.ThreadState.Standby: s.State = "Standby"; break;
                        case System.Diagnostics.ThreadState.Terminated: s.State = "Terminated"; break;
                        case System.Diagnostics.ThreadState.Transition: s.State = "Transition"; break;
                        case System.Diagnostics.ThreadState.Unknown: s.State = "Unknown"; break;
                        case System.Diagnostics.ThreadState.Wait: s.State = "Wait"; break;
                        default: s.State = "---"; break;
                    }
                    s.StartTime = t.StartTime.ToString("G");


                    _streams.Add(s);

                }
            }
            OnPropertyChanged("Streams");
            _streamCount = _streams.Count;
            OnPropertyChanged("StreamCount");
        }
        public Stream SelectedStream {
            get { return _selStream; }
            set { _selStream = value;
                OnPropertyChanged("SelectedStream");
            }
        }

        public Module SelectedModule {
            get  { return _selModule; }
            set { _selModule = value;
                OnPropertyChanged("SelectedModule");
            }
        }

        public void Stop() {
            if (streamsThread.IsAlive) { streamsThread.Abort(); }
            if (modulesThread.IsAlive) { modulesThread.Abort(); }
        }

        public bool Enabled {
            get { return _isEnabled; }
        }

        public ObservableCollection<Stream> Streams
        {
            get { return _streams; }
        }

        public ObservableCollection<Module> Modules
        { get { return _modules; } }


        public int StreamCount
        {
            get { return _streamCount; }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        

    }
}
