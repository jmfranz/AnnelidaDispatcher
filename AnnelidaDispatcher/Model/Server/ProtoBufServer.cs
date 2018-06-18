using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;


namespace AnnelidaDispatcher.Model.Server
{
    public class ProtoBufServer : AsyncAbstractServer
    {
        private MessageParser<AnnelidaSensors> protobuffParser;
        public ProtoBufServer(int tcpPort) : base(tcpPort)
        {
            


        }

        protected override void HandleMessage(byte[] buffer, int ammountRead,
            ClientTypes.Types myType)
        {
            var workingCopy = new byte[ammountRead];
            Array.Copy(buffer,4,workingCopy,0,workingCopy.Length);

            var s = AnnelidaSensors.Parser.ParseFrom(buffer);
            s.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            
            messageDispatchStrategy.RedespatchMessage(s.ToByteArray(),connectedClients,myType);
        }
        
    }
}
