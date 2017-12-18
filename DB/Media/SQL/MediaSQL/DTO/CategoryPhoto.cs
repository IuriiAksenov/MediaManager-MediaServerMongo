using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace Media
{
    [Table]
    public sealed class CategoryPhoto
    {
        [Column(Name = "id", IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        [Column(Name = "category_id")]
        public int CategoryId { get; set; }

        [Column(Name = "image", UpdateCheck = UpdateCheck.Never)]
        public byte[] Picture { get; set; }

        [Column(Name = "type_id", CanBeNull = true)]

        public int? Type { get; set; }
    }
}
