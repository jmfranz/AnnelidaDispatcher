
using System;

namespace AnnelidaDataFormat.Members
{
    public class Manifolds
    {
        private const int NumSolenoids = 8;
        public bool[][] SolenoidActivation { get; set; }
        public float[] LegsHydraulicPressure { get; set; }
        public float[] MainCylinderHydraulicPressure { get; set; }

        public Manifolds(int fifoSize)
        {
            SolenoidActivation = new bool[NumSolenoids][];
            LegsHydraulicPressure = new float[fifoSize];
            MainCylinderHydraulicPressure = new float[fifoSize];

            for (int i = 0; i < NumSolenoids; i++)
            {
                SolenoidActivation[i] = new bool[fifoSize];
            }
        }

    }
}
