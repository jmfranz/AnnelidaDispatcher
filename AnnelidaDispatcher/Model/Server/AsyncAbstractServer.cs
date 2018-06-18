using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnnelidaDispatcher.Model.Server
{
    public abstract class AsyncAbstractServer : AbstractServer
    {
        private CancellationTokenSource cts;
        private TcpListener listener;


        public AsyncAbstractServer(int tcpPort)
        {
            cts = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, tcpPort);
            messageDispatchStrategy = new NoDBDispatchStrategy();
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

        private async Task ClientHandler(TcpClient client, int clientIndex, CancellationToken ct)
        {
            Console.WriteLine($"New client ({clientIndex}) connected");
            var myType = ClientTypes.Types.Undefined;
            var clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            using (client)
            {
                var buf = new byte[4096];
                byte[] completedMessage = null;
                var stream = client.GetStream();
                int count = 1;
                var ammountToReceive = 4;
                var totalMessageSize = 0;
                var totalReceivedSize = 0;
                while (!ct.IsCancellationRequested)
                {
                    var timeOutTask = Task.Delay(TimeSpan.FromMinutes(1));
                    var ammountReadTask = stream.ReadAsync(buf, 0, ammountToReceive, ct);
                    var completedTask = await Task.WhenAny(ammountReadTask).ConfigureAwait(false);

                    if (completedTask == timeOutTask)
                    {
                        break;
                    }

                    int ammountRead;

                    try
                    {
                        ammountRead = ammountReadTask.Result;
                    }
                    catch (AggregateException ex)
                    {
                        Console.WriteLine(ex);
                        break;
                    }

                    if (ammountRead == 0) break;


                    if (myType == ClientTypes.Types.Undefined)
                    {
                        myType = IdentifyClient(buf, client);
                        OnClientConnected(myType, clientEndPoint.Address.ToString());
                        //ClientConnectedEvent?.Invoke(myType, clientEndPoint.Address.ToString());
                    }
                    else
                    {
                        totalReceivedSize += ammountRead;
                        if (ammountToReceive == 4)
                        {
                            byte[] size = { buf[0], buf[1], buf[2], buf[3] };
                            totalMessageSize = BitConverter.ToInt32(size, 0);
                            ammountToReceive = totalMessageSize - 4;
                            completedMessage = new byte[totalMessageSize];
                            Array.Copy(size, completedMessage, 4);

                        }
                        else
                        {
                            Array.Copy(buf, 0, completedMessage, totalReceivedSize - ammountRead, ammountRead);

                            if (totalReceivedSize == totalMessageSize)
                            {
                                HandleMessage(completedMessage, totalMessageSize, myType);
                                ammountToReceive = 4;
                                totalMessageSize = 0;
                                totalReceivedSize = 0;
                            }
                            else
                            {
                                ammountToReceive = totalMessageSize - totalReceivedSize;
                            }
                        }
                    }
                }
            }

            OnClientDisconnected(myType, clientEndPoint.Address.ToString());
            connectedClients[myType].Remove(client);
            Console.WriteLine($"Client ({clientIndex}) disconnected");
        }

        private ClientTypes.Types IdentifyClient(byte[] buffer, TcpClient client)
        {
            //TODO: Handle incorrect types
            var myType = (ClientTypes.Types)BitConverter.ToInt32(buffer, 0);
            connectedClients[ClientTypes.Types.Undefined].Remove(client);
            connectedClients[myType].Add(client);
            return myType;
        }

        protected abstract void HandleMessage(byte[] buffer, int ammountRead, ClientTypes.Types myType);
    }
}
