
using AnnelidaDataFormat.Members;

namespace AnnelidaDataFormat.PumpsEngines
{
    public class PumpEngWithRes : PumpEngine
    {
        public float[] ReservoirOilTemp { get; set; }

        public PumpEngWithRes(int fifoSize) : base(fifoSize)
        {
            ReservoirOilTemp = new float[fifoSize];
        }
    }
}
