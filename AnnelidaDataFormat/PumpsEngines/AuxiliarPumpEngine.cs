using AnnelidaDataFormat.Members;

namespace AnnelidaDataFormat.PumpsEngines
{
    public class AuxiliarPumpEngine : PumpEngine
    {
        public float[] Temperature { get; set; }
        public AuxiliarPumpEngine(int fifoSize) : base(fifoSize)
        {
            Temperature = new float[fifoSize];
        }
    }
}