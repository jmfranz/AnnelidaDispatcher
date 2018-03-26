using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace AnnelidaDispatcher.Model
{
    /// <summary>
    /// Wrapper class around the MongoDB Driver.
    /// Provides simplified access to insert methods.
    /// </summary>
    public class MongoWrapper
    {

        protected IMongoClient client;
        protected IMongoDatabase database;
        
        /// <summary>
        /// Constructor for the wrapper class using a known external server
        /// </summary>
        /// <param name="mongoAddress">MongoURL String, starts with mongodb://</param>
        /// <param name="databaseName">The name of the database to be accessed</param>
        public MongoWrapper(string mongoAddress, string databaseName)
        {
            if (mongoAddress != null)
            {
                MongoUrl mongoURL = new MongoUrl(mongoAddress);
                client = new MongoClient(mongoURL);
            }
            else
            {
                client = new MongoClient();
            }
            database = client.GetDatabase(databaseName);
        }
            
        /// <summary>
        /// Cleans up the connection variables. There is no need to actually 
        /// disconnect from the DB
        /// </summary>
        public void Disconnect()
        {
            client = null;
            database = null;
        }

        /// <summary>
        /// Write a single document to the desired collectionn
        /// </summary>
        /// <param name="document">The Bson document to be added</param>
        /// <param name="collectionName">The target collection</param>
        /// <returns></returns>
        public async Task WriteSingleToCollection(BsonDocument document, string collectionName)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertOneAsync(document);
        }

        /// <summary>
        /// Write a single serialized document to the desired collection
        /// </summary>
        /// <param name="bytes"> The byte array containing a BSonDocument</param>
        /// <param name="collectionName">The target collection</param>
        /// <returns></returns>
        public async Task WriteSingleToCollection(byte[] bytes, string collectionName)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var document = BsonSerializer.Deserialize<BsonDocument>(bytes);
            await collection.InsertOneAsync(document);
        }

        /// <summary>
        /// Write a single Record to the desired collection
        /// </summary>
        /// <param name="record">The record object to be stored</param>
        /// <param name="collectionName">The target collection</param>
        /// <returns></returns>
        //TODO: Should this be async too?
        public void WriteSingleToCollection(Record record, string collectionName)
        {
            var collection = database.GetCollection<Record>(collectionName);
            collection.InsertOne(record);
        }

        /// <summary>
        /// Write a list of documents in bulk
        /// </summary>
        /// <param name="documents">A list of BSon documents</param>
        /// <param name="collectionName">The target collection</param>
        /// <returns></returns>
        public async Task WriteManyToCollection(List<BsonDocument> documents, string collectionName)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertManyAsync(documents.ToArray());
        }

        /// <summary>
        /// Write a list of serialized BsonDocuments in bulk
        /// </summary>
        /// <param name="documents">A list containing the BsonDocuments byte arrays</param>
        /// <param name="collectionName">The target colletion</param>
        /// <returns></returns>
        public async Task WriteManyToCollection(List<byte[]> bytesList, string collectionName)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            List<BsonDocument> documents = new List<BsonDocument>();
            foreach(var bArr in bytesList)
            {
                var document = BsonSerializer.Deserialize<BsonDocument>(bArr);
                documents.Add(document);
            }
            await collection.InsertManyAsync(documents.ToArray());
        }
    }
}
