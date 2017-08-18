using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace AnnelidaDispatcher.Model
{
    public class Sensors
    {
        public double temperature { get; set; }
        public double distance { get; set; }
        public double depth { get; set; }
        public double s0 { get; set; }
        public double s1 { get; set; }
        public double s2 { get; set; }
        public double s3 { get; set; }
        public double s4 { get; set; }
        public double s5 { get; set; }
        public double s6 { get; set; }
        public double s7 { get; set; }
        public double s8 { get; set; }
        public double s9 { get; set; }
        public double s10 { get; set; }
        public double s11 { get; set; }
        public double s12 { get; set; }
        public double s13 { get; set; }
        public double s14 { get; set; }
        public double s15 { get; set; }
        public double s16 { get; set; }

        [BsonRepresentation(BsonType.Document)]
        public DateTime timestamp { get; set; }

        public Sensors()
        {
            timestamp = DateTime.UtcNow;
        }
    }
       
}
