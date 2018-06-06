using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDataFormat.Members
{
    public abstract class Locomotive
    {
        public bool[] CylinderForward { get; set; }
        public bool[] CylinderBackward { get; set; }

        protected Locomotive(int fifoSize)
        {
            CylinderBackward = new bool[fifoSize];
            CylinderForward = new bool[fifoSize];
        }
    }
}
