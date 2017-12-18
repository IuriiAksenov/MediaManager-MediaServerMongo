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
    public class CategoryPhotoRepository : IPhotoRepository<CategoryPhoto>
    {
        private readonly IMongoDatabase _database; // бд
        private readonly IGridFSBucket _gridFS; //файловое хранилище

        public CategoryPhotoRepository(MediaContext db)
        {
            Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(CategoryPhotoRepository)}: Constructor is stratring.");
            _database = db.Database;
            _gridFS = db.GridFsBucket;
            CreateProductIdIndex().GetAwaiter().GetResult();
            Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(CategoryPhotoRepository)}: Constructor is ended.");
        }

        public IMongoCollection<CategoryPhoto> Photos
        {
            get
            {
                try
                {
                    Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(Photos)}: Get Photos is starting");
                    var result = _database.GetCollection<CategoryPhoto>("CategoryPhotos");
                    Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(Photos)}: Get Photos is successfully ended.");
                    return result;
                }
                catch (Exception e)
                {
                    Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(Photos)}: Error");
                    throw;
                }

            }
        }

        public async Task<IEnumerable<CategoryPhoto>> GetAllItems()
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(GetAllItems)}: GetAllItems is starting");
                var builder = new FilterDefinitionBuilder<CategoryPhoto>();
                var filter = builder.Empty; //фильтр для выборки всех элементов
                var result = await Photos.Find(filter).ToListAsync();
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(GetAllItems)}: GetAllItems is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(GetAllItems)}: Error");
                throw;
            }
        }

        public async Task<IEnumerable<CategoryPhoto>> GetPhotoById(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(GetPhotoById)}: GetPhotoById is starting");
                //строитель фильтров
                var builder = new FilterDefinitionBuilder<CategoryPhoto>();
                var filter = builder.Empty; //фильтр для выборки всех элементов

                //фильтра по имени
                if (!String.IsNullOrWhiteSpace("CategoryId"))
                {
                    filter = filter & builder.Eq("CategoryId", itemId);
                }

                var result = await Photos.Find(filter).ToListAsync();
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(GetPhotoById)}: GetPhotoById is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(GetPhotoById)}: Error");
                throw;
            }
        }

        public async Task Create(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(Create)}: Create is starting");
                CategoryPhoto photo = new CategoryPhoto { CategoryId = itemId };
                await Photos.InsertOneAsync(photo);
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(Create)}: Create is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(Create)}: Error");
                throw;
            }
        }

        public async Task Update(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(Update)}: Update is starting");
                CategoryPhoto photo = new CategoryPhoto { CategoryId = itemId };
                await Photos.ReplaceOneAsync(new BsonDocument("CategoryId", photo.CategoryId), photo);
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(Update)}: Update is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(Create)}: Error");
                throw;
            }

        }

        public async Task Delete(string productId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(Delete)}: Delete is starting");
                CategoryPhoto photo = new CategoryPhoto { CategoryId = productId };
                await Photos.DeleteOneAsync(new BsonDocument("CategoryId", photo.CategoryId));
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(Delete)}: Delete is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(Delete)}: Error");
                throw;
            }

        }

        public async Task StoreImage(CategoryPhoto photo, byte[] image)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(StoreImage)}: StoreImage is starting. {nameof(CategoryPhoto)}");
                if (photo.HasImageSource())
                {
                    //если ранее уже была фотка то удаляем её
                    photo.ImageSize = "0";
                    await _gridFS.DeleteAsync(new ObjectId(photo.ImageSourceId));
                }

                string fileName = photo.CategoryId;
                ObjectId imageId = await _gridFS.UploadFromBytesAsync(fileName, image);

                photo.ImageSize = image.Length.ToString();

                //обновляем данные по дуомукументу

                photo.ImageSourceId = imageId.ToString();
                var filter = Builders<CategoryPhoto>.Filter.Eq("CategoryId", photo.CategoryId);
                var update = Builders<CategoryPhoto>.Update.Set("ImageSourceId", photo.ImageSourceId).Set("ImageSize", photo.ImageSize);
                await Photos.UpdateOneAsync(filter, update);
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(StoreImage)}: StoreImage is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(StoreImage)}: Error");
                throw;
            }
        }

        public async Task<byte[]> GetImageSource(CategoryPhoto photo)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(GetImageSource)}: GetImageSource is starting");
                var result =  await _gridFS.DownloadAsBytesAsync(new ObjectId(photo.ImageSourceId));
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(GetImageSource)}: GetImageSource is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(GetImageSource)}: Error");
                throw;
            }
        }

        public string GetSizeOfImageSource(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(GetSizeOfImageSource)}: GetSizeOfImageSource is starting");
                var photo = GetPhotoById(itemId).GetAwaiter().GetResult().FirstOrDefault() ?? new CategoryPhoto();
                var result =  photo.ImageSize ?? "0";
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(GetSizeOfImageSource)}: GetSizeOfImageSource is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(GetSizeOfImageSource)}: Error");
                throw;
            }
        }

        public void InsertPhotoAndImageSource(string itemId, byte[] image)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: InsertPhotoAndImageSource is starting");
                Create(itemId).GetAwaiter().GetResult();
                StoreImage(new CategoryPhoto { CategoryId = itemId, ImageSize = image.Length.ToString() }, image).GetAwaiter().GetResult();
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: InsertPhotoAndImageSource is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: Error");
                throw;
            }
        }

        public bool IsFileExist(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(IsFileExist)}: IsFileExist");
                var photo = GetPhotoById(itemId).GetAwaiter().GetResult().FirstOrDefault() ?? new CategoryPhoto();
                var result =  photo.HasImageSource();
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(IsFileExist)}: IsFileExist is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(IsFileExist)}: Error");
                throw;
            }
        }

        private async Task CreateProductIdIndex()
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(CreateProductIdIndex)}: CreateProductIdIndex");
                var collection = _database.GetCollection<CategoryPhoto>("Photos");
                var builder = Builders<CategoryPhoto>.IndexKeys.Ascending(x => x.CategoryId);
                await collection.Indexes.CreateOneAsync(builder);
                Log.Instance.LogAsInfo($"{nameof(CategoryPhotoRepository)}.{nameof(CreateProductIdIndex)}: CreateProductIdIndex is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(CategoryPhotoRepository)}.{nameof(CreateProductIdIndex)}: Error");
                throw;
            }
        }
    }
}
