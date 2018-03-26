using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

#pragma warning disable 1591

namespace AnnelidaDispatcher.Model
{
    public class Record
    {
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.Document)]
        public DateTime timestamp { get; set; }
        //public List<Sensors> sensors;
        public List<BsonDocument> sensors;


        public Record()
        {
            timestamp = DateTime.UtcNow;
            sensors = new List<BsonDocument>();
        }

    }

}
