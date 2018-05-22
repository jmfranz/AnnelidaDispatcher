using AnnelidaDataFormat.Members;

namespace AnnelidaDataFormat.Converters
{
    public class NotRegulatedConverter : Converter
    {
        public float[] BusElectricalTension700V { get; set; }
        public NotRegulatedConverter(int fifoSize) : base(fifoSize)
        {
            BusElectricalTension700V = new float[fifoSize];
        }
    }
}
