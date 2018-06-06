using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnnelidaDataFormat;
using AnnelidaDispatcher.Model;
using MongoDB.Bson;

namespace StoreInMongo
{
    class Program
    {
        static void Main(string[] args)
        {
            AnnelidaSensorPackage pk = new AnnelidaSensorPackage();
            MongoWrapper mongo = new MongoWrapper(null,"NewPK");

            var bson = pk.ToBson();
            
            pqp a = new pqp();
            a.HUE(mongo, bson);
            Thread.Sleep(3000);

        }

        
    }

    public class pqp
    {
        public async Task HUE(MongoWrapper mongo, byte[] bson)
        {
            await mongo.WriteSingleToCollection(bson, "NewPK");
            

        }
    }

}
