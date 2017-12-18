
using System.Data.Linq.Mapping;

namespace MediaSQL.DTO
{
    [Table]
    public sealed class Photo
    {
        [Column(Name = "id", IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        [Column(Name = "product_id")]
        public int ProductId { get; set; }

        [Column(Name = "image", UpdateCheck = UpdateCheck.Never)]
        public byte[] Picture { get; set; }
    }
}

