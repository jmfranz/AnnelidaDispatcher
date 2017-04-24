using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendBsonToDispatcher
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
            Console.ReadKey();

            Console.WriteLine("Sending dummy data");
            Console.WriteLine("Press ESC to stop");

            var document = new BsonDocument
            {
                { "Time", 1 },
                { "Sensors", //new BsonArray
                new BsonDocument
                    {
                        { "Depth", 2},
                        { "Distance", 3},
                    }
                }
            };

            do
            {
                while (!Console.KeyAvailable)
                {
                    byte[] b = document.ToBson();
                    stream.Write(BitConverter.GetBytes(b.Length), 0, 4);
                    stream.Write(b, 0, b.Length);
                    stream.Flush();
                    Thread.Sleep(1000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
