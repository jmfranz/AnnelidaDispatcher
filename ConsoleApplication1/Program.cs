using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AnnelidaDispatcher.Model;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 5; i ++)
            {
                Thread t = new Thread(() => InsertThread(i));
                t.Start();
            }

            Console.ReadKey();
        }


        public static void InsertThread(int number)
        {
            Console.WriteLine($"Thread number {number}");

            var client = new MongoClient();
            var db = client.GetDatabase("classTest");
            //db.DropCollection("recordClass");
            var col = db.GetCollection<Record>("recordClass5m");

            var obj = new Record();
            int insertionCount = 0;
            while (insertionCount < 60)
            {
                var s = new Sensors();
                s.temperature = 49;
                s.depth = 100.4123;
                s.distance = 4231;

                if ((s.timestamp - obj.timestamp).TotalSeconds > 1)
                {
                    col.InsertOne(obj);
                    obj = new Record();
                    obj.timestamp = s.timestamp;
                    obj.sensors.Add(s);
                    insertionCount++;
                }
                else
                {
                    obj.sensors.Add(s);
                    Thread.Sleep(5);
                }
            }
        }
          
            


            //var uid = obj.Id;
            //var filter = Builders<Record>.Filter.Eq("_id", uid);

            //var res = col.FindSync<Record>(filter).ToList();
            //if(res != null)
            //{
            //    var s = DateTime.UtcNow - res[0].Timestamp;
            //    Console.WriteLine(s.Milliseconds);
            //}
            //Console.ReadKey();






        
    }
}
