using System;
using System.Data.Linq;
using System.Drawing;
using System.IO;
using System.Linq;
using Media;
using MediaAPI;
using MediaSQL.DTO;

namespace MediaSQL
{
    public class CategoryPhotoStorage : IPhotoStorage
    {
        private readonly string _connectionString;


        public CategoryPhotoStorage(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool IsImageExist(int id)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                if (dataContext.GetTable<CategoryPhoto>().FirstOrDefault(c => c.CategoryId == id) == null)
                    return false;
                else return true;
            }
        }
        public Image GetImageById(int id)
        {
            var bits = GetImageBytesByCategoryId(id);

            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(bits, 0, bits.Length);
            Image img = Image.FromStream(memoryStream);

            return img;
        }
        public byte[] GetImageBytesById(int Id)
        {
            return GetImageBytesByCategoryId(Id);
        }
        public long GetImageSize(int Id)
        {
            return GetImageBytesByCategoryId(Id).LongLength;
        }

        public int InsertImageOfCategory(CategoryPhoto photo)
        {
            using (var dataContext = new DataContext(_connectionString))
            {

                dataContext.GetTable<CategoryPhoto>().InsertOnSubmit(photo);
                dataContext.SubmitChanges();

            }

            return photo.Id;
        }


        public void UpdateImageOfCategory(CategoryPhoto photo)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                CategoryPhoto dbPhoto = dataContext.GetTable<CategoryPhoto>().Single(p => p.CategoryId == photo.CategoryId);

                dbPhoto.Picture = photo.Picture;

                dataContext.SubmitChanges();
            }

        }

        public void DeleteImageOfCategory(int id)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                dataContext.ExecuteCommand("DELETE FROM CategoryPhoto WHERE category_id={0}", id);
            }
        }

        public byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public byte[] GetImageBytesByCategoryId(int categoryId)
        {
            if (!IsImageExist(categoryId))
                throw new Exception("This photo does not exist");
            else
            {
                using (var dataContext = new DataContext(_connectionString))
                {
                    byte[] rez = dataContext.GetTable<CategoryPhoto>().Single(c => c.CategoryId == categoryId).Picture;
                    return rez;
                }
            }
        }

        public string GetImageTypeByCategoryId(int categoryId)
        {
            if (!IsImageExist(categoryId))
                throw new Exception("This photo does not exist");
            else
            {
                using (var dataContext = new DataContext(_connectionString))
                {
                    int? typeId = dataContext.GetTable<CategoryPhoto>().Single(c => c.CategoryId == categoryId).Type;
                    string type = dataContext.GetTable<PhotoType>().Single(c => c.Id == typeId).TypeName;

                    if (type == null)
                        throw new Exception("Type is not defined");
                    else
                        return type;
                }
            }
        }

        public bool AddOrUpdateFile(byte[] file, int id)
        {
            if (IsImageExist(id))
            {
                UpdateImageOfCategory(new CategoryPhoto()
                {
                    Id = id,
                    Picture = file
                });
                return true;
            }
            InsertImageOfCategory(new CategoryPhoto()
            {
                Id = id,
                Picture = file
            });
            return true;
        }
    }
}
