using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MediaMongoDb
{
    public interface IPhotoRepository<TPhotoEntity> where TPhotoEntity : BasePhotoEntity
    {
        IMongoCollection<TPhotoEntity> Photos { get; }

        Task<IEnumerable<TPhotoEntity>> GetAllItems();

        Task<IEnumerable<TPhotoEntity>> GetPhotoById(string itemId);

        Task Create(string itemId);

        Task Update(string itemId);

        Task Delete(string productId);

        Task StoreImage(TPhotoEntity photo, byte[] image);

        Task<byte[]> GetImageSource(TPhotoEntity photo);

        string GetSizeOfImageSource(string itemId);

        void InsertPhotoAndImageSource(string itemId, byte[] image);

        bool IsFileExist(string itemId);
    }
}
