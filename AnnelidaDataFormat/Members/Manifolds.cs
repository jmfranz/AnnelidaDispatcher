
using System;

namespace AnnelidaDataFormat.Members
{
    public class Manifolds
    {
        private const int NUM_SOLENOIDS = 8;
        public bool[][] SolenoidActivation { get; set; }
        public float[] LegsHydraulicPressure { get; set; }
        public float[] MainCylinderHydraulicPressure { get; set; }

        public Manifolds(int fifoSize)
        {
            SolenoidActivation = new bool[NUM_SOLENOIDS][];
            LegsHydraulicPressure = new float[fifoSize];
            MainCylinderHydraulicPressure = new float[fifoSize];

            for (int i = 0; i < fifoSize; i++)
            {
                SolenoidActivation[i] = new bool[fifoSize];
            }
        }

    }
}
