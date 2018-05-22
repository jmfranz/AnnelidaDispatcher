using AnnelidaDataFormat.Members;

namespace AnnelidaDataFormat.Converters
{
    public class RegulatedConverter : Converter
    {
        public float[] IntermidiateBusElectricalTension { get; set; }

        public RegulatedConverter(int fifoSize) : base(fifoSize)
        {
            IntermidiateBusElectricalTension = new float[fifoSize];
        }
    }
}
