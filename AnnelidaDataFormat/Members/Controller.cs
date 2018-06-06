using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDataFormat.Members
{
    public class Controller : BaseSensoriedModule
    {
        public float[] ElectricalCurrent { get; set; }
        public float[] ElectricalTension { get; set; }

        public Controller(int fifoSize) : base(fifoSize)
        {
            ElectricalCurrent = new float[fifoSize];
            ElectricalTension = new float[fifoSize];
        }
    }
}
