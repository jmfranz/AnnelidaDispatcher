using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;


using MongoDB.Bson;
using MongoDB.Bson.Serialization;


namespace AnnelidaDispatcher.Model
{

    /// <summary>
    /// Dispatcher model class. Handles accepting client connections,
    /// classifies them and handle message propagation.
    /// </summary>
    
    
    public class DispatcherServer
    {
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);
        private const int BufferSize = 1024;

        //List of all the connected clients
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

        private readonly MongoWrapper sensorDb;
        private readonly MongoWrapper controlDb;
        private readonly string missionName;
        private Record record;

        /// <summary>
        /// Class constructor. Initialize the client lists
        /// </summary>
        public DispatcherServer(MongoWrapper sensorDb, MongoWrapper controlDb, string missionName)
        {
            connectedClients = new Dictionary<ClientTypes.Types, List<Socket>>
            {
                {ClientTypes.Types.Undefined, new List<Socket>()},
                {ClientTypes.Types.Controller, new List<Socket>()},
                {ClientTypes.Types.View, new List<Socket>()},
                {ClientTypes.Types.Robot, new List<Socket>()}
            };

            this.sensorDb = sensorDb;
            this.controlDb = controlDb;
            this.missionName = missionName;

            record = new Record();
        }

        /// <summary>
        /// Starts listening for incomming connections
        /// </summary>
        /// <param name="port">The port to listen for connections</param>
        public void Start(int port)
        {
            record.timestamp = DateTime.UtcNow;

            //Set the local end point
            var localEndPoint = new IPEndPoint(IPAddress.Any, port);

            //Create the listener socket, TCP because we need delivery guarantee
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Set network layer
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                //Manages connections, will the while break the UI thread I ask...
                while(true)
                {
                    AllDone.Reset();
                    Console.WriteLine(Strings.DispatcherServer_WaitingForConnections);
                    listener.BeginAccept(AcceptHandler, listener);
                    // Wait until a connection is made before continuing.
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Handle accepting each individual client connection
        /// outside of the main thread
        /// </summary>
        /// <param name="result"></param>
        public  void AcceptHandler(IAsyncResult result)
        {
            AllDone.Set();

            Socket listener = (Socket)result.AsyncState;
            Socket client = listener.EndAccept(result);

            //When a client connects we do not know what type he is yet
            connectedClients[ClientTypes.Types.Undefined].Add(client);

            var so = new DispatcherClientObject(BufferSize) {workSocket = client};
            //Starts receving messages
            client.BeginReceive(so.buffer, 0, so.bufferSize, 0, ReadHandler, so);
        }

        /// <summary>
        /// Message handler for each individual client outside of the main thread
        /// </summary>
        /// <param name="ar"></param>
        public void ReadHandler(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            DispatcherClientObject state = (DispatcherClientObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            //TODO: handle disonnection messages (SHUTODOWNMODES)
            catch(SocketException e)
            {
                // ReSharper disable once LocalizableElement
                Console.WriteLine($"Socket error {e.ToString()}");
                //probably our client disconnected
                connectedClients[state.myType].Remove(state.workSocket);
                var clientAddr = state.workSocket.RemoteEndPoint as IPEndPoint;
                if (clientAddr != null)
                    ClientDisconnectedEvent?.Invoke(state.myType, clientAddr.Address.ToString());
                else
                    throw new ArgumentNullException();
                state.workSocket.Close();
                state = null;
            }

            //if our client has not identified itself the first
            //message he sends is his ID
            if (state != null && (bytesRead > 0 && !state.isInitialized))
            {

                //We are expecting and int representing the client type
                var type = BitConverter.ToInt32(state.buffer,0);
                state.isInitialized = true;
                state.myType = (ClientTypes.Types)type;
                var clientAddr = state.workSocket.RemoteEndPoint as IPEndPoint;
                connectedClients[ClientTypes.Types.Undefined].Remove(state.workSocket);
                connectedClients[(ClientTypes.Types)type].Add(state.workSocket);
                
                //Raise and event so the UI can update the client list
                if (clientAddr != null)
                    ClientConnectedEvent?.Invoke((ClientTypes.Types) type, clientAddr.Address.ToString());
                else
                    throw new ArgumentNullException();
                //Continue handling messages
                handler.BeginReceive(state.buffer, 0, state.bufferSize, 0,
                    ReadHandler, state);
                //sets the buffer to 0 because the next message contains the size
                state.bufferSize = 0;
            }
            else if (state != null && (bytesRead > 0 && state.isInitialized))
            {
                //We are receiving the package but don't know the size yet
                //Serialized bson contains the size in the first 4 bytes.
                if(state.bufferSize == 0)
                {
                    int size = BitConverter.ToInt32(state.buffer, 0);
                    //We take 4 out because we already red those bytes
                    state.bufferSize = size - 4;
                    //Resize the array because we need the full set of
                    //bytes in order to deserialize the Bson
                    Array.Resize(ref state.buffer, size);
                    state.recvBytesCount = 0;
                    handler.BeginReceive(state.buffer, 4, state.bufferSize , 0,
                        ReadHandler, state);
                }
                //we already know the package size
                else
                {
                    state.recvBytesCount += bytesRead;
                    if (state.recvBytesCount < state.bufferSize)
                        handler.BeginReceive(state.buffer, 4 + state.recvBytesCount - 1, state.bufferSize, 0,
                       ReadHandler, state);
                    else
                    {
                        HandleMessage(state.buffer, state);
                        //Prep to receive another package
                        state.bufferSize = 0;
                        //int32 with the size
                        state.buffer = new byte[4];
                        handler.BeginReceive(state.buffer, 0, 4, 0,
                            ReadHandler, state);
                    }
                 }
            }
        }

        /// <summary>
        /// After we have then entire message defragmented
        /// this method sorts the message to according to the source.
        /// Handles saving the message to the DB and dispatching to others
        /// clients asynchronouslly 
        /// </summary>
        /// <param name="bytes">The message bytes</param>
        /// <param name="state">The client who sent the message originally</param>
        public void HandleMessage(byte[] bytes, DispatcherClientObject state)
        {
            switch (state.myType)
            {
                case ClientTypes.Types.Controller:
                    //Save to control DB
                    controlDb?.WriteSingleToCollection(bytes, missionName);
                    //Notify
                    NotifyNetworkViewListeners(state.myType, bytes);
                    break;
                case ClientTypes.Types.Robot:
                    //Save to sensor DB async
                    var deserializedDocument = ProcessSerializedBson(bytes);
                    var t = deserializedDocument["timestamp"].ToUniversalTime();

                    //Send data to DB in batches of 1s
                    if( (t - record.timestamp).TotalSeconds > 1)
                    {
                        sensorDb.WriteSingleToCollection(record, missionName);
                        record = new Record {timestamp = t};
                        record.sensors.Add(deserializedDocument);                        
                    }
                    else
                    {
                        record.sensors.Add(deserializedDocument);
                    }

                    //Notify all views that the DB was updated inside async method
                    NotifyNetworkViewListeners(state.myType, bytes);
                    break;
                case ClientTypes.Types.Undefined:
                    throw  new InvalidOperationException();
                case ClientTypes.Types.View:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Network dispatcher method
        /// </summary>
        /// <param name="sender">The original client type</param>
        /// <param name="document">The byte array containing the list</param>
        private void NotifyNetworkViewListeners(ClientTypes.Types sender, byte[] document)
        {
            switch(sender)
            {
                //Notifiy the view clients
                case ClientTypes.Types.Robot:
                    try
                    {
                        foreach (var c in connectedClients[ClientTypes.Types.View])
                        {

                            c.BeginSend(document, 0, document.Length, 0, null, c);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    break;
                //Notify the robot
                case ClientTypes.Types.Controller:
                    foreach (var c in connectedClients[ClientTypes.Types.Robot])
                    {
                        c.BeginSend(document, 0, document.Length, 0, null, c);
                    }
                    break;
                    
            }      

        }
        // ReSharper disable once UnusedMember.Local
        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private static BsonDocument ProcessSerializedBson(byte[] bytes)
        {
            var doc = BsonSerializer.Deserialize<BsonDocument>(bytes);
            BsonDateTime timestamp = DateTime.UtcNow;
            doc["timestamp"] = timestamp;
            return doc;
        }

    }
}
