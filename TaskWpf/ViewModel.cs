using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TaskWpf
{
    class ViewModel : INotifyPropertyChanged
    {
        private ICommand _processParametersChangeCommand;
        private ICommand _processDirCommand;
        private ICommand _processDelCommand;
        private ProcessEntity _selectedItem;
        private WMIProcessRepository _processRepository = new WMIProcessRepository();

        public ViewModel()
        {
            _processParametersChangeCommand =
                new ProcessParametersChangeCommandImplementation(this);
            _processDirCommand = new ProcessDirCommandImplementation(this);
            _processDelCommand = new ProcessDelCommandImplementation(this);
            _processRepository.InitializeAllAndBeginTracking();
          

        }

        public WMIProcessRepository ProcessRepository
        {
            get { return _processRepository; }
        }

        public ICommand ProcessParametersChangeCommand
        {
            get { return _processParametersChangeCommand; }
        }
        public ICommand ProcessDirCommand
        {
            get { return _processDirCommand; }
        }

        public ICommand ProcessDelCommand
        {
            get { return _processDelCommand; }
        }

        class ProcessDirCommandImplementation : ICommand
        {
            ProcessEntity _targetProcess;
            private ViewModel _vMdl;
            public ProcessDirCommandImplementation(ViewModel vMdl)
            {
                _vMdl = vMdl;
                _targetProcess = _vMdl.SelectedItem;
            }

            public bool CanExecute(object parameter)
            {
                if (_vMdl._selectedItem == null) return false;
                return true; // _vMdl._selectedItem.CanUserAccess;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                String path = System.IO.Path.GetDirectoryName(_vMdl._selectedItem.FileName);
                System.Diagnostics.Process.Start(@path);
            }
        }

        class ProcessDelCommandImplementation : ICommand
        {
            private ViewModel _vMdl;
            public ProcessDelCommandImplementation(ViewModel vMdl)
            {
                _vMdl = vMdl;
            }

            public bool CanExecute(object parameter)
            {
                if (_vMdl._selectedItem == null) return false;
                return true; // _vMdl._selectedItem.CanUserAccess;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Are you sure?", "Kill Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)  // error is here
                {
                    _vMdl._selectedItem.Process.Kill();
                }
            }
        }

        class ProcessParametersChangeCommandImplementation : ICommand
        {
            private ViewModel _vMdl;
            public ProcessParametersChangeCommandImplementation(ViewModel vMdl)
            {
                _vMdl = vMdl;
            }

            public bool CanExecute(object parameter)
            {
                if (_vMdl._selectedItem == null) return false;
                return true; // _vMdl._selectedItem.CanUserAccess;
            }


            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                ShowProcess procAccWnd = new ShowProcess();
                procAccWnd.Show(_vMdl._selectedItem);
            }
        }

        

        public ProcessEntity SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged("SelectedItem"); }
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
