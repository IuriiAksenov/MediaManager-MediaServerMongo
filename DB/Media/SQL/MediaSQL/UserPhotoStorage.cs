using System;
using System.Data.Linq;
using System.Drawing;
using System.IO;
using System.Linq;
using MediaAPI;
using MediaSQL.DTO;

namespace MediaSQL
{
    public sealed class UserPhotoStorage : IPhotoStorage
    {
        private readonly string _connectionString;


        public UserPhotoStorage(string connectionString)
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
                    return dataContext.GetTable<UserPhoto>().Any(c => c.UserId == id && c.Picture != null);
                }
            }
        }

        public Image GetImageById(int id)
        {
            var bits = GetImageBytesById(id);
           
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(bits, 0, bits.Length);
            Image img = Image.FromStream(memoryStream);

            return img;
        }

        public int InsertImageOfUser(UserPhoto userPhoto)
        {
            using (var dataContext = new DataContext(_connectionString))
            {

                dataContext.GetTable<UserPhoto>().InsertOnSubmit(userPhoto);
                dataContext.SubmitChanges();

            }

            return userPhoto.Id;
        }

        public void UpdateImageOfUser(UserPhoto userPhoto)
        {
            if (AppCache.FindPhoto(userPhoto.Id) != null)
            {
                AppCache.DeletePhoto(userPhoto.Id);
                AppCache.Add(userPhoto.Id, userPhoto.Picture);
            }
            using (var dataContext = new DataContext(_connectionString))
            {
                UserPhoto dbPhoto = dataContext.GetTable<UserPhoto>().Single(p => p.UserId == userPhoto.UserId);

                dbPhoto.Picture = userPhoto.Picture;

                dataContext.SubmitChanges();
            }

        }

        public void DeleteImageOfUser(int id)
        {
            if (AppCache.FindPhoto(id) != null)
                AppCache.DeletePhoto(id);
            using (var dataContext = new DataContext(_connectionString))
            {
                dataContext.ExecuteCommand("DELETE FROM UserPhoto WHERE user_id={0}", id);
            }
        }

       
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
                        byte[] rez = dataContext.GetTable<UserPhoto>().Single(c => c.UserId == Id).Picture;
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
                    byte[] rez = dataContext.GetTable<UserPhoto>().Single(c => c.UserId == Id).Picture;
                    return rez.LongLength;
                }

            }
        }

        public bool AddOrUpdateFile(byte[] file, int id)
        {
            if (IsFileExist(id))
            {
                UpdateImageOfUser(new UserPhoto()
                {
                    UserId =  id,
                    Picture = file
                });
                return true;
            }
            InsertImageOfUser(new UserPhoto()
            {
                UserId = id,
                Picture = file
            });
            return true;
        }

        public bool IsFileExist(int userId)
        {
            using (var dataContext = new DataContext(_connectionString))
            {
                return dataContext.GetTable<UserPhoto>().Any(c => c.UserId == userId);
            }
        }
    }
}

