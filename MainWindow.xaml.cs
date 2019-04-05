using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel vm;
        public MainWindow()
        {
            InitializeComponent();

            vm = new ViewModel();
            DataContext = vm;
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

        }
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            App.Current.Shutdown();
        }

        
        private void ProcessGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            
                if (vm.SelectedItem != null) {
                       ProcessGrid.SelectedItem = vm.SelectedItem;
                                ProcessGrid.Dispatcher.InvokeAsync(() =>
                                {
                                    ProcessGrid.ScrollIntoView(ProcessGrid.SelectedItem, null);
                                });

                }
            

        }
    }
}
