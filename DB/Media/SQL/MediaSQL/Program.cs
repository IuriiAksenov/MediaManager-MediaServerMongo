using System.Data.Linq;
using DBService;
using DataContracts;
using System.Configuration;
using System;
using System.Drawing;
using MediaSQL.DTO;
using MediaMongo;

namespace MediaSQL
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=mkr0009;Initial Catalog=Media; User ID=db_user;Password=Qwerty_123; Integrated Security=False";

            DataContext db = new DataContext(connectionString);
            CategoryPhotoStorage categoryPhoto = new CategoryPhotoStorage(connectionString);
            BigPhotoStorage bigPhoto = new BigPhotoStorage(connectionString);


            UserPhotoStorage userPhoto = new UserPhotoStorage(connectionString);

           //    BigPhotoStorageMongo big = new BigPhotoStorageMongo(@"mongodb://mkr0007:27017/mediastore");

           Image img = Image.FromFile(@"C:\Users\Лена\Desktop\Photo\user.jpg");
            byte[] img1 = bigPhoto.ImageToByteArray(img);



            userPhoto.GetImageById(1).Save(@"C:\Users\Лена\Desktop\Photo\rayan.jpg");
            
        }
    }
}
