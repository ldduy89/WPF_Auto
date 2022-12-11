using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            LoadDevices();

        }
        void LoadData()
        {
            //CHROME_BMP = (Bitmap)Bitmap.FromFile("data//ss1.png");

            using (StreamReader sr = File.OpenText("./data.json"))
            {
                var obj = sr.ReadToEnd();
                chars = JsonConvert.DeserializeObject<List<Charater>>(obj);
                lstChar.ItemsSource = chars;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task t = new Task(() =>
            {

            });
            t.Start();

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
            var listDevice = KAutoHelper.ADBHelper.GetDevices();
            int index = 0;
            if (listDevice.Count == 0) return;
            isStart = true;
            foreach (Charater item in chars)
            {
                if (item.isChecked)
                {
                    if (index < listDevice.Count)
                    {
                        item.status = "playing";
                        item.deviceId = listDevice[index];
                        Auto(item);
                        index++;
                    }
                    else
                    {
                        item.status = "waiting";
                    }

                }

            }
            CollectionViewSource.GetDefaultView(lstChar.ItemsSource).Refresh();
            //checkDevice();
        }

        private void checkDevice()
        {
            new Task(() =>
            {
                while (true)
                {
                    if (!isStart) return;
                    Delay(2);
                    var listDevice = KAutoHelper.ADBHelper.GetDevices();
                    List<Charater> _chars = chars.FindAll(c => c.isChecked && c.status == "playing");
                    var drivers = listDevice.FindAll(d => chars.Find(c => c.deviceId == d) == null);
                }
            }).Start();
        }

        private void Auto(Charater item)
        {
            Task t = new Task(() =>
            {
                Delay(5);
                if (!isStart) return;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Charater _char = chars.Find(c => c.isChecked && c.status == "waiting");
                    if (_char != null)
                    {
                        _char.status = "playing";
                        _char.deviceId = item.deviceId;
                        Auto(_char);
                    }
                    item.status = "";
                    item.deviceId = "";
                    item.isChecked = false;
                    CollectionViewSource.GetDefaultView(lstChar.ItemsSource).Refresh();
                });
            });
            t.Start();
        }

        class Charater
        {
            public string username { get; set; }
            public string password { get; set; }
            public bool isChecked { get; set; }
            public string status { get; set; }
            public string deviceId { get; set; }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _ = chars;
            isStart = false;
            foreach (Charater item in chars)
            {
                if (item.isChecked)
                {

                    item.status = "";
                    item.deviceId = "";

                }

            }
            CollectionViewSource.GetDefaultView(lstChar.ItemsSource).Refresh();
        }

        private void lstChar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
