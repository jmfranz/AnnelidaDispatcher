using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AnnelidaDispatcher.Model;

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
            uint count = 0;

            var document = new Sensors();
            document.timestamp = DateTime.UtcNow;
            Random rnd = new Random();

            document.backward.enclosure1.pressure = rnd.NextDouble();
            document.backward.enclosure1.temperature = rnd.NextDouble();
            document.backward.enclosure1.traction = 5 * Math.Sin(count * Math.PI / 180); 
            document.backward.enclosure1.orientation.Add(rnd.NextDouble());
            document.backward.enclosure1.orientation.Add(rnd.NextDouble());
            document.backward.enclosure1.orientation.Add(rnd.NextDouble());

            document.backward.enclosure2.temperature = rnd.NextDouble();
            document.backward.enclosure2.pressure = rnd.NextDouble();
            document.backward.enclosure2.inputCurrent = rnd.NextDouble();
            document.backward.enclosure2.inputVoltage = rnd.NextDouble();
            document.backward.enclosure2.outputCurrent = rnd.NextDouble();
            document.backward.enclosure2.outputVoltage = rnd.NextDouble();

            document.backward.enclosure3.engineSpeed.Add(rnd.NextDouble());
            document.backward.enclosure3.engineSpeed.Add(rnd.NextDouble());
            document.backward.enclosure3.engineSpeed.Add(rnd.NextDouble());
            document.backward.enclosure3.appliedPower.Add(rnd.NextDouble());
            document.backward.enclosure3.appliedPower.Add(rnd.NextDouble());
            document.backward.enclosure3.appliedPower.Add(rnd.NextDouble());

            document.central.enclosure4.temperature = rnd.NextDouble();
            document.central.enclosure4.pressure = rnd.NextDouble();
            document.central.enclosure4.engineControllerCurrent = rnd.NextDouble();
            document.central.enclosure4.engineControllerTemperature = rnd.NextDouble();

            document.central.enclosure5.pressure = rnd.NextDouble();
            document.central.enclosure5.temperature = rnd.NextDouble();
            document.central.enclosure5.travel = rnd.NextDouble();
            document.central.enclosure5.orientation.Add(rnd.NextDouble());
            document.central.enclosure5.orientation.Add(rnd.NextDouble());
            document.central.enclosure5.orientation.Add(rnd.NextDouble());

            document.central.enclosure6.temperature = rnd.NextDouble();
            document.central.enclosure6.pressure = rnd.NextDouble();
            document.central.enclosure6.engineControllerCurrent = rnd.NextDouble();
            document.central.enclosure6.engineControllerTemperature = rnd.NextDouble();

            document.forward.enclosure7.engineSpeed.Add(rnd.NextDouble());
            document.forward.enclosure7.engineSpeed.Add(rnd.NextDouble());
            document.forward.enclosure7.engineSpeed.Add(rnd.NextDouble());
            document.forward.enclosure7.appliedPower.Add(rnd.NextDouble());
            document.forward.enclosure7.appliedPower.Add(rnd.NextDouble());
            document.forward.enclosure7.appliedPower.Add(rnd.NextDouble());

            document.forward.enclosure8.externalPressure = rnd.NextDouble();
            document.forward.enclosure8.externalTemperature = rnd.NextDouble();

           


            do
            {
                while (!Console.KeyAvailable)
                {
                    byte[] b = document.ToBson();                   
                    stream.Write(b, 0, b.Length);
                    stream.Flush();
                    Thread.Sleep(100);
                    document.timestamp = DateTime.UtcNow;
                    count++;

                    document.backward.enclosure1.pressure = 2 * Math.Sin(count * Math.PI / 180);
                    document.backward.enclosure1.temperature = 2 * Math.Sin(count * Math.PI / 180 + 5 * Math.PI / 180); ;
                    document.backward.enclosure1.traction = 2 * Math.Sin(count * Math.PI / 180 + 10 * Math.PI / 180);
                    document.backward.enclosure1.orientation[0] = 2 * Math.Sin(count * Math.PI / 180 + 15 * Math.PI / 180);
                    document.backward.enclosure1.orientation[1] = 2 * Math.Sin(count * Math.PI / 180 + 20 * Math.PI / 180);
                    document.backward.enclosure1.orientation[2] = 2 * Math.Sin(count * Math.PI / 180 + 25 * Math.PI / 180);

                    document.backward.enclosure2.temperature = rnd.NextDouble();
                    document.backward.enclosure2.pressure = rnd.NextDouble();
                    document.backward.enclosure2.inputCurrent = rnd.NextDouble();
                    document.backward.enclosure2.inputVoltage = rnd.NextDouble();
                    document.backward.enclosure2.outputCurrent = rnd.NextDouble();
                    document.backward.enclosure2.outputVoltage = rnd.NextDouble();

                    document.backward.enclosure3.engineSpeed[0] = rnd.NextDouble();
                    document.backward.enclosure3.engineSpeed[1] = rnd.NextDouble();
                    document.backward.enclosure3.engineSpeed[2] = rnd.NextDouble();
                    document.backward.enclosure3.appliedPower[0] = rnd.NextDouble();
                    document.backward.enclosure3.appliedPower[1] = rnd.NextDouble();
                    document.backward.enclosure3.appliedPower[2] = rnd.NextDouble();

                    document.central.enclosure4.temperature = rnd.NextDouble();
                    document.central.enclosure4.pressure = rnd.NextDouble();
                    document.central.enclosure4.engineControllerCurrent = rnd.NextDouble();
                    document.central.enclosure4.engineControllerTemperature = rnd.NextDouble();

                    document.central.enclosure5.pressure = rnd.NextDouble();
                    document.central.enclosure5.temperature = rnd.NextDouble();
                    document.central.enclosure5.travel = rnd.NextDouble();
                    document.central.enclosure5.orientation[0] = rnd.NextDouble();
                    document.central.enclosure5.orientation[1] = rnd.NextDouble();
                    document.central.enclosure5.orientation[2] = rnd.NextDouble();

                    document.central.enclosure6.temperature = rnd.NextDouble();
                    document.central.enclosure6.pressure = rnd.NextDouble();
                    document.central.enclosure6.engineControllerCurrent = rnd.NextDouble();
                    document.central.enclosure6.engineControllerTemperature = rnd.NextDouble();

                    document.forward.enclosure7.engineSpeed[0] = rnd.NextDouble();
                    document.forward.enclosure7.engineSpeed[1] = rnd.NextDouble();
                    document.forward.enclosure7.engineSpeed[2] = rnd.NextDouble();
                    document.forward.enclosure7.appliedPower[0] = rnd.NextDouble();
                    document.forward.enclosure7.appliedPower[1] = rnd.NextDouble();
                    document.forward.enclosure7.appliedPower[2] = rnd.NextDouble();

                    document.forward.enclosure8.externalPressure = rnd.NextDouble();
                    document.forward.enclosure8.externalTemperature = rnd.NextDouble();



                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
