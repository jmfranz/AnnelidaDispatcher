using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using AnnelidaDispatcher.Utilities;
using AnnelidaDispatcher.Model;
using System.Collections.ObjectModel;

namespace AnnelidaDispatcher.ViewModel
{
    class MainViewViewModel
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
        #endregion

        public MTObservableCollection<string> ViewClients { get; private set; }
        public MTObservableCollection<string> ControlClients { get; private set; }
        public MTObservableCollection<string> RobotClients { get; private set; }
        public string MyIP { get; }
        public string MyPort { get; }

        private DispatcherServer disp;

        public MainViewViewModel()
        {
            ViewClients = new MTObservableCollection<string>();
            ControlClients = new MTObservableCollection<string>();
            RobotClients = new MTObservableCollection<string>();
            disp = new DispatcherServer();
            disp.clientConnectedEvent += ClientConnected;
            disp.clientDisconnectedEvent += ClientDisconnected;
            MyIP = DispatcherServer.GetLocalIPAddress();
            MyPort = "9999";


        }
        async void StartListening()
        {
            
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
    }
}
