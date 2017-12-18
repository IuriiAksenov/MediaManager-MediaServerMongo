using System;
using System.Data.Linq;
using System.Drawing;
using System.IO;
using System.Linq;
using MediaAPI;
using MediaSQL.DTO;

namespace MediaSQL
{
    public sealed class PhotoStorage : IPhotoStorage
    {
        private readonly string _connectionString;


        public PhotoStorage(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool IsImageExist(int id)
        {
            
                using (var dataContext = new DataContext(_connectionString))
                {
                    if (dataContext.GetTable<Photo>().FirstOrDefault(c => c.ProductId == id) == null)
                        return false;
                    else return true;
                }
            
        }

        public Image GetImageById(int id)
        {
            var bits = GetImageBytesById(id);
          
            // Dmitry Belkin TODO: не плодить стримы, нормальное копирование через маршалинг и LockBits(...)
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(bits, 0, bits.Length);
            Image img = Image.FromStream(memoryStream);

            return img;
        }

        public int InsertImageOfProduct(Photo bigPhoto)
        {
            using (var dataContext = new DataContext(_connectionString))
            {

                dataContext.GetTable<Photo>().InsertOnSubmit(bigPhoto);
                dataContext.SubmitChanges();

            }

            return bigPhoto.Id;
        }


        public void UpdateImageOfProduct(Photo bigPhoto)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                Photo dbPhoto = dataContext.GetTable<Photo>().Single(p => p.ProductId == bigPhoto.ProductId);

                dbPhoto.Picture = bigPhoto.Picture;

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

        // Dmitry Belkin Note: Why Gif ?!
        // Saves for test compatibility
        public byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public byte[] GetImageBytesById(int Id)
        {
            if (!IsImageExist(Id))
                throw new Exception("This photo does not exist");
            else
            {
                    using (var dataContext = new DataContext(_connectionString))
                    {
                        byte[] rez = dataContext.GetTable<Photo>().Single(c => c.ProductId == Id).Picture;
                        return rez;
                    }
                
            }        
        }

        public long GetImageSize(int id)
        {
            throw new NotImplementedException();
        }

        public bool AddOrUpdateFile(byte[] file, int id)
        {
            if (IsImageExist(id))
            {
                UpdateImageOfProduct(new Photo()
                {
                    Id = id,
                    Picture = file
                });
                return true;
            }
            InsertImageOfProduct(new Photo()
            {
                Id = id,
                Picture = file
            });
            return true;
        }
    }
}
