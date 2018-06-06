using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDataFormat
{
    public class AnnelidaControlPackage
    {
        /// <summary>
        /// Array containing any detected fault codes from the High-Level Interface
        /// </summary>
        public string[] Faults { get; set; }
        /// <summary>
        /// Speed value from physical interface
        /// Values should be between -1 and +1 i.e. [-1,1]
        /// </summary>
        public float SpeedControl { get; set; }
        /// <summary>
        /// Step size value from physcal interface
        /// Values should be between -1 and +1 i.e. [-1,1]
        /// </summary>
        public float StepSizeControl { get; set; }

        /// <summary>
        /// Emergency stop boolean
        /// </summary>
        public bool EStop { get; set; }
        
        private const int MaximumFaultsCount = 256;

        public AnnelidaControlPackage()
        {
            Faults = new string[MaximumFaultsCount];
            EStop = false;
        }
    }
}
