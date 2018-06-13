using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDispatcher.Model
{
    public abstract class AbstractServer
    {

        protected readonly Dictionary<ClientTypes.Types, List<TcpClient>> connectedClients;
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

        protected AbstractServer()
        {
            connectedClients = new Dictionary<ClientTypes.Types, List<TcpClient>>
            {
                {ClientTypes.Types.Undefined, new List<TcpClient>()},
                {ClientTypes.Types.Controller, new List<TcpClient>()},
                {ClientTypes.Types.View, new List<TcpClient>()},
                {ClientTypes.Types.Robot, new List<TcpClient>()}
            };
        }

        protected virtual void OnClientConnected(ClientTypes.Types type, string addr)
        {
            ClientConnectedEvent?.Invoke(type,addr);
        }

        protected virtual void OnClientDisconnected(ClientTypes.Types type, string addr)
        {
            ClientDisconnectedEvent?.Invoke(type, addr);
        }

        public abstract void Start();

    }
}
