namespace AnnelidaDataFormat.Members
{
    public class Umbilical
    {
        public float[] Traction { get; set; }

        public Umbilical(int fifoSize)
        {
            Traction = new float[fifoSize];
        }
    }
}
