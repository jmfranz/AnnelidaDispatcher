namespace AnnelidaDataFormat.Members
{
    public class EmbeddedSystem : BaseSensoriedModule
    {
        public float ExternalModulePressure { get; set; }
        public float[] Rotation { get; set; }
        public float[] Acceleration { get; set; }

        public EmbeddedSystem()
        {
            Rotation = new float[3];
            Acceleration = new float[3];
        }

    }
}
