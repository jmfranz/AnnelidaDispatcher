using AnnelidaDataFormat.Members;

namespace AnnelidaDataFormat.Converters
{
    public class RegulatedConverter : Converter
    {
        public RegulatedConverter()
        {
            ConverterElectricalCurrent = new float[4];
            ConverterElectricalPower = new float[4];
        }
    }
}
