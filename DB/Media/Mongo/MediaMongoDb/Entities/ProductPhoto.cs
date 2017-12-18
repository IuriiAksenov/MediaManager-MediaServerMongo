using System;
using MongoDB.Bson;

namespace MediaMongoDb
{
    public class ProductPhoto : BasePhotoEntity
    {
        public string ProductId { get; set; }
    }
}
