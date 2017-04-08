using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;


using MongoDB.Bson;


namespace AnnelidaDispatcher.Model
{

    /// <summary>
    /// Dispatcher model class. Handles accepting client connections,
    /// classifies them and handle message propagation.
    /// </summary>
    
    
    public class DispatcherServer
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static readonly int bufferSize = 1024;
        //List of all the connected clients
        private Dictionary<ClientTypes.Types, List<Socket>> connectedClients;

        public delegate void ClientConnectionDelegate(ClientTypes.Types type, string addr);
        public event ClientConnectionDelegate clientConnectedEvent;
        public event ClientConnectionDelegate clientDisconnectedEvent;

        private MongoWrapper sensorDB, controlDB;
        private string missionName;

        /// <summary>
        /// Class constructor. Initialize the client lists
        /// </summary>
        public DispatcherServer(MongoWrapper sensorDB, MongoWrapper controlDB, string missionName)
        {
            connectedClients = new Dictionary<ClientTypes.Types, List<Socket>>();
            connectedClients.Add(ClientTypes.Types.Undefined, new List<Socket>());
            connectedClients.Add(ClientTypes.Types.Controller, new List<Socket>());
            connectedClients.Add(ClientTypes.Types.View, new List<Socket>());
            connectedClients.Add(ClientTypes.Types.Robot, new List<Socket>());

            this.sensorDB = sensorDB;
            this.controlDB = controlDB;
            this.missionName = missionName;
        }

        /// <summary>
        /// Starts listening for incomming connections
        /// </summary>
        /// <param name="port">The port to listen for connections</param>
        public void Start(int port)
        {
            //Data buffer
            byte[] buffer = new byte[bufferSize];

            //Set the local end point
            IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            //Create the listener socket, TCP because we need delivery guarantee
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Set network layer
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                //Manages connections, will the while break the UI thread I ask...
                while(true)
                {
                    allDone.Reset();
                    Console.WriteLine("Wating for connections");
                    listener.BeginAccept(new AsyncCallback(AcceptHandler), listener);
                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
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
            allDone.Set();

            Socket listener = (Socket)result.AsyncState;
            Socket client = listener.EndAccept(result);

            //When a client connects we do not know what type he is yet
            connectedClients[ClientTypes.Types.Undefined].Add(client);

            var so = new DispatcherClientObject(bufferSize);
            so.workSocket = client;
            //Starts receving messages
            client.BeginReceive(so.buffer, 0, so.bufferSize, 0, new AsyncCallback(ReadHandler), so);
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
            //TOOD: handle disonnection messages (SHUTODOWNMODES)
            catch(SocketException e)
            {
                Console.WriteLine($"Socket error {e.ToString()}");
                //probably our client disconnected
                connectedClients[state.myType].Remove(state.workSocket);
                var clientAddr = state.workSocket.RemoteEndPoint as IPEndPoint;
                clientDisconnectedEvent?.Invoke(state.myType, clientAddr.Address.ToString());
                state.workSocket.Close();
                state = null;
            }

            //if our client has not identified itself the first
            //message he sends is his ID
            if (bytesRead > 0 && !state.isInitialized)
            {

                //We are expecting and int representing the client type
                int type = BitConverter.ToInt32(state.buffer,0);
                state.isInitialized = true;
                state.myType = (ClientTypes.Types)type;
                var clientAddr = state.workSocket.RemoteEndPoint as IPEndPoint;
                connectedClients[ClientTypes.Types.Undefined].Remove(state.workSocket);
                connectedClients[(ClientTypes.Types)type].Add(state.workSocket);
                
                //Raise and event so the UI can update the client list
                clientConnectedEvent?.Invoke((ClientTypes.Types)type, clientAddr.Address.ToString());
                //Continue handling messages
                handler.BeginReceive(state.buffer, 0, state.bufferSize, 0,
                    new AsyncCallback(ReadHandler), state);
                //sets the buffer to 0 because the next message contains the size
                state.bufferSize = 0;
            }
            else if (bytesRead > 0 && state.isInitialized)
            {
                //We are receiving the size of the package which is an int32
                if(state.bufferSize == 0)
                {
                    int size = BitConverter.ToInt32(state.buffer, 0);
                    state.bufferSize = size;
                    state.buffer = new byte[size];
                    state.recvBytesCount = 0;
                    handler.BeginReceive(state.buffer, 0, state.bufferSize, 0,
                        new AsyncCallback(ReadHandler), state);
                }
                //we already know the package size
                else
                {
                    state.recvBytesCount += bytesRead;
                    if (state.recvBytesCount < state.bufferSize)
                        handler.BeginReceive(state.buffer, state.recvBytesCount - 1, state.bufferSize, 0,
                       new AsyncCallback(ReadHandler), state);
                    else
                    {
                        HandleMessage(state.buffer, state);
                        //Prep to receive another package
                        state.bufferSize = 0;
                        //int32 with the size
                        state.buffer = new byte[4];
                        handler.BeginReceive(state.buffer, 0, 4, 0,
                            new AsyncCallback(ReadHandler), state);
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
            Console.WriteLine($"Read {bytes.Length} bytes from socket.");
            switch(state.myType)
            {
                case ClientTypes.Types.Controller:
                    //Save to control DB
                    //Do nothing else to fake movement for now
                    break;
                case ClientTypes.Types.Robot:
                    //Save to sensor DB async
                    Task write = sensorDB.WriteSingleToCollection(bytes, missionName); 
                    //Notify all views that the DB was updated inside async method
                    NotifyNetworkViewListeners(state.myType, bytes);
                    break;
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
                    foreach (var c in connectedClients[ClientTypes.Types.View])
                    {
                        c.Send(BitConverter.GetBytes(document.Length), 4, 0);
                        c.BeginSend(document, 0, document.Length, 0, null, c);
                    }
                    break;
                //Notify the roboy
                case ClientTypes.Types.Controller:
                    foreach (var c in connectedClients[ClientTypes.Types.Robot])
                    {
                        c.Send(BitConverter.GetBytes(document.Length), 4, 0);
                        c.BeginSend(document, 0, document.Length, 0, null, c);
                    }
                    break;
            }      

        }
        public static string GetLocalIPAddress()
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
    }
}
