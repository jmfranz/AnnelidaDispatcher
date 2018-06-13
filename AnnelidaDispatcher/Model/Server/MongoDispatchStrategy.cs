using System.Collections.Generic;
using System.Net.Sockets;

namespace AnnelidaDispatcher.Model.Server
{
    public class MongoDispatchStrategy : NoDBDispatchStrategy, IMessageDispatcherStrategy
    {
        private readonly MongoWrapper sensorDb;
        private readonly MongoWrapper controlDb;
        private readonly string missionName;
        private readonly Dictionary<ClientTypes.Types, List<byte[]>> sensorMessages;

        private readonly int messagesQueueSize = 20;

        public MongoDispatchStrategy(MongoWrapper sensorDb, MongoWrapper controlDb, string missionName, int tcpPort) : base()
        {
            this.sensorDb = sensorDb;
            this.controlDb = controlDb;
            this.missionName = missionName;
            sensorMessages = new Dictionary<ClientTypes.Types, List<byte[]>>
            {
                {ClientTypes.Types.Robot, new List<byte[]>()},
                {ClientTypes.Types.Controller, new List<byte[]>()}
            };
        }

        public void RedespatchMessage(byte[] message, Dictionary<ClientTypes.Types, List<TcpClient>> connectedClients,
            ClientTypes.Types origin)
        {
            base.RedespatchMessage(message,connectedClients,origin);
            sensorMessages[origin].Add(message);

            if (sensorMessages[origin].Count <= messagesQueueSize)
                return;

            sensorDb.WriteManyToCollection(sensorMessages[origin], missionName);
            sensorMessages[origin].Clear();

        }
    }
}