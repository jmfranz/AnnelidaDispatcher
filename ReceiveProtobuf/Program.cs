using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;

using static AnnelidaSensors;

namespace ReceiveProtobuf
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient c = new TcpClient();
            c.Connect("127.0.0.1", 9999);
            var stream = c.GetStream();

            //int id = (int)ClientTypes.Types.Controller ;
            Console.Write("Whats the id?: ");
            int id;
            string ca = Console.ReadLine();
            Int32.TryParse(ca, out id);

            stream.Write(BitConverter.GetBytes(id), 0, 4);
            stream.Flush();

            Console.WriteLine("Ready to receive...");
            Console.WriteLine("Press ESC to stop");

            do
            {
                while (!Console.KeyAvailable)
                {
                    byte[] buff = new byte[4];
                    c.Client.Receive(buff, 4, 0);
                    int s = BitConverter.ToInt32(buff, 0);
                    buff = new byte[s];
                    c.Client.Receive(buff, s, 0);
                    var prot = AnnelidaSensors.Parser.ParseFrom(buff);
                    Console.WriteLine(prot.ToString());
                }

            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
