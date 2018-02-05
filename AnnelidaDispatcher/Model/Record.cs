using System;
using System.Collections.Generic;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

//#pragma warning disable 1591

namespace AnnelidaDispatcher.Model
{
    /// <summary>
    /// Record class represents and entry on the database
    /// The record can contain one or more sensors document
    /// </summary>
    public class Record
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        public ObjectId Id { get; set; }
        /// <summary>
        /// Time on which the record was created
        /// </summary>
        [BsonRepresentation(BsonType.Document)]
        public DateTime Timestamp { get; set; }
        //public List<Sensors> sensors;
        /// <summary>
        /// List containt the sensor documents on that specific time (range)
        /// </summary>
        public List<BsonDocument> Sensors;


        /// <summary>
        /// Class constructor that sets the timestamp of the record
        /// </summary>
        public Record()
        {
            Timestamp = DateTime.UtcNow;
            Sensors = new List<BsonDocument>();
        }

    }

}
