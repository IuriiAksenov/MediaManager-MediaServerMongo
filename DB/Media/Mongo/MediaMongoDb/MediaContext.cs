using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logger;
using MediaMongoDb.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace MediaMongoDb
{
    public class MediaContext
    {
        public IMongoDatabase Database{ get; }// бд
        public IGridFSBucket GridFsBucket { get; } //файловое хранилище

        public MediaContext() : this("mongodb://mkr0007:27017/mediastore")
        {
        }

        public MediaContext(string connectionString)
        {
            Log.Instance.LogAsInfo($"{nameof(MediaContext)}.{nameof(MediaContext)}: Constructor is tratring.");
            var connection = new MongoUrlBuilder(connectionString);

            MongoClient client = new MongoClient(connectionString);
            Database = client.GetDatabase(connection.DatabaseName);
            GridFsBucket = new GridFSBucket(Database);
            Log.Instance.LogAsInfo($"{nameof(MediaContext)}.{nameof(MediaContext)}: Constructor is completed.");
        }
    }

}

