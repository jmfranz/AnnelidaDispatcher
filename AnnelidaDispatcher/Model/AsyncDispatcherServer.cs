using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace AnnelidaDispatcher.Model
{
    public class AsyncDispatcherServer
    {
        private CancellationTokenSource cts;
        private TcpListener listener;

        private readonly Dictionary<ClientTypes.Types, List<TcpClient>> connectedClients;

        /// <summary>
        /// Delegate method for client connect/disconnect actions
        /// </summary>
        /// <param name="type">The type of the client</param>
        /// <param name="addr">The address of the client</param>
        public delegate void ClientConnectionDelegate(ClientTypes.Types type, string addr);
        /// <summary>
        /// Client connected event
        /// </summary>
        public event ClientConnectionDelegate ClientConnectedEvent;
        /// <summary>
        /// Client disconnected event
        /// </summary>
        public event ClientConnectionDelegate ClientDisconnectedEvent;

        public AsyncDispatcherServer(int tcpPort)
        {
            connectedClients = new Dictionary<ClientTypes.Types, List<TcpClient>>
            {
                {ClientTypes.Types.Undefined, new List<TcpClient>()},
                {ClientTypes.Types.Controller, new List<TcpClient>()},
                {ClientTypes.Types.View, new List<TcpClient>()},
                {ClientTypes.Types.Robot, new List<TcpClient>()}
            };
            cts = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, tcpPort);
            
        }

        public void Start()
        {
            listener.Start();
            //TODO: Add public state property
            AcceptClientAssync(listener, cts.Token);
        }

        private async Task AcceptClientAssync(TcpListener listener, CancellationToken ct)
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
                var stream = client.GetStream();
                int count = 1;
                var ammountToReceive = 4;
                while (!ct.IsCancellationRequested)
                {
                    var timeOutTask = Task.Delay(TimeSpan.FromMinutes(1));
                    var ammountReadTask = stream.ReadAsync(buf, 0, ammountToReceive, ct);
                    var completedTask = await Task.WhenAny(ammountReadTask).ConfigureAwait(false);

                    if (completedTask == timeOutTask)
                    {
                        //TODO: Handle UI
                        break;
                    }

                    var ammountRead = ammountReadTask.Result;
                    if (ammountRead == 0) break;

                    if (myType == ClientTypes.Types.Undefined)
                    {
                        myType = IdentifyClient(buf,client);
                        ClientConnectedEvent?.Invoke(myType, clientEndPoint.Address.ToString());
                    }
                    else
                    {
                        ammountToReceive = HandleMessage(buf,ammountRead,myType);
                    }
                   
                }
            }
           
            ClientDisconnectedEvent?.Invoke(myType, clientEndPoint.Address.ToString());
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

        private int HandleMessage(byte[] buffer, int ammountRead,ClientTypes.Types myType)
        {
            if (ammountRead == 4)
            {
                byte[] size = new byte[] { buffer[0], buffer[1], buffer[2], buffer[3] };
                return BitConverter.ToInt32(size, 0) - 4;
            }

            var message = ProcessSerializedBson(buffer, ammountRead + 4);
            
            RedespatchMessage(message.ToBson(), myType);
            return 4;

        }
        private BsonDocument ProcessSerializedBson(byte[] bytes, int packageSize)
        {
            var workingBuffer = new byte[packageSize];
            var size = BitConverter.GetBytes(packageSize);
            Array.Copy(size, workingBuffer, 4);
            Array.Copy(bytes, 0, workingBuffer, 4, workingBuffer.Length - 4);

            try
            {
                var doc = BsonSerializer.Deserialize<BsonDocument>(workingBuffer);
                BsonDateTime timestamp = DateTime.UtcNow;
                doc["timestamp"] = timestamp;
                return doc;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }

        private void RedespatchMessage(byte[] message, ClientTypes.Types origin)
        {
            //TODO: Change behaviour to state
            try
            {
                switch (origin)
                {
                    case ClientTypes.Types.Undefined:
                        break;
                    case ClientTypes.Types.View:
                        break;
                    case ClientTypes.Types.Controller:
                        break;
                    case ClientTypes.Types.Robot:
                        foreach (var client in connectedClients[ClientTypes.Types.View])
                        {
                            client.GetStream().WriteAsync(message, 0, message.Length);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}