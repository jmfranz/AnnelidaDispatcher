using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDataFormat
{
    public abstract class BaseSensoriedModule
    {
        public float[] InternalTemperature { get; set; }
        public float[] InternalPressure { get; set; }
        //public float[] ModuleDeformation { get; set; }

        protected BaseSensoriedModule(int fifoSize)
        {
            InternalPressure = new float[fifoSize];
            InternalTemperature = new float[fifoSize];
        }
    }
}
