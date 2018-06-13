using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AnnelidaDispatcher.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace AnnelidaDispatcher
{
    public class AsyncDispatcherServer
    {
        private CancellationTokenSource cts;
        private TcpListener listener;

        private readonly Dictionary<ClientTypes.Types, List<Socket>> connectedClients;

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
            connectedClients = new Dictionary<ClientTypes.Types, List<Socket>>
            {
                {ClientTypes.Types.Undefined, new List<Socket>()},
                {ClientTypes.Types.Controller, new List<Socket>()},
                {ClientTypes.Types.View, new List<Socket>()},
                {ClientTypes.Types.Robot, new List<Socket>()}
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
                connectedClients[ClientTypes.Types.Undefined].Add(client.Client);
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
                        myType = IdentifyClient(buf,client.Client);
                        ClientConnectedEvent?.Invoke(myType, clientEndPoint.Address.ToString());
                    }
                    else
                    {
                        ammountToReceive = HandleMessage(buf);
                    }
                   
                }
            }
           
            ClientDisconnectedEvent?.Invoke(myType, clientEndPoint.Address.ToString());
            connectedClients[myType].Remove(client.Client);
            Console.WriteLine($"Client ({clientIndex}) disconnected");
        }

        private ClientTypes.Types IdentifyClient(byte[] buffer, Socket socket)
        {
            //TODO: Handle incorrect types
            var myType = (ClientTypes.Types)BitConverter.ToInt32(buffer, 0);
            connectedClients[ClientTypes.Types.Undefined].Remove(socket);
            connectedClients[myType].Add(socket);
            return myType;
        }

        private int HandleMessage(byte[] buffer)
        {
            if (buffer.Length == 4)
            {
                byte[] size = new byte[] { buffer[0], buffer[1], buffer[2], buffer[3] };
                return BitConverter.ToInt32(size, 0) - 4;
            }
            Console.WriteLine($"{ProcessSerializedBson(buffer).ToString()}");
            return 4;

        }
        private BsonDocument ProcessSerializedBson(byte[] bytes)
        {
            try
            {
                var doc = BsonSerializer.Deserialize<BsonDocument>(bytes);
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
    }
}