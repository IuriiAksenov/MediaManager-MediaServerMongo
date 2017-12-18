using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;

namespace MediaSQL.DTO
{
    [Table]
    public sealed class UserPhoto
    {
        [Column(Name = "id", IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        [Column(Name = "user_id")]
        public int UserId { get; set; }

        [Column(Name = "image", UpdateCheck = UpdateCheck.Never)]
        public byte[] Picture { get; set; }
    }
}
