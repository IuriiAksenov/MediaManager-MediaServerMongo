using System;
using System.Data.Linq;
using System.Drawing;
using System.IO;
using System.Linq;
using MediaSQL.DTO;

namespace MediaSQL
{
    public sealed class MediaStorage
    {
        private readonly string _connectionString;


        public MediaStorage(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool CheckExistingImage(int product_id)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                if (dataContext.GetTable<Photo>().FirstOrDefault(c => c.ProductId == product_id) == null)
                    return false;
                else return true;
            }
        }

        public Image GetImageByProductId(int product_id)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                if (!CheckExistingImage(product_id))
                {
                    throw new Exception("This photo does not exist");
                }
                else
                {

                    byte[] link = dataContext.GetTable<Photo>().Single(c => c.ProductId == product_id).Picture;

                    MemoryStream memoryStream = new MemoryStream();
                    memoryStream.Write(link, 0, link.Length);
                    Image img = Image.FromStream(memoryStream);


                    return img;
                }
            }
        }



        public int InsertImageOfProduct(Photo photo)
        {
            using (var dataContext = new DataContext(_connectionString))
            {

                dataContext.GetTable<Photo>().InsertOnSubmit(photo);
                dataContext.SubmitChanges();

            }

            return photo.Id;
        }


        public void UpdateImageOfProduct(Photo photo)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                Photo dbPhoto = dataContext.GetTable<Photo>().Single(p => p.ProductId == photo.ProductId);

                dbPhoto.Picture = photo.Picture;

                dataContext.SubmitChanges();
            }

        }

        public void DeleteImageOfProduct(int id)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                dataContext.ExecuteCommand("DELETE FROM Photo WHERE product_id={0}", id);
            }
        }

        public byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }


    }
}
