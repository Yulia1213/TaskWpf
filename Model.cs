using System;
using System.Diagnostics;
using System.Management;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Timers = System.Timers;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Threading;

namespace TaskWpf
{



    public class ProcessEntity : INotifyPropertyChanged
    {
        private Int32 _identity;
        private Process _process; //Процесс
        private Boolean _canUserAccess; //Может ли пользователь менять процесс
        private readonly PerformanceCounter _cpuLoadCounter; //Отслеживает загрузку ЦП
        private readonly PerformanceCounter _memUsageCounter;
        private float _cpuLoadPercentage; //Загрузка ЦП
        private float _memoryMBytesUsage; //Использование памяти в байтах
        private float _memPro;
        private String _fileName;
        private String _userName;
        private String _startTime;
        private long _totalMem;


        public ProcessEntity(Int32 identity, Boolean canUserAccess, String userName)
        {

            _identity = (Int32)identity;
            _process = Process.GetProcessById(Convert.ToInt32(identity)); //Процесс
            _canUserAccess = canUserAccess; //Может ли пользователь менять
            _userName = userName;
            //Производительность
            _cpuLoadCounter =
                new PerformanceCounter("Process", "% Processor Time", _process.ProcessName, true);
            _memUsageCounter =
                new PerformanceCounter("Process", "Working Set", _process.ProcessName, true);
            _totalMem = (long)new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;

            try
            {
                _fileName = _process.Modules[0].FileName;
            }
            catch 
            {
                _fileName = "Отказано в доступе";
            }
            try
            {
                _startTime = _process.StartTime.ToString("G");
            }
            catch
            {
                _startTime = "Отказано в доступе";
            }


        }



      
        public Process Process
        {
            get { return _process; }
        }


        public long TotalMem
        {
            get { return _totalMem; }
        }

        public String UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }
        public String StartTime
        {
            get { return _startTime; }
        }

        public String FileName
        {
            get { return _fileName; }
        }

        public Boolean CanUserAccess
        {
            get { return _canUserAccess; }
        }

        public PerformanceCounter CpuLoadCounter
        {
            get { return _cpuLoadCounter; }
        }

        public PerformanceCounter MemUsageCounter
        {
            get { return _memUsageCounter; }
        }

        public float MemoryProcent
        {
            set { _memPro = value; OnPropertyChanged("MemoryProcent"); }
            get { return _memPro; }
        }

        public float MemoryMBytesUsage
        {
            set { _memoryMBytesUsage = value; OnPropertyChanged("MemoryMBytesUsage"); }
            get { return _memoryMBytesUsage; }
        }

        public float CpuLoadPercentage
        {
            set { _cpuLoadPercentage = value; OnPropertyChanged("CpuLoadPercentage"); }
            get { return _cpuLoadPercentage; }
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

        public BitmapImage IconSource
        {
            get
            {
                BitmapImage bImg = new BitmapImage();
                try
                {
                    Icon procIco = Icon.ExtractAssociatedIcon(_process.MainModule.FileName);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Bitmap dImg = procIco.ToBitmap();
                        dImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        bImg.BeginInit();
                        bImg.StreamSource = new MemoryStream(ms.ToArray());
                        bImg.EndInit();
                    }
                }
                catch
                {
                }
                return bImg;
            }
        }

    }



}