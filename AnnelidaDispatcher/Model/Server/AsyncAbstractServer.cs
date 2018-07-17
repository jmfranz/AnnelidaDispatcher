using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnnelidaDispatcher.Model.Watchdog;

namespace AnnelidaDispatcher.Model.Server
{
    public abstract class AsyncAbstractServer : AbstractServer
    {
        
        private TcpListener listener;
        protected CancellationTokenSource cts;

        public AsyncAbstractServer(int tcpPort)
        {
            cts = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, tcpPort);
            
        }

        public override void Start()
        {
            listener.Start();
            //TODO: Add public state property
            AcceptClientAsync(listener, cts.Token);
        }

        private async Task AcceptClientAsync(TcpListener listener, CancellationToken ct)
        {
            var clientCounter = 0;
            while (!ct.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                clientCounter++;
                connectedClients[ClientTypes.Types.Undefined].Add(client);
                ClientHandler(client, clientCounter, ct);
            }
        }

        protected abstract Task ClientHandler(TcpClient client, int clientIndex, CancellationToken ct);

        protected ClientTypes.Types IdentifyClient(byte[] buffer, TcpClient client)
        {
            //TODO: Handle incorrect types
            var myType = (ClientTypes.Types)BitConverter.ToInt32(buffer, 0);
            connectedClients[ClientTypes.Types.Undefined].Remove(client);
            connectedClients[myType].Add(client);
            return myType;
        }

        protected abstract void HandleMessage(byte[] buffer, int ammountRead, ClientTypes.Types myType);

        public override async Task StartWatchdogBroadcaster(int port, string token)
        {
            var watchdogBroadcaster = new WatchDogBroadcaster(port, token);
            while (!cts.IsCancellationRequested)
            {
                watchdogBroadcaster.SendWatchdogToken();
                await Task.Delay(new TimeSpan(0, 0, 0, 1));
            }
        }
    }
}
