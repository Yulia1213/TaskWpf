using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TaskWpf
{
    class ViewDetailModel : INotifyPropertyChanged
    {
        private ProcessDetails _processDetails;
        private Int32 _Id;
        readonly ICommand _cmdClose;

        public ViewDetailModel()
        {
            _cmdClose = new CmdCloseWindow(this);

        }

        public void setDetails(ProcessEntity process) {
            this._Id = process.Process.Id;
            _processDetails = new ProcessDetails(process);
        }

        public ProcessDetails Details {
            get { return _processDetails; }
        }


        public ICommand CloseWindow
        {
            get { return _cmdClose; }
        }

        class CmdCloseWindow : ICommand
        {
            ViewDetailModel _wnd;

            public CmdCloseWindow(ViewDetailModel wnd)
            {
                _wnd = wnd;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                _wnd._processDetails.Stop();
                
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
