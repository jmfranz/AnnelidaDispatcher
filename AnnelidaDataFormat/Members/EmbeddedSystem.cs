namespace AnnelidaDataFormat.Members
{
    public class EmbeddedSystem : BaseSensoriedModule
    {
        public float[] ExternalModulePressure { get; set; }
        public float[][] Rotation { get; set; }
        public float[][] Displacement { get; set; }

        public EmbeddedSystem(int fifoSize) : base(fifoSize)
        {
            ExternalModulePressure = new float[fifoSize];
            Rotation = new float[3][];
            Displacement = new float[3][];

            for (int i = 0; i < fifoSize; i++)
            {
                Rotation[i] = new float[fifoSize];
                Displacement[i] = new float[fifoSize];
            }


        }

    }
}
