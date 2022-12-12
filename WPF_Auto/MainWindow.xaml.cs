using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using KAutoHelper;
using Newtonsoft.Json;

namespace WPF_Auto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region data
        Bitmap CHROME_BMP;
        #endregion
        List<Charater> chars = new List<Charater>();
        LDPlayer ldplayer = new LDPlayer();
        
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            LoadDevices();
        }
        void LoadData()
        {
            //CHROME_BMP = (Bitmap)Bitmap.FromFile("data//ss1.png");

            LDPlayer.pathLD = "C:\\LDPlayer\\LDPlayer4.0\\ldconsole.exe";
            using (StreamReader sr = File.OpenText("./data.json"))
            {
                var obj = sr.ReadToEnd();
                chars = JsonConvert.DeserializeObject<List<Charater>>(obj);
                lstChar.ItemsSource = chars;
            }
        }

        bool isStart = false;
        void LoadDevices()
        {
            //while (true)
            //{
            //    if (isStop) return;

            //    Delay(2);
            //}
            //Task t = new Task(() =>
            //{
            //    while (true)
            //    {
            //        if (isStop) return;
            //        Application.Current.Dispatcher.Invoke(() =>
            //        {
            //            List<Device> devices = new List<Device>();
            //            var deviceIds = KAutoHelper.ADBHelper.GetDevices();
            //            for (int i = 0; i < deviceIds.Count; i++)
            //            {
            //                devices.Add(new Device() { Name = deviceIds[i] });
            //            }
            //            lstDevices.ItemsSource = devices;
            //            CollectionViewSource.GetDefaultView(lstDevices.ItemsSource).Refresh();
            //        });
            //        Delay(2);
            //    }

            //});
            //t.Start();
            //string deviceID = null;
            //var listDevice = KAutoHelper.ADBHelper.GetDevices();
            //if (listDevice != null && listDevice.Count > 0)
            //{
            //    deviceID = listDevice.First();
            //}

            //bool isPoint = true;
            //while (true)
            //{
            //    if (isStop) return;
            //    var sreen = KAutoHelper.ADBHelper.ScreenShoot(deviceID);
            //    var point = KAutoHelper.ImageScanOpenCV.FindOutPoint(sreen, CHROME_BMP);
            //    if (point != null)
            //    {
            //        KAutoHelper.ADBHelper.Tap(deviceID, point.Value.X, point.Value.Y);
            //        Delay(2);
            //    }
            //}

        }
        void Delay(int delay)
        {
            while (delay > 0)
            {
                if (!isStart) return;
                Thread.Sleep(TimeSpan.FromSeconds(1));
                delay--;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<Charater> _charsChk = chars.FindAll(c => c.isChecked);
            if (_charsChk.Count == 0) return;
            isStart = true;
            var devices = ldplayer.GetDevices();

            int _count = _charsChk.Count > devices.Count ? devices.Count : _charsChk.Count;
            for(int i = 0; i < _count; i++)
            {
                ldplayer.Open("name", devices[i]);
            }
            foreach (Charater item in chars)
            {
                if (item.isChecked)
                {
                    item.status = "Wait";
                }
            }
            CollectionViewSource.GetDefaultView(lstChar.ItemsSource).Refresh();
            
            WaitRunDevices();
        }

        private void WaitRunDevices()
        {
            new Task(() =>
            {
                //waitCPU();
                while (isStart)
                {
                    var devicesRuning = ldplayer.GetDevices2_Running();
                    List<Charater> _charsDrop = chars.FindAll(c => c.deviceName != null && devicesRuning.Find(d => d.name == c.deviceName).name == null);
                    List<Charater> _charsChk = chars.FindAll(c => c.isChecked);
                    if (_charsChk.Count == 0) isStart = false;

                    if (!isStart) return;
                    List<string> devices = new List<string>();

                    foreach (var device in devicesRuning)
                    {
                        Charater _char = chars.Find(c => c.deviceName == device.name);
                        if (_char == null && device.adb_id != "-1")
                        {
                            Charater _charwait = chars.Find(c => c.isChecked && c.status == "Wait");
                            if (_charwait != null)
                            {
                                _charwait.status = "Running";
                                _charwait.deviceId = device.adb_id;
                                _charwait.deviceName = device.name;
                                Auto(_charwait);

                            }
                        }
                    };

                    if (_charsDrop.Count > 0)
                    {
                        foreach(Charater _char in _charsDrop)
                        {
                            _char.status = "";
                            _char.deviceId = null;
                            _char.deviceName = null;
                            _char.isChecked = false;
                        }

                    }

                    Application.Current.Dispatcher.Invoke(() => {
                        CollectionViewSource.GetDefaultView(lstChar.ItemsSource).Refresh();
                    });
                    Delay(3);
                }
            }).Start();
        }



        public PerformanceCounter myCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");

        private void waitCPU()
        {
            var check = true;
            while (check)
            {
                Delay(10);
                ManagementObjectSearcher cpuSearch = new ManagementObjectSearcher(@"\root\CIMV2",
                      "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name=\"_Total\"");
                ManagementObjectCollection moc = cpuSearch.Get();
                ManagementObject mo = moc.Cast<ManagementObject>().First();
                string cpu = mo["PercentIdleTime"].ToString();
                if (Int32.Parse(cpu) < 30) check = false;
            }
        }

        //private void checkDevice()
        //{
        //    new Task(() =>
        //    {
        //        while (true)
        //        {
        //            if (!isStart) return;
        //            Delay(2);
        //            var listDevice = KAutoHelper.ADBHelper.GetDevices();
        //            List<Charater> _chars = chars.FindAll(c => c.isChecked && c.status == "Running");
        //            var drivers = listDevice.FindAll(d => chars.Find(c => c.deviceId == d) == null);
        //        }
        //    }).Start();
        //}

        private void Auto(Charater item)
        {
            new Task(() =>
            {
                Delay(10);
                ldplayer.RunApp("name", item.deviceName, "com.kv1.mobi");
                Delay(60);
                ldplayer.KillApp("name", item.deviceName, "com.kv1.mobi");
                //if (!isStart) return;
                //Charater _char = chars.Find(c => c.isChecked && c.status == "Wait");
                //if (_char != null)
                //{
                //    _char.status = "Running";
                //    _char.deviceId = item.deviceId;
                //    _char.deviceName = item.deviceName;
                //}
                item.status = "";
                item.deviceId = null;
                item.deviceName = null;
                item.isChecked = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CollectionViewSource.GetDefaultView(lstChar.ItemsSource).Refresh();
                });
                //Auto(_char);
            }).Start();
        }

        class Charater
        {
            public string username { get; set; }
            public string password { get; set; }
            public bool isChecked { get; set; }
            public string status { get; set; }
            public string deviceId { get; set; }
            public string deviceName { get; set; }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            isStart = false;
            foreach (Charater item in chars)
            {
                if (item.isChecked)
                {
                    item.status = "";
                    item.deviceId = null;
                    item.deviceName = null;
                }

            }
            CollectionViewSource.GetDefaultView(lstChar.ItemsSource).Refresh();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            bool isChecked = (bool)cb.IsChecked;

            foreach (Charater item in chars)
            {
                item.isChecked = isChecked;
            }
            CollectionViewSource.GetDefaultView(lstChar.ItemsSource).Refresh();
        }
    }
}
