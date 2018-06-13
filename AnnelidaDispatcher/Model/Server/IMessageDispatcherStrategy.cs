using System.Collections.Generic;
using System.Net.Sockets;

namespace AnnelidaDispatcher.Model.Server
{
    public interface IMessageDispatcherStrategy
    {
        void RedespatchMessage(byte[] message, Dictionary<ClientTypes.Types, List<TcpClient>> connectedClients, ClientTypes.Types origin);
    }
}