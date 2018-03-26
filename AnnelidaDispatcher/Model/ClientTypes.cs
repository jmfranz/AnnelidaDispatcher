using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDispatcher.Model
{
    /// <summary>
    /// Class that defines the possible types for the dispacher clients
    /// Note that all clients are Undefined until they identify themselves
    /// </summary>
    public static class ClientTypes
    {
        
        /// <summary>
        /// The types definition
        /// </summary>
        public enum Types {
            /// <summary>
            /// A client that has just connected and not yet identified itself
            /// </summary>
            Undefined,
            /// <summary>
            /// A view type i.e. receives sensor and fault data
            /// </summary>
            View,
            /// <summary>
            /// A controller type i.e. sends control signals to the robot
            /// </summary>
            Controller,
            /// <summary>
            /// A roboto type i.e. sends and receives data
            /// </summary>
            Robot };

    }
}
