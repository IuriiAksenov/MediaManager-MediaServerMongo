using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MediaMongoDb
{
    public class BasePhotoEntity
    {
        public ObjectId Id { get; set; }
        public string ImageSourceId { get; set; }
        public string ImageSize { get; set; } = "0";
        public bool HasImageSource() => !String.IsNullOrWhiteSpace(ImageSourceId);
    }
}
