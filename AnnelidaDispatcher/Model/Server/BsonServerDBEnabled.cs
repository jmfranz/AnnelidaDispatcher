using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders;

namespace AnnelidaDispatcher.Model.Server
{
    public class BsonServerDBEnabled : BsonServer
    {


        /// <inheritdoc />
        public BsonServerDBEnabled(string missionName, int tcpPort) : base(tcpPort)
        {
            var sensorDb = new MongoWrapper("sensors");
            var controlDb = new MongoWrapper("control");
            messageDispatchStrategy = new MongoDispatchStrategy(sensorDb,controlDb,missionName,tcpPort);
        }


    }
}