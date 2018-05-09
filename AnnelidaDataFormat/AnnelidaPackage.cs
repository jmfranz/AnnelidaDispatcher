using System;
using AnnelidaDataFormat.Converters;
using AnnelidaDataFormat.Locomotives;
using AnnelidaDataFormat.Manifolds;
using AnnelidaDataFormat.Members;
using AnnelidaDataFormat.PumpsEngines;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnnelidaDataFormat
{
    public class AnnelidaPackage
    {
        [BsonRepresentation(BsonType.Document)]
        public DateTime UtcTimeStamp { get; set; }
        public TimeSpan TimeSinceStart { get; set; }
        public Umbilical Umbilical { get; set; }
        public Converter[] Converters { get; set; }
        public EmbeddedSystem EmbeddedSystem { get; set; }
        public Controller[] Controllers { get; set; }
        public Locomotive[] Locomotives { get; set; }
        public Manifold[] Manifolds { get; set; }
        public PumpEngine[] PumpsEngines { get; set; }
        public SGNValve SgnValve { get; set; }
        public SGNReactor SgnReactor { get; set; }

        public string[] Faults { get; set; }
        



        private const int MaximumFaultsCount = 256;

        public AnnelidaPackage()
        {
            Umbilical = new Umbilical();
            Converters = new Converter[4];
            EmbeddedSystem = new EmbeddedSystem();
            Controllers = new Controller[5];
            Locomotives = new Locomotive[2];
            Manifolds = new Manifold[4];
            PumpsEngines = new PumpEngine[5];

            Converters[0] = new NotRegulatedConverter();
            Converters[1] = new NotRegulatedConverter();
            Converters[2] = new RegulatedConverter();
            Converters[3] = new RegulatedConverter();

            for (int i = 0; i < Controllers.Length; i++)
            {
                Controllers[i] = new Controller();
            }

            Locomotives[0] = new ForwardLocomotive();
            Locomotives[1] = new BackwardLocomotive();

            Manifolds[0] = new ManifoldType1();
            Manifolds[1] = new ManifoldType2();
            Manifolds[2] = new ManifoldType1();
            Manifolds[3] = new ManifoldType2();

            PumpsEngines[0] = new PumpEngWithRes();
            PumpsEngines[1] = new PumpEngNoRes();
            PumpsEngines[2] = new PumpEngNoRes();
            PumpsEngines[3] = new PumpEngNoRes();
            PumpsEngines[4] = new PumpEngWithRes();

            Faults = new string[MaximumFaultsCount];

        }
    }
}
