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
    public class DispatcherServer
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static readonly int bufferSize = 1024;
        //List of all the connected clients
        private Dictionary<ClientTypes.Types, List<Socket>> connectedClients;

        public delegate void ClientConnectionDelegate(ClientTypes.Types type, string addr);
        public event ClientConnectionDelegate clientConnectedEvent;
        public event ClientConnectionDelegate clientDisconnectedEvent;

        public DispatcherServer()
        {
            connectedClients = new Dictionary<ClientTypes.Types, List<Socket>>();

            connectedClients.Add(ClientTypes.Types.Undefined, new List<Socket>());
            connectedClients.Add(ClientTypes.Types.Controller, new List<Socket>());
            connectedClients.Add(ClientTypes.Types.View, new List<Socket>());
            connectedClients.Add(ClientTypes.Types.Robot, new List<Socket>());
        }

        public void Start(int port)
        {
            //Data buffer
            byte[] buffer = new byte[bufferSize];

            //Set the local end point
            IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            //Create the listener socket, TCP becase we need delivery guarantee
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Set network layer
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                //Manages connections, will the break the UI thread I ask...
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

        public  void AcceptHandler(IAsyncResult result)
        {
            allDone.Set();

            //object[] state = (object[])result.AsyncState;

            Socket listener = (Socket)result.AsyncState;
            Socket client = listener.EndAccept(result);

            //var connectedClients = (Dictionary<ClientTypes.Types, List<Socket>>)state[1];
            connectedClients[ClientTypes.Types.Undefined].Add(client);

            var so = new ClientState(bufferSize);
            so.workSocket = client;
            client.BeginReceive(so.buffer, 0, so.bufferSize, 0, new AsyncCallback(ReadHandler), so);
        }

        public void ReadHandler(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            ClientState state = (ClientState)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
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

            if (bytesRead > 0 && !state.isInitialized)
            {

                //We are expecting and int representing the client type

                int type = BitConverter.ToInt32(state.buffer,0);
                state.isInitialized = true;
                state.myType = (ClientTypes.Types)type;
                var clientAddr = state.workSocket.RemoteEndPoint as IPEndPoint;
                connectedClients[ClientTypes.Types.Undefined].Remove(state.workSocket);
                connectedClients[(ClientTypes.Types)type].Add(state.workSocket);
                clientConnectedEvent?.Invoke((ClientTypes.Types)type, clientAddr.Address.ToString());
                handler.BeginReceive(state.buffer, 0, state.bufferSize, 0,
                    new AsyncCallback(ReadHandler), state);
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


                        //int acqSize = 1024;
                        //if (size - recvBytes < 1024)
                        //    acqSize = size - recvBytes;
                        //recvBytes += stream.Read(anchor, recvBytes, acqSize);
                    }
            }
        }

        public void HandleMessage(byte[] bytes, ClientState state)
        {
            Console.WriteLine("Read {0} bytes from socket.",
            bytes.Length);
            var document = new RawBsonDocument(bytes);
            switch(state.myType)
            {
                case ClientTypes.Types.Controller:
                    //Save to control DB
                    //Do mething else to fake movement for now
                    break;
                case ClientTypes.Types.Robot:
                    //Save to sensor DB async
                    //Notify all views that the DB was updated inside async method
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
