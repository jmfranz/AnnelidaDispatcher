
namespace AnnelidaDataFormat.Members
{
    public abstract class PumpEngine
    {
        public float[] EngineRpm { get; set; }
        public float[] EnginePower { get; set; }

        protected PumpEngine(int fifoSize)
        {
            EngineRpm = new float[fifoSize];
            EnginePower = new float[fifoSize];
        }
    }
}
