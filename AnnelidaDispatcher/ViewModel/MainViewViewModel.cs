using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using AnnelidaDispatcher.Utilities;
using AnnelidaDispatcher.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;

namespace AnnelidaDispatcher.ViewModel
{
    class MainViewViewModel: INotifyPropertyChanged
    {
        #region Properties
        #region StartCommand
        private ICommand startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (startCommand == null)
                    startCommand = new RelayCommand(call => StartListening());
                return startCommand;
            }
        }
        #endregion
        #region StartButtonState
        private Boolean startButtonEnabled;
        public Boolean StartButtonEnabled
        {
            get { return startButtonEnabled;}
            set
            {
                startButtonEnabled = value;
                OnPropertyChanged(nameof(StartButtonEnabled));
            }
        }
            

        #endregion
        

        public MTObservableCollection<string> ViewClients { get; private set; }
        public MTObservableCollection<string> ControlClients { get; private set; }
        public MTObservableCollection<string> RobotClients { get; private set; }
        public string MyIP { get; }
        public string MyPort { get; }
        public string MongoURL { get; set;}
        public string SensorDBName { get; set; }
        public string ControlDBName { get; set; }
        public string MissionName { get; set; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private DispatcherServer disp;
               

        public MainViewViewModel()
        {
            ViewClients = new MTObservableCollection<string>();
            ControlClients = new MTObservableCollection<string>();
            RobotClients = new MTObservableCollection<string>();
            MyIP = DispatcherServer.GetLocalIPAddress();
            MyPort = "9999";

            StartButtonEnabled = true;
        }
        async void StartListening()
        {
            if (SensorDBName == null || ControlDBName == null || MissionName == null)
            {
                //Sets values to default
                SensorDBName = "sensors";
                ControlDBName = "control";
                MissionName = "default";
            }
            StartButtonEnabled = false;
            

            var sensorDB = new MongoWrapper(MongoURL, SensorDBName);
            var controlDB = new MongoWrapper(MongoURL, ControlDBName);
            disp = new DispatcherServer(sensorDB, controlDB, MissionName);
            disp.clientConnectedEvent += ClientConnected;
            disp.clientDisconnectedEvent += ClientDisconnected;
            await Task.Run(() => disp.Start(9999));

        }

        private void ClientConnected(ClientTypes.Types type, string addr)
        {
            switch(type)
            {
                case ClientTypes.Types.View:
                    ViewClients.Add(addr);
                    break;
                case ClientTypes.Types.Controller:
                    ControlClients.Add(addr);
                    break;
                case ClientTypes.Types.Robot:
                    RobotClients.Add(addr);
                    break;
                default:
                    break;
            }
        }

        private void ClientDisconnected(ClientTypes.Types type, string addr)
        {
            switch (type)
            {
                case ClientTypes.Types.View:
                    ViewClients.Remove(addr);
                    break;
                case ClientTypes.Types.Controller:
                    ControlClients.Remove(addr);
                    break;
                case ClientTypes.Types.Robot:
                    RobotClients.Remove(addr);
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
