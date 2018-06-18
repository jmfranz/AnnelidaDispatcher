using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace AnnelidaDispatcher.Model.Server
{
    public class BsonServer : AsyncAbstractServer
    {
        public BsonServer(int tcpPort) : base(tcpPort)
        {
            
        }

        protected override void HandleMessage(byte[] buffer, int ammountRead,ClientTypes.Types myType)
        {
           var message = ProcessSerializedBson(buffer, ammountRead);
                       
           messageDispatchStrategy.RedespatchMessage(message.ToBson(), connectedClients, myType);
        }
        private BsonDocument ProcessSerializedBson(byte[] bytes, int packageSize)
        {

            try
            {
                var doc = BsonSerializer.Deserialize<BsonDocument>(bytes);
                BsonDateTime timestamp = DateTime.UtcNow;
                doc["timestamp"] = timestamp;
                return doc;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }

      
    }
}