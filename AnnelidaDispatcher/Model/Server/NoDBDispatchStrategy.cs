using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDispatcher.Model.Server
{
    public class NoDBDispatchStrategy: IMessageDispatcherStrategy
    {
        public void RedespatchMessage(byte[] message, Dictionary<ClientTypes.Types, List<TcpClient>> connectedClients, ClientTypes.Types origin)
        {
            //TODO: Change behaviour to state, maybe?
            try
            {
                switch (origin)
                {
                    case ClientTypes.Types.Undefined:
                        break;
                    case ClientTypes.Types.View:
                        break;
                    case ClientTypes.Types.Controller:
                        foreach (var client in connectedClients[ClientTypes.Types.Robot])
                        {
                            client.GetStream().WriteAsync(message, 0, message.Length);
                        }
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
