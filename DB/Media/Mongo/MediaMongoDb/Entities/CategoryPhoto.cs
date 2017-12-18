using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MediaMongoDb
{
    public class CategoryPhoto : BasePhotoEntity
    {
        public string CategoryId { get; set; }
    }
}
