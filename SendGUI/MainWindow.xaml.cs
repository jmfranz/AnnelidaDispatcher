using AnnelidaDispatcher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

using MongoDB.Bson;
using System.Threading;

namespace SendGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Sensors document;
        //Random rnd = new Random();
        List<string> faultList;
        public bool shouldSend { get; set; } = false;
        uint count = 0;
        NetworkStream stream;
        int currentFault = 0;

        public MainWindow()
        {
            document = new Sensors();
            faultList = new List<string>();
            InitializeComponent();
        }



        void Initialize()
        {
            TcpClient c = new TcpClient();
            c.Connect("127.0.0.1", 9999);
            stream = c.GetStream();

            
            int id = 3;
            stream.Write(BitConverter.GetBytes(id), 0, 4);
            stream.Flush();
            AssembleDoc();
        }

        async void SendData()
        {
            while (true)
            {
                count++;
                document.timestamp = DateTime.UtcNow;
                if (shouldSend)
                {
                    document.backward.enclosure1.pressure = 2 * Math.Sin(count * Math.PI / 180);
                    document.backward.enclosure1.temperature = 2 * Math.Sin(count * Math.PI / 180 + 5 * Math.PI / 180); ;
                    document.backward.enclosure1.traction = 2 * Math.Sin(count * Math.PI / 180 + 10 * Math.PI / 180);
                    document.faultList = this.faultList;
                    byte[] b = document.ToBson();
                    stream.Write(b, 0, b.Length);
                    stream.Flush();
                    await Task.Delay(100);
                }
            }
                
        }

        private void RemoveFault_Click(object sender, RoutedEventArgs e)
        {
            
            faultList.Remove(faultValue.Text);
            
        }

        private void AddFault_Click(object sender, RoutedEventArgs e)
        {
            faultList.Add(faultValue.Text);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Initialize();
            shouldSend = true;
            SendData();
        }

        void AssembleDoc()
        {
            document.backward.enclosure1.pressure = 0;
            document.backward.enclosure1.temperature = 0;
            document.backward.enclosure1.traction = 0;
            document.backward.enclosure1.orientation.Add(0);
            document.backward.enclosure1.orientation.Add(0);
            document.backward.enclosure1.orientation.Add(0);

            document.backward.enclosure2.temperature = 0;
            document.backward.enclosure2.pressure = 0;
            document.backward.enclosure2.inputCurrent = 0;
            document.backward.enclosure2.inputVoltage = 0;
            document.backward.enclosure2.outputCurrent = 0;
            document.backward.enclosure2.outputVoltage = 0;

            document.backward.enclosure3.engineSpeed.Add(0);
            document.backward.enclosure3.engineSpeed.Add(0);
            document.backward.enclosure3.engineSpeed.Add(0);
            document.backward.enclosure3.appliedPower.Add(0);
            document.backward.enclosure3.appliedPower.Add(0);
            document.backward.enclosure3.appliedPower.Add(0);

            document.central.enclosure4.temperature = 0;
            document.central.enclosure4.pressure = 0;
            document.central.enclosure4.engineControllerCurrent = 0;
            document.central.enclosure4.engineControllerTemperature = 0;

            document.central.enclosure5.pressure = 0;
            document.central.enclosure5.temperature = 0;
            document.central.enclosure5.travel = 0;
            document.central.enclosure5.orientation.Add(0);
            document.central.enclosure5.orientation.Add(0);
            document.central.enclosure5.orientation.Add(0);

            document.central.enclosure6.temperature = 0;
            document.central.enclosure6.pressure = 0;
            document.central.enclosure6.engineControllerCurrent = 0;
            document.central.enclosure6.engineControllerTemperature = 0;

            document.forward.enclosure7.engineSpeed.Add(0);
            document.forward.enclosure7.engineSpeed.Add(0);
            document.forward.enclosure7.engineSpeed.Add(0);
            document.forward.enclosure7.appliedPower.Add(0);
            document.forward.enclosure7.appliedPower.Add(0);
            document.forward.enclosure7.appliedPower.Add(0);

            document.forward.enclosure8.externalPressure = 0;
            document.forward.enclosure8.externalTemperature = 0;
        }


    }
}
