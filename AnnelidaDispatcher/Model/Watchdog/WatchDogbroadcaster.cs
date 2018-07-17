using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDispatcher.Model.Watchdog
{
    public class WatchDogBroadcaster
    {
        private int port;
        private byte[] TokenBytes;
        private readonly UdpClient udpClient;
        private readonly IPEndPoint endPoint;

        public WatchDogBroadcaster(int port, string token)
        {
            endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            udpClient = new UdpClient
            {
                EnableBroadcast = true
            };

            TokenBytes = new Guid(token).ToByteArray();
        }

        public async Task SendWatchdogToken()
        {
            await udpClient.SendAsync(TokenBytes, 16, endPoint);
        }
    }
}
