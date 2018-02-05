using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming

#pragma warning disable 1591

namespace AnnelidaDispatcher.Model
{
    /// <summary>
    /// Sensor class that defines all the sensors in the robot
    ///   and how they are stored in the database
    /// Not that this specification is old
    /// </summary>
    public class Sensors
    {
        [BsonRepresentation(BsonType.Document)]
        public DateTime timestamp;
        public BackwardSector backward;
        public CentralSector central;
        public ForwardSector forward;

        public Sensors()
        {
            backward = new BackwardSector();
            central = new CentralSector();
            forward = new ForwardSector();
        }

        public class Enclosure1
        {
            public double traction;
            public double temperature;
            public double pressure;
            public List<double> orientation;

            public Enclosure1()
            {
                orientation = new List<double>(3);
            }
        }
        public class Enclosure2
        {
            public double temperature;
            public double pressure;
            public double inputCurrent;
            public double outputCurrent;
            public double inputVoltage;
            public double outputVoltage;
        }
        public class Enclosure3n7
        {
            public List<double> engineSpeed;
            public List<double> appliedPower;

            public Enclosure3n7()
            {
                engineSpeed = new List<double>();
                appliedPower = new List<double>();
            }
        }
        public class Enclosure4n6
        {
            public double temperature;
            public double pressure;
            public double engineControllerTemperature;
            public double engineControllerCurrent;
        }
        public class Enclosure5
        {
            public double temperature;
            public double pressure;
            public List<double> orientation;
            public double travel;

            public Enclosure5()
            {
                orientation = new List<double>();
            }
        }
        public class Enclosure8
        {
            public double externalTemperature;
            public double externalPressure;
            public bool obstructionSensor;
        }

        public class BackwardSector
        {
            public Enclosure1 enclosure1;
            public Enclosure2 enclosure2;
            public Enclosure3n7 enclosure3;
            
            public BackwardSector()
            {
                enclosure1 = new Enclosure1();
                enclosure2 = new Enclosure2();
                enclosure3 = new Enclosure3n7();
            }
        }

        public class CentralSector
        {
            public Enclosure4n6 enclosure4;
            public Enclosure5 enclosure5;
            public Enclosure4n6 enclosure6;

            public CentralSector()
            {
                enclosure4 = new Enclosure4n6();
                enclosure5 = new Enclosure5();
                enclosure6 = new Enclosure4n6();
            }
        }

        public class ForwardSector
        {
            public Enclosure3n7 enclosure7;
            public Enclosure8 enclosure8;

            public ForwardSector()
            {
                enclosure7 = new Enclosure3n7();
                enclosure8 = new Enclosure8();
            }
        }
    }
    
}


