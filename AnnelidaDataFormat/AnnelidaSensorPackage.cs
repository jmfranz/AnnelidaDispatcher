using System;
using AnnelidaDataFormat.Converters;
using AnnelidaDataFormat.Locomotives;
using AnnelidaDataFormat.Members;
using AnnelidaDataFormat.PumpsEngines;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnnelidaDataFormat
{
    public class AnnelidaSensorPackage
    {
        [BsonRepresentation(BsonType.Document)]
        public DateTime UtcTimeStamp { get; set; }
        public DateTime StartTime { get; set; }
        public Umbilical Umbilical { get; set; }
        public Converter[] Converters { get; set; }
        public EmbeddedSystem EmbeddedSystem { get; set; }
        public Controller[] Controllers { get; set; }
        public Locomotive[] Locomotives { get; set; }
        public Manifolds Manifolds { get; set; }
        public PumpEngine[] PumpsEngines { get; set; }
        public SGNValve SgnValve { get; set; }
        public SGNReactor SgnReactor { get; set; }

        public string[] Faults { get; set; }
        



        private const int MaximumFaultsCount = 256;
        private const int FifoSize = 10;

        public AnnelidaSensorPackage()
        {
            Umbilical = new Umbilical(FifoSize);
            Converters = new Converter[13];
            EmbeddedSystem = new EmbeddedSystem(FifoSize);
            Controllers = new Controller[5];
            Locomotives = new Locomotive[2];
            Manifolds = new Manifolds(FifoSize);
            PumpsEngines = new PumpEngine[5];

            Converters[0] = new NotRegulatedConverter(FifoSize);
            Converters[1] = new NotRegulatedConverter(FifoSize);
            Converters[2] = new NotRegulatedConverter(FifoSize);
            Converters[3] = new NotRegulatedConverter(FifoSize);
            Converters[4] = new NotRegulatedConverter(FifoSize);
            
            Converters[5] = new RegulatedConverter(FifoSize);
            Converters[6] = new RegulatedConverter(FifoSize);
            Converters[7] = new RegulatedConverter(FifoSize);
            Converters[8] = new RegulatedConverter(FifoSize);
            Converters[9] = new RegulatedConverter(FifoSize);
            Converters[10] = new RegulatedConverter(FifoSize);
            Converters[11] = new RegulatedConverter(FifoSize);
            Converters[12] = new RegulatedConverter(FifoSize);

            for (int i = 0; i < Controllers.Length; i++)
            {
                Controllers[i] = new Controller(FifoSize);
            }

            Locomotives[0] = new ForwardLocomotive(FifoSize);
            Locomotives[1] = new BackwardLocomotive(FifoSize);


            PumpsEngines[0] = new PumpEngWithRes(FifoSize);
            PumpsEngines[1] = new PumpEngNoRes(FifoSize);
            PumpsEngines[2] = new AuxiliarPumpEngine(FifoSize);
            PumpsEngines[3] = new PumpEngNoRes(FifoSize);
            PumpsEngines[4] = new PumpEngWithRes(FifoSize);

            Faults = new string[MaximumFaultsCount];

        }
    }
}
