using System;
using System.Data.Linq;
using System.Drawing;
using System.IO;
using System.Linq;
using MediaAPI;
using MediaSQL.DTO;

namespace MediaSQL
{
    public sealed class BigPhotoStorage : IPhotoStorage
    {
        private readonly string _connectionString;


        public BigPhotoStorage(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool IsImageExist(int id)
        {
            if (AppCache.FindPhoto(id) != null)
                return true;
            else
            {
                using (var dataContext = new DataContext(_connectionString))
                {
                    return dataContext.GetTable<BigPhoto>().Any(c => c.ProductId == id && c.Picture != null);
                }
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

        public int InsertImageOfProduct(BigPhoto bigPhoto)
        {
            using (var dataContext = new DataContext(_connectionString))
            {

                dataContext.GetTable<BigPhoto>().InsertOnSubmit(bigPhoto);
                dataContext.SubmitChanges();

            }

            return bigPhoto.Id;
        }

        public void UpdateImageOfProduct(BigPhoto bigPhoto)
        {
            if (AppCache.FindPhoto(bigPhoto.Id) != null)
            {
                AppCache.DeletePhoto(bigPhoto.Id);
                AppCache.Add(bigPhoto.Id, bigPhoto.Picture);
            }
            using (var dataContext = new DataContext(_connectionString))
            {
                BigPhoto dbPhoto = dataContext.GetTable<BigPhoto>().Single(p => p.ProductId == bigPhoto.ProductId);

                dbPhoto.Picture = bigPhoto.Picture;

                dataContext.SubmitChanges();
            }

        }

        public void DeleteImageOfProduct(int id)
        {
            if (AppCache.FindPhoto(id) != null)
                AppCache.DeletePhoto(id);
            using (var dataContext = new DataContext(_connectionString))
            {
                dataContext.ExecuteCommand("DELETE FROM BigPhoto WHERE product_id={0}", id);
            }
        }

        // Dmitry Belkin Note: Why Gif ?!
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
                if (AppCache.FindPhoto(Id) != null)
                {
                    return AppCache.FindPhoto(Id);
                }
                else
                {
                    using (var dataContext = new DataContext(_connectionString))
                    {
                        byte[] rez = dataContext.GetTable<BigPhoto>().Single(c => c.ProductId == Id).Picture;
                        AppCache.Add(Id, rez);
                        return rez;
                    }
                }
                
            }
        }
        public long GetImageSize(int Id)
        {
            if (!IsImageExist(Id))
                throw new Exception("This photo does not exist");
            else
            {
                
                    using (var dataContext = new DataContext(_connectionString))
                    {
                        byte[] rez = dataContext.GetTable<BigPhoto>().Single(c => c.ProductId == Id).Picture;
                        return rez.LongLength;
                    }
                
            }
        }

        public bool AddOrUpdateFile(byte[] file, int id)
        {
            if (IsImageExist(id))
            {
                UpdateImageOfProduct(new BigPhoto()
                {
                    Id = id,
                    Picture = file
                });
                return true;
            }
            InsertImageOfProduct(new BigPhoto()
            {
                Id = id,
                Picture = file
            });
            return true;
        }

        public bool IsFileExist(int productId)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                return dataContext.GetTable<BigPhoto>().Any(c => c.ProductId == productId);
            }
        }
    }
}
