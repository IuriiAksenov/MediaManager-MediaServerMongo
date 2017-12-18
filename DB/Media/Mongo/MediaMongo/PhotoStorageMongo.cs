using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaAPI;
using MediaMongoCache;
using MediaMongoDb;
using MediaMongoDb.Repositories;
using Logger;

namespace MediaMongo
{
    public class PhotoStorageMongo<T> : IPhotoStorage where T : BasePhotoEntity
    {
        private readonly IPhotoRepository<T> _db;
        private readonly PhotoCache _cache;

        public PhotoStorageMongo(IPhotoRepository<T> photoRepository)
        {
            Log.Instance.LogAsInfo($"{nameof(PhotoStorageMongo<T>)}.{nameof(PhotoStorageMongo<T>)}: Constructor is stratring.");
            _db = photoRepository;
            _cache = new PhotoCache();
            Log.Instance.LogAsInfo($"{nameof(PhotoStorageMongo<T>)}.{nameof(PhotoStorageMongo<T>)}: Constructor is stratring.");
        }

        public byte[] GetImageBytesById(int itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(PhotoStorageMongo<T>)}.{nameof(GetImageBytesById)}: GetImageBytesById is stratring. {nameof(itemId)}:{itemId}");
                byte[] image = new byte[0];

                if (_cache.TryGetValue(itemId, out image))
                {
                    Log.Instance.LogAsError($"{nameof(PhotoStorageMongo<T>)}.{nameof(GetImageBytesById)}: GetImageBytesById Error. {nameof(itemId)}:{itemId}");
                    return new byte[0];
                }

                var photo = _db.GetPhotoById(itemId.ToString()).GetAwaiter().GetResult().FirstOrDefault();
                image = _db.GetImageSource(photo).GetAwaiter().GetResult();
                _cache.AddOrUpdate(itemId, image);
                Log.Instance.LogAsInfo($"{nameof(PhotoStorageMongo<T>)}.{nameof(GetImageBytesById)}: GetImageBytesById is completed. {nameof(itemId)}:{itemId}");
                return image;

            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(PhotoStorageMongo<T>)}.{nameof(GetImageBytesById)}: GetImageBytesById Error. {nameof(itemId)}:{itemId}");
                return new byte[0];
            }
        }

        public long GetImageSize(int itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(PhotoStorageMongo<T>)}.{nameof(GetImageSize)}: GetImageSize is stratring. {nameof(itemId)}:{itemId}");
                var result = GetImageBytesById(itemId).LongLength;
                Log.Instance.LogAsInfo($"{nameof(PhotoStorageMongo<T>)}.{nameof(GetImageSize)}: GetImageSize is stratring. {nameof(itemId)}:{itemId}");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(PhotoStorageMongo<T>)}.{nameof(GetImageSize)}: GetImageSize Error. {nameof(itemId)}:{itemId}");
                return 0;
            }
        }

        public bool AddOrUpdateFile(byte[] file, int itemId)
        {
            try
            {
                Log.Instance.LogAsInfo(nameof(PhotoStorageMongo<T>), nameof(AddOrUpdateFile), $"AddOrUpdateFile is stratring. {nameof(itemId)}:{itemId}");
                if (_cache.IsExist(itemId))
                    _cache.TryRemove(itemId);
                if (_db.IsFileExist(itemId.ToString()))
                {
                    var item = _db.GetPhotoById(itemId.ToString()).GetAwaiter().GetResult().FirstOrDefault();
                    _db.StoreImage(item, file).GetAwaiter().GetResult();
                    _cache.AddOrUpdate(itemId, file);
                    Log.Instance.LogAsInfo(nameof(PhotoStorageMongo<T>), nameof(AddOrUpdateFile), $"AddOrUpdateFile is stratring. {nameof(itemId)}:{itemId}");
                    return true;
                }

                _db.InsertPhotoAndImageSource(itemId.ToString(), file);
                _cache.AddOrUpdate(itemId, file);
                Log.Instance.LogAsInfo(nameof(PhotoStorageMongo<T>), nameof(AddOrUpdateFile), $"AddOrUpdateFile is stratring. {nameof(itemId)}:{itemId}");
                return true;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError(nameof(PhotoStorageMongo<T>), nameof(AddOrUpdateFile), $"AddOrUpdateFile Error. {nameof(itemId)}:{itemId}");
                return false;
            }
        }

        public bool IsImageExist(int itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(PhotoStorageMongo<T>)}.{nameof(IsImageExist)}: IsImageExist is stratring. {nameof(itemId)}:{itemId}");
                if (_cache.IsExist(itemId)) return true;
                var reult = _db.IsFileExist(itemId.ToString());
                Log.Instance.LogAsInfo($"{nameof(PhotoStorageMongo<T>)}.{nameof(IsImageExist)}: IsImageExist is stratring. {nameof(itemId)}:{itemId}");
                return reult;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(PhotoStorageMongo<T>)}.{nameof(IsImageExist)}: IsImageExist Error. {nameof(itemId)}:{itemId}");
                return false;
            }
        }
    }
}
