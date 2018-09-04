using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using AnnelidaDispatcher.Utilities;
using AnnelidaDispatcher.Model;
using AnnelidaDispatcher.Model.Server;


namespace AnnelidaDispatcher.ViewModel
{
    /// <summary>
    /// Class that binds the ispacher view with the model
    /// </summary>
    internal class MainViewViewModel: INotifyPropertyChanged
    {
        #region Properties
  
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
        public int MyPort { get; }
        public string MongoUrl { get; set;}
        public string SensorDbName { get; set; }
        public string ControlDbName { get; set; }
        public string MissionName { get; set; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private AbstractServer asyncDispatcherServer;

        /// <summary>
        /// Initializes all the necessary objects for the dispacher to work.
        /// Port is fixed on 9999
        /// </summary>
        //TODO: allow for default port to be defined by the user
        //TODO: if allowed to change then fix disp.Start(9999)
        public MainViewViewModel()
        {
            ViewClients = new MTObservableCollection<string>();
            ControlClients = new MTObservableCollection<string>();
            RobotClients = new MTObservableCollection<string>();
            MyIP = GetLocalIpAddress();
            MyPort = 9999;

            StartButtonEnabled = true;

            var missionName = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

            SensorDbName = "sensors";
            ControlDbName = "control";
            MissionName = missionName;

            asyncDispatcherServer = new ProtoBufServer(MyPort);
            //AbstractServer asyncDispatcherServer = new AsyncDispatcherServerDBEnabled(missionName,MyPort);
            asyncDispatcherServer.ClientConnectedEvent += ClientConnected;
            asyncDispatcherServer.ClientDisconnectedEvent += ClientDisconnected;
            asyncDispatcherServer.Start();

        }

        public MainViewViewModel(string watchdogToken) : this()
        {
            asyncDispatcherServer.StartWatchdogBroadcaster(9998, watchdogToken);
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
                case ClientTypes.Types.Undefined:
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
                case ClientTypes.Types.Undefined:
                    break;
                
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ReSharper disable once UnusedMember.Local
        public static string GetLocalIpAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            throw new Exception("Local IP Address Not Found!");
        }
    }
}
