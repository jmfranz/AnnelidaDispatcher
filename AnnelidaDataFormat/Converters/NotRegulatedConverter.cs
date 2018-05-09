using AnnelidaDataFormat.Members;

namespace AnnelidaDataFormat.Converters
{
    public class NotRegulatedConverter : Converter
    {
        public NotRegulatedConverter()
        {
            ConverterElectricalCurrent = new float[3];
            ConverterElectricalPower = new float[3];
        }
    }
}
