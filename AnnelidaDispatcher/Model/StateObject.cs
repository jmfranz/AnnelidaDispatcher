using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AnnelidaDispatcher.Model
{
    // State object for reading client data asynchronously
    public class DispatcherClientObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public int bufferSize;
        // Receive buffer.
        public byte[] buffer;
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        // Is initialized into a specific client type
        public bool isInitialized;
        public int recvBytesCount;

        public ClientTypes.Types myType;

        public DispatcherClientObject(int buffSize)
        {
            //Uppon init we expect and int32
            bufferSize = 4;
            recvBytesCount = 0;
            isInitialized = false;
            buffer = new byte[bufferSize];
            myType = ClientTypes.Types.Undefined;
        }
    }
}
