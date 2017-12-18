using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace MediaMongoDb.Repositories
{
    public class UserPhotoRepository :IPhotoRepository<UserPhoto>
    {
        private readonly IMongoDatabase _database; // бд
        private readonly IGridFSBucket _gridFS; //файловое хранилище

        public UserPhotoRepository(MediaContext db)
        {
            Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(UserPhotoRepository)}: Constructor is tratring.");
            _database = db.Database;
            _gridFS = db.GridFsBucket;

            CreateProductIdIndex().GetAwaiter().GetResult();
            Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(UserPhotoRepository)}: Constructor is ended.");
        }

        public IMongoCollection<UserPhoto> Photos
        {
            get
            {
                try
                {
                    Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(Photos)}: Get Photos is starting");
                    var result = _database.GetCollection<UserPhoto>("UserPhotos");
                    Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(Photos)}: Get Photos is successfully ended.");
                    return result;
                }
                catch (Exception e)
                {
                    Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(Photos)}: Error");
                    throw;
                }

            }
        }

        public async Task<IEnumerable<UserPhoto>> GetAllItems()
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(GetAllItems)}: GetAllItems is starting");
                var builder = new FilterDefinitionBuilder<UserPhoto>();
                var filter = builder.Empty; //фильтр для выборки всех элементов
                var result = await Photos.Find(filter).ToListAsync();
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(GetAllItems)}: GetAllItems is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(GetAllItems)}: Error");
                throw;
            }
        }

        public async Task<IEnumerable<UserPhoto>> GetPhotoById(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(GetPhotoById)}: GetPhotoById is starting");
                //строитель фильтров
                var builder = new FilterDefinitionBuilder<UserPhoto>();
                var filter = builder.Empty; //фильтр для выборки всех элементов

                //фильтра по имени
                if (!String.IsNullOrWhiteSpace("UserId"))
                {
                    filter = filter & builder.Eq("UserId", itemId);
                }

                var result = await Photos.Find(filter).ToListAsync();
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(GetPhotoById)}: GetPhotoById is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(GetPhotoById)}: Error");
                throw;
            }
        }

        public async Task Create(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(Create)}: Create is starting");
                UserPhoto photo = new UserPhoto { UserId = itemId };
                await Photos.InsertOneAsync(photo);
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(Create)}: Create is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(Create)}: Error");
                throw;
            }
        }

        public async Task Update(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(Update)}: Update is starting");
                UserPhoto photo = new UserPhoto { UserId = itemId };
                await Photos.ReplaceOneAsync(new BsonDocument("UserId", photo.UserId), photo);
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(Update)}: Update is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(Create)}: Error");
                throw;
            }

        }

        public async Task Delete(string productId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(Delete)}: Delete is starting");
                UserPhoto photo = new UserPhoto { UserId = productId };
                await Photos.DeleteOneAsync(new BsonDocument("UserId", photo.UserId));
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(Delete)}: Delete is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(Delete)}: Error");
                throw;
            }

        }

        public async Task StoreImage(UserPhoto photo, byte[] image)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(StoreImage)}: StoreImage is starting. {nameof(UserPhoto)}");
                if (photo.HasImageSource())
                {
                    //если ранее уже была фотка то удаляем её
                    photo.ImageSize = "0";
                    await _gridFS.DeleteAsync(new ObjectId(photo.ImageSourceId));
                }

                string fileName = photo.UserId;
                ObjectId imageId = await _gridFS.UploadFromBytesAsync(fileName, image);

                photo.ImageSize = image.Length.ToString();

                //обновляем данные по дуомукументу

                photo.ImageSourceId = imageId.ToString();
                var filter = Builders<UserPhoto>.Filter.Eq("UserId", photo.UserId);
                var update = Builders<UserPhoto>.Update.Set("ImageSourceId", photo.ImageSourceId).Set("ImageSize", photo.ImageSize);
                await Photos.UpdateOneAsync(filter, update);
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(StoreImage)}: StoreImage is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(StoreImage)}: Error");
                throw;
            }
        }

        public async Task<byte[]> GetImageSource(UserPhoto photo)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(GetImageSource)}: GetImageSource is starting");
                var result = await _gridFS.DownloadAsBytesAsync(new ObjectId(photo.ImageSourceId));
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(GetImageSource)}: GetImageSource is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(GetImageSource)}: Error");
                throw;
            }
        }

        public string GetSizeOfImageSource(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(GetSizeOfImageSource)}: GetSizeOfImageSource is starting");
                var photo = GetPhotoById(itemId).GetAwaiter().GetResult().FirstOrDefault() ?? new UserPhoto();
                var result = photo.ImageSize ?? "0";
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(GetSizeOfImageSource)}: GetSizeOfImageSource is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(GetSizeOfImageSource)}: Error");
                throw;
            }
        }

        public void InsertPhotoAndImageSource(string itemId, byte[] image)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: InsertPhotoAndImageSource is starting");
                Create(itemId).GetAwaiter().GetResult();
                StoreImage(new UserPhoto { UserId = itemId, ImageSize = image.Length.ToString() }, image).GetAwaiter().GetResult();
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: InsertPhotoAndImageSource is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: Error");
                throw;
            }
        }

        public bool IsFileExist(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(IsFileExist)}: IsFileExist");
                var photo = GetPhotoById(itemId).GetAwaiter().GetResult().FirstOrDefault() ?? new UserPhoto();
                var result = photo.HasImageSource();
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(IsFileExist)}: IsFileExist is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(IsFileExist)}: Error");
                throw;
            }
        }

        private async Task CreateProductIdIndex()
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(CreateProductIdIndex)}: CreateProductIdIndex");
                var collection = _database.GetCollection<UserPhoto>("Photos");
                var builder = Builders<UserPhoto>.IndexKeys.Ascending(x => x.UserId);
                await collection.Indexes.CreateOneAsync(builder);
                Log.Instance.LogAsInfo($"{nameof(UserPhotoRepository)}.{nameof(CreateProductIdIndex)}: CreateProductIdIndex is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(UserPhotoRepository)}.{nameof(CreateProductIdIndex)}: Error");
                throw;
            }
        }
    }
}

