namespace AnnelidaDataFormat.Members
{
    public abstract class Converter : BaseSensoriedModule
    {
        public float BusElectricalTension { get; set; }
        public float[] ConverterElectricalCurrent { get; set; }
        public float[] ConverterElectricalPower { get; set; }
    }
}
