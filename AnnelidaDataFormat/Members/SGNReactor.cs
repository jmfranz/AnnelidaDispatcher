using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDataFormat.Members
{
    public class SGNReactor
    {
        public float[] ReactionTemperature { get; set; }
        public float[] InternalReactorPressure { get; set; }
        public bool[] ObstructionDetected { get; set; }

        public SGNReactor(int fifoSize)
        {
            ReactionTemperature = new float[fifoSize];
            InternalReactorPressure = new float[fifoSize];
            ObstructionDetected = new bool[fifoSize];
        }

    }
}
