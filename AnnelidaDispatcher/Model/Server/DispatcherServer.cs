﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
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

        private static Semaphore semaphore;

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

        private static uint messageCount = 0;
        private bool isDoingStuff = false;
        private Stopwatch sp;
        

        /// <summary>
        /// Class constructor. Initialize the client lists
        /// </summary>
        public DispatcherServer(MongoWrapper sensorDb, MongoWrapper controlDb, string missionName)
        {
            semaphore = new Semaphore(1,1);

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
            sp = Stopwatch.StartNew();
        }

        /// <summary>
        /// Starts listening for incomming connections
        /// </summary>
        /// <param name="port">The port to listen for connections</param>
        public void Start(int port)
        {
            record.Timestamp = DateTime.UtcNow;

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

            var so = new DispatcherClientObject() {WorkSocket = client};
            //Starts receving messages
            client.BeginReceive(so.Buffer, 0, 4, 0, ReadHandler, so);
        }

        /// <summary>
        /// Message handler for each individual client outside of the main thread
        /// </summary>
        /// <param name="ar"></param>
        public void ReadHandler(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (DispatcherClientObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            // Read data from the client socket. 
            try
            {
                semaphore.WaitOne();
                state.RecvBytesCount += handler.EndReceive(ar);
            }
            //TODO: handle disonnection messages (SHUTODOWNMODES)
            catch(SocketException e)
            {
                // ReSharper disable once LocalizableElement
                //Console.WriteLine($"Socket error {e}");
                //probably our client disconnected
                connectedClients[state.MyType].Remove(state.WorkSocket);
                var clientAddr = state.WorkSocket.RemoteEndPoint as IPEndPoint;
                if (clientAddr != null)
                    ClientDisconnectedEvent?.Invoke(state.MyType, clientAddr.Address.ToString());
                else
                    throw new ArgumentNullException();
                state.WorkSocket.Close();
                state = null;
            }

            if(state?.RecvBytesCount <=0)
                return;

            //if our client has not identified itself the first
            //message he sends is his ID
            if (state != null  && !state.IsInitialized)
            {

                //We are expecting and int representing the client type
                var type = BitConverter.ToInt32(state.Buffer,0);
                state.IsInitialized = true;
                state.MyType = (ClientTypes.Types)type;
                var clientAddr = state.WorkSocket.RemoteEndPoint as IPEndPoint;
                connectedClients[ClientTypes.Types.Undefined].Remove(state.WorkSocket);
                connectedClients[(ClientTypes.Types)type].Add(state.WorkSocket);
                
                //Raise and event so the UI can update the client list
                if (clientAddr != null)
                    ClientConnectedEvent?.Invoke((ClientTypes.Types) type, clientAddr.Address.ToString());
                else
                    throw new ArgumentNullException();

                state.RecvBytesCount = 0;

                semaphore.Release();
                //Continue handling messages
                handler.BeginReceive(state.Buffer, 0, 4, 0,
                    ReadHandler, state);
            }
            else if (state != null && state.IsInitialized)
            {
                //We are receiving the package but don't know the size yet
                //This -if- is here just to be safe...
                if (state.RecvBytesCount < 4)
                {
                    handler.BeginReceive(state.Buffer, state.RecvBytesCount-1, 4 - state.RecvBytesCount, 0, ReadHandler,
                        state);
                }
                //Serialized bson contains the size in the first 4 bytes.
                if (state.RecvBytesCount == 4)
                {
                    state.TotalPackageSize = BitConverter.ToInt32(state.Buffer, 0);

                    state.ResetBuffer(state.TotalPackageSize);
                    var length  = BitConverter.GetBytes(state.TotalPackageSize);
                    state.Buffer[0] = length[0];
                    state.Buffer[1] = length[1];
                    state.Buffer[2] = length[2];
                    state.Buffer[3] = length[3];
                    //Keep reading...
                    semaphore.Release();
                    handler.BeginReceive(state.Buffer, 4, state.TotalPackageSize - 4, 0, ReadHandler,
                        state);
                }

               //we already know the package size
                else
                {
                    
                        

                    //Not done yet
                    if (state.RecvBytesCount < state.TotalPackageSize - 4)
                    {
                        handler.BeginReceive(state.Buffer, state.RecvBytesCount - 1,
                            state.TotalPackageSize - state.RecvBytesCount, 0, ReadHandler, state);
                        semaphore.Release();
                    }
                    else
                    {
                        PrintMD5(state.Buffer);
                        byte[] workingBuffer = new byte[state.Buffer.Length];
                        //TODO: Make the working buffer the size of the received package
                        Array.Copy(state.Buffer,workingBuffer,state.Buffer.Length);
                        HandleMessage(workingBuffer, state);
                        //Prep to receive another package
                        state.ResetBuffer();
                        state.RecvBytesCount = 0;
                        semaphore.Release();
                        handler.BeginReceive(state.Buffer, 0, 4, 0,
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
            switch (state.MyType)
            {
                case ClientTypes.Types.Controller:
                    //Save to control DB
                    controlDb?.WriteSingleToCollection(bytes, missionName);
                    //Notify
                    NotifyNetworkViewListeners(state.MyType, bytes);
                    break;
                case ClientTypes.Types.Robot:
                    //Save to sensor DB async
                    var deserializedDocument = ProcessSerializedBson(bytes);
                    if(deserializedDocument == null)
                        return;
                    var t = deserializedDocument["timestamp"].ToUniversalTime();

                    //Send data to DB in batches of 1s
                    if( (t - record.Timestamp).TotalSeconds > 1)
                    {
                        //Keep te entire logic the same but don't store
                        //sensorDb.WriteSingleToCollection(record, missionName);
                        record = new Record {Timestamp = t};
                        record.Sensors.Add(deserializedDocument);                        
                    }
                    else
                    {
                        record.Sensors.Add(deserializedDocument);
                    }

                    //Notify all views that the DB was updated inside async method
                    NotifyNetworkViewListeners(state.MyType, bytes);
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


        private  BsonDocument ProcessSerializedBson(byte[] bytes)
        {
            try
            {
                var doc = BsonSerializer.Deserialize<BsonDocument>(bytes);
                BsonDateTime timestamp = DateTime.UtcNow;
                doc["timestamp"] = timestamp;
                messageCount++;
                return doc;
            }
            catch (Exception e)
            {
                 Console.WriteLine(e.ToString());
                
                return null;
            }           
        }

        private void PrintMD5(byte[] bytes)
        {
            MD5 md5 = MD5.Create();

            var hash = md5.ComputeHash(bytes);
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < hash.Length; i++)
            {
                sBuilder.Append(hash[i].ToString("x2"));
            }
            Console.WriteLine($"Messsage Number: {messageCount}, Checksum: {sBuilder.ToString()} Length: {bytes.Length}");
        }

    }
}
