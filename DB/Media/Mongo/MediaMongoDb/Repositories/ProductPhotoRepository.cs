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
    public class ProductPhotoRepository : IPhotoRepository<ProductPhoto>
    {
        private readonly IMongoDatabase _database; // бд
        private readonly IGridFSBucket _gridFS; //файловое хранилище

        public ProductPhotoRepository(MediaContext db)
        {
            Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(ProductPhotoRepository)}: Constructor is tratring.");
            _database = db.Database;
            _gridFS = db.GridFsBucket;

            CreateProductIdIndex().GetAwaiter().GetResult();
            Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(ProductPhotoRepository)}: Constructor is ended.");
        }

        public IMongoCollection<ProductPhoto> Photos
        {
            get
            {
                try
                {
                    Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(Photos)}: Get Photos is starting");
                    var result = _database.GetCollection<ProductPhoto>("ProductPhotos");
                    Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(Photos)}: Get Photos is successfully ended.");
                    return result;
                }
                catch (Exception e)
                {
                    Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(Photos)}: Error");
                    throw;
                }

            }
        }

        public async Task<IEnumerable<ProductPhoto>> GetAllItems()
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(GetAllItems)}: GetAllItems is starting");
                var builder = new FilterDefinitionBuilder<ProductPhoto>();
                var filter = builder.Empty; //фильтр для выборки всех элементов
                var result = await Photos.Find(filter).ToListAsync();
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(GetAllItems)}: GetAllItems is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(GetAllItems)}: Error");
                throw;
            }
        }

        public async Task<IEnumerable<ProductPhoto>> GetPhotoById(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(GetPhotoById)}: GetPhotoById is starting");
                //строитель фильтров
                var builder = new FilterDefinitionBuilder<ProductPhoto>();
                var filter = builder.Empty; //фильтр для выборки всех элементов

                //фильтра по имени
                if (!String.IsNullOrWhiteSpace("ProductId"))
                {
                    filter = filter & builder.Eq("ProductId", itemId);
                }

                var result = await Photos.Find(filter).ToListAsync();
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(GetPhotoById)}: GetPhotoById is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(GetPhotoById)}: Error");
                throw;
            }
        }

        public async Task Create(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(Create)}: Create is starting");
                ProductPhoto photo = new ProductPhoto { ProductId = itemId };
                await Photos.InsertOneAsync(photo);
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(Create)}: Create is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(Create)}: Error");
                throw;
            }
        }

        public async Task Update(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(Update)}: Update is starting");
                ProductPhoto photo = new ProductPhoto { ProductId = itemId };
                await Photos.ReplaceOneAsync(new BsonDocument("ProductId", photo.ProductId), photo);
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(Update)}: Update is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(Create)}: Error");
                throw;
            }

        }

        public async Task Delete(string productId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(Delete)}: Delete is starting");
                ProductPhoto photo = new ProductPhoto { ProductId = productId };
                await Photos.DeleteOneAsync(new BsonDocument("ProductId", photo.ProductId));
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(Delete)}: Delete is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(Delete)}: Error");
                throw;
            }

        }

        public async Task StoreImage(ProductPhoto photo, byte[] image)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(StoreImage)}: StoreImage is starting. {nameof(ProductPhoto)}");
                if (photo.HasImageSource())
                {
                    //если ранее уже была фотка то удаляем её
                    photo.ImageSize = "0";
                    await _gridFS.DeleteAsync(new ObjectId(photo.ImageSourceId));
                }

                string fileName = photo.ProductId;
                ObjectId imageId = await _gridFS.UploadFromBytesAsync(fileName, image);

                photo.ImageSize = image.Length.ToString();

                //обновляем данные по дуомукументу

                photo.ImageSourceId = imageId.ToString();
                var filter = Builders<ProductPhoto>.Filter.Eq("ProductId", photo.ProductId);
                var update = Builders<ProductPhoto>.Update.Set("ImageSourceId", photo.ImageSourceId).Set("ImageSize", photo.ImageSize);
                await Photos.UpdateOneAsync(filter, update);
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(StoreImage)}: StoreImage is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(StoreImage)}: Error");
                throw;
            }
        }

        public async Task<byte[]> GetImageSource(ProductPhoto photo)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(GetImageSource)}: GetImageSource is starting");
                var result = await _gridFS.DownloadAsBytesAsync(new ObjectId(photo.ImageSourceId));
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(GetImageSource)}: GetImageSource is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(GetImageSource)}: Error");
                throw;
            }
        }

        public string GetSizeOfImageSource(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(GetSizeOfImageSource)}: GetSizeOfImageSource is starting");
                var photo = GetPhotoById(itemId).GetAwaiter().GetResult().FirstOrDefault() ?? new ProductPhoto();
                var result = photo.ImageSize ?? "0";
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(GetSizeOfImageSource)}: GetSizeOfImageSource is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(GetSizeOfImageSource)}: Error");
                throw;
            }
        }

        public void InsertPhotoAndImageSource(string itemId, byte[] image)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: InsertPhotoAndImageSource is starting");
                Create(itemId).GetAwaiter().GetResult();
                StoreImage(new ProductPhoto { ProductId = itemId, ImageSize = image.Length.ToString() }, image).GetAwaiter().GetResult();
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: InsertPhotoAndImageSource is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(InsertPhotoAndImageSource)}: Error");
                throw;
            }
        }

        public bool IsFileExist(string itemId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(IsFileExist)}: IsFileExist");
                var photo = GetPhotoById(itemId).GetAwaiter().GetResult().FirstOrDefault() ?? new ProductPhoto();
                var result = photo.HasImageSource();
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(IsFileExist)}: IsFileExist is successfully ended.");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(IsFileExist)}: Error");
                throw;
            }
        }

        private async Task CreateProductIdIndex()
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(CreateProductIdIndex)}: CreateProductIdIndex");
                var collection = _database.GetCollection<ProductPhoto>("Photos");
                var builder = Builders<ProductPhoto>.IndexKeys.Ascending(x => x.ProductId);
                await collection.Indexes.CreateOneAsync(builder);
                Log.Instance.LogAsInfo($"{nameof(ProductPhotoRepository)}.{nameof(CreateProductIdIndex)}: CreateProductIdIndex is successfully ended.");
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(ProductPhotoRepository)}.{nameof(CreateProductIdIndex)}: Error");
                throw;
            }
        }
    }
}

