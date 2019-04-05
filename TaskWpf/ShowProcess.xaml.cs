using System;
using System.Windows;
using System.Windows.Input;

namespace TaskWpf
{
    /// <summary>
    /// Логика взаимодействия для ShowProcess.xaml
    /// </summary>
    /// 
   

    public partial class ShowProcess : Window
    {
        private ProcessEntity _targetProcess;

        ViewDetailModel _vdm;
    


        public ShowProcess()
        {
            InitializeComponent();
            _vdm = new ViewDetailModel();
          
            DataContext = _vdm;
        }
        public void Show(ProcessEntity targetProcess)
        {
            
            _targetProcess = targetProcess;
            _vdm.setDetails(_targetProcess);
           
            base.Show();
        }


    }
}
