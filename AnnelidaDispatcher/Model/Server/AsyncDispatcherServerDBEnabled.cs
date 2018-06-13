using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders;

namespace AnnelidaDispatcher.Model.Server
{
    public class AsyncDispatcherServerDBEnabled : AsyncDispatcherServer
    {

        /// <inheritdoc />
        public AsyncDispatcherServerDBEnabled(MongoWrapper sensorDb, MongoWrapper controlDb, string missionName, int tcpPort) : base(tcpPort)
        {
           messageDispatchStrategy = new MongoDispatchStrategy(sensorDb,controlDb,missionName,tcpPort);
        }


    }
}