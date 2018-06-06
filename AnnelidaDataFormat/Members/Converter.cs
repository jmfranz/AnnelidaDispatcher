namespace AnnelidaDataFormat.Members
{
    public abstract class Converter : BaseSensoriedModule
    {
        
        public float[] ConverterElectricalCurrent { get; set; }
        public float[] ConverterElectricalPower { get; set; }

        protected Converter(int fifoSize) : base(fifoSize)
        {
            ConverterElectricalCurrent = new float[fifoSize];
            ConverterElectricalPower = new float[fifoSize];
        }

    }
}
