using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Net.Sockets;

namespace ReceiveBsonFromDispatcher
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
                    byte[] size = new byte[4];
                    byte[] buff;
                    c.Client.Receive(size, 4, 0);
                    int s = BitConverter.ToInt32(size, 0);
                    buff = new byte[s];
                    c.Client.Receive(buff, s, 0);

                    var d = BsonSerializer.Deserialize<BsonDocument>(buff);
                    Console.WriteLine(d.ToString());
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

        }
    }
}
