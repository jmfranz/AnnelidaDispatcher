using System.Net.Sockets;
using System.Text;

namespace AnnelidaDispatcher.Model
{
  
    /// <summary>
    /// State class for reading client data asynchronously
    /// </summary>
    public class DispatcherClientObject
    {
        /// <summary>
        /// Client  socket.
        /// </summary>
        public Socket WorkSocket = null;
        /// <summary>
        /// Size of receive buffer.
        /// </summary>
        public int BufferSize;
        /// <summary>
        /// Receive buffer.
        /// </summary>
        public byte[] Buffer;
        /// <summary>
        /// Received data string.
        /// </summary>
        public StringBuilder Sb = new StringBuilder();
        /// <summary>
        /// Is initialized into a specific client type
        /// </summary>
        public bool IsInitialized;
        /// <summary>
        /// Counter for how many bytes were received in the last recv call
        /// </summary>
        public int RecvBytesCount;

        /// <summary>
        /// The type of the client instance
        /// </summary>
        public ClientTypes.Types MyType;

        /// <summary>
        /// Class constructor, defines the type as undefined and waits for client identification
        /// </summary>
        public DispatcherClientObject()
        {
            //Uppon init we expect and int32
            BufferSize = 4;
            RecvBytesCount = 0;
            IsInitialized = false;
            Buffer = new byte[BufferSize];
            MyType = ClientTypes.Types.Undefined;
        }
    }
}
