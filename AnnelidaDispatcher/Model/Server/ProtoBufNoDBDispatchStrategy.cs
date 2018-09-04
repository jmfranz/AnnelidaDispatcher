using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDispatcher.Model.Server
{
    public class ProtoBufNoDBDispatchStrategy : NoDBDispatchStrategy
    {
        public override void RedespatchMessage(byte[] message,
            Dictionary<ClientTypes.Types, List<TcpClient>> connectedClients, ClientTypes.Types origin)
        {
            var buffer = new byte[message.Length+4];
            Array.Copy(BitConverter.GetBytes(message.Length),buffer,4);
            Array.Copy(message,0,buffer,4,message.Length);
            base.RedespatchMessage(buffer,connectedClients,origin);
        }

    }
}
