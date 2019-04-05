using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace TaskWpf
{
    class WMIProcessRepository : INotifyPropertyChanged
    {
        //Имя пользователя
        private static String _userName = Environment.UserName;
        private static String _domainName = Environment.UserDomainName;
        //Процессы
        private ObservableCollection<ProcessEntity> _processEntitiesCollection;

        public ObservableCollection<ProcessEntity> ProcessEntitiesCollection
        {
            set { _processEntitiesCollection = value; }
            get { return _processEntitiesCollection; }
        }

        public WMIProcessRepository()
        {
           // Thread t  = new Thread(InitializeAllAndBeginTracking);
           // t.Start();
            
            //InitializeAllAndBeginTracking();
        }

        public string[] GetProcessOwner(Int32 processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    // return DOMAIN\user
                    return argList;
                }
            }
            return null;
        }
            //Инициализация списка процессов
        public void InitializeAllAndBeginTracking()
        {

            _processEntitiesCollection = new ObservableCollection<ProcessEntity>();
            //ObjectQuery processQueryObject = new ObjectQuery();
            //ManagementObjectSearcher processSearcher = new ManagementObjectSearcher();
             //ManagementObjectCollection foundProcessesList;
             //processQueryObject.QueryString = "Select * From Win32_Process"; //Строка запроса для обьекта
              //processSearcher.Query = processQueryObject; //Обьект запроса
            //foundProcessesList = processSearcher.Get(); //Выполнение запроса
            //foreach (ManagementObject managementObject in foundProcessesList)
            lock (_processEntitiesCollection)
            {
                //foreach (ManagementObject managementObject in foundProcessesList)
                foreach (Process proccess in Process.GetProcesses())
                {
                    try
                    {
                    //    Int32 processId = (Int32)managementObject.Properties["ProcessId"].Value;
                        Int32 processId = proccess.Id;
                        if (processId == 0 || processId == 4) continue; //System Idle не является реальным процессом
                                                                        //Массив из двух строк, в который запишутся домен и владелец
                        String[] processOwner = GetProcessOwner(processId);// { "username", "domain" };
                                                                           // managementObject.InvokeMethod("GetOwner", processOwner); //Владелец
                        String userName = "Нет доступа";
                        Boolean canUserAccess = false; //Пользователь имеет доступ только к своим (не системным) процессам
                        if (processOwner != null)
                        {
                            canUserAccess = processOwner[0] == _userName && processOwner[1] == _domainName;
                            userName = processOwner[0];
                        }
                        //Добавление процесса в коллекцию
                        _processEntitiesCollection.Add(new ProcessEntity(processId, canUserAccess, userName));
                    }
                    catch
                    {

                    }
                }

                
            }

            //Отслеживание изменений процессов (появился новый \ пропал из имеющихся)
            PeriodicScanForNewAndGhostProcesses();

            //Периодическое обновления показателей использования ЦП и памяти
            System.Timers.Timer updateProcessParamsTimer = new System.Timers.Timer(); //Таймер обновления
            updateProcessParamsTimer.Elapsed +=
                (sender, eventArgs) => RefreshProcessesData(sender, eventArgs);
            updateProcessParamsTimer.Interval = 1000;
            updateProcessParamsTimer.Enabled = true;
            updateProcessParamsTimer.Start();
            OnPropertyChanged("ProcessEntitiesCollection");
        }

       


        //Обновление процента использования ЦП и памяти. Выполняется в отдельном потоке
        public void RefreshProcessesData(object sender, EventArgs timerArguments)
        {
            //Пока данные процесса обновляются, нельзя его удалять\менять приоритет и так далее
            lock (_processEntitiesCollection)
            {

                for (int i = 0; i < _processEntitiesCollection.Count; i++)
                {
                    try
                    {
                        ProcessEntity processEntity = _processEntitiesCollection[i];
                        if (processEntity.Process.HasExited) continue;
                        processEntity.CpuLoadPercentage = //Использование ЦП
                            (float)Math.Round(processEntity.CpuLoadCounter.NextValue(), 2);
                        float t = processEntity.MemUsageCounter.NextValue();
                        processEntity.MemoryMBytesUsage = //Использование ОП
                            (float)Math.Round(t / 1024 / 1024, 2);
                        processEntity.MemoryProcent = (float)Math.Round(100 * t / processEntity.TotalMem, 2);

                    }
                    catch { }
                }
            }
        }
        //Отслеживает через WMI события появления и исчезновения процессов 1 per/sec
        public void PeriodicScanForNewAndGhostProcesses()
        {
            TimeSpan updateInterval = new TimeSpan(0, 0, 1);


            WqlEventQuery queryCreate =
                 new WqlEventQuery("__InstanceCreationEvent", updateInterval, "TargetInstance isa \"Win32_Process\"");

            ManagementEventWatcher instanceCreationWatcher = new ManagementEventWatcher();
            instanceCreationWatcher.Query = queryCreate; // instanceCreationMonitorQuery;
            instanceCreationWatcher.Options.Timeout = updateInterval;
            instanceCreationWatcher.EventArrived += new EventArrivedEventHandler(ProcessStartEvent);
            instanceCreationWatcher.Start();

            //Отслеживание закрытия
            WqlEventQuery instanceDeletionQuery =
                    new WqlEventQuery("__InstanceDeletionEvent", updateInterval, "TargetInstance isa \"Win32_Process\"");
            ManagementEventWatcher instanceDeletionWatcher = new ManagementEventWatcher(instanceDeletionQuery);
            instanceDeletionWatcher.Query = instanceDeletionQuery;
            instanceDeletionWatcher.Options.Timeout = updateInterval;
            instanceDeletionWatcher.EventArrived += new EventArrivedEventHandler(ProcessEndEvent);
            instanceDeletionWatcher.Start();

        }
        //Реакция на появление процесса
        public void ProcessStartEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject newEvent = e.NewEvent;
            ManagementBaseObject targetInstanceBase = (ManagementBaseObject)newEvent["TargetInstance"];

            UInt32 processId = (UInt32)targetInstanceBase.Properties["ProcessId"].Value;
            //newEvent.

            ObjectQuery processQueryObject = new ObjectQuery();
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher();
            ManagementObjectCollection foundProcessesList;
            processQueryObject.QueryString = //Строка запроса для обьекта
                "Select * From Win32_Process WHERE ProcessId = " + processId;
            processSearcher.Query = processQueryObject; //Обьект запроса
            foundProcessesList = processSearcher.Get(); //Выполнение запроса
            foreach (ManagementObject managementObject in foundProcessesList)
            {
                //Массив из двух строк, в который запишутся домен и владелец
                String[] processOwner = { "username", "domain" };
                //запоминаем выбранный id
                
                try
                {
                    managementObject.InvokeMethod("GetOwner", processOwner); //Владелец
                    Boolean canUserAccess = //Пользователь имеет доступ только к своим (не системным) процессам
                        processOwner[0] == _userName && processOwner[1] == _domainName;
                    //Добавление процесса в коллекцию
                    
                    lock (_processEntitiesCollection)
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                            new Action(() =>
                                _processEntitiesCollection.Add(
                                new ProcessEntity((Int32)processId, canUserAccess, processOwner[0]))));
                    }
                }
                catch
                {
                }

            }
        }
        //Реакция на исчезновение процесса
        public void ProcessEndEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject newEvent = e.NewEvent;
            ManagementBaseObject targetInstance = (ManagementBaseObject)newEvent["TargetInstance"];
            UInt32 processId = (UInt32)targetInstance.Properties["ProcessId"].Value;

            lock (_processEntitiesCollection)
            {
                for (UInt16 i = 0; i < _processEntitiesCollection.Count; ++i)
                {
                    if (_processEntitiesCollection[i].Process.Id == processId)
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                            new Action(() => _processEntitiesCollection.Remove(
                                _processEntitiesCollection[i])));
                        break;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }



}
