using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaAPI;
using MediaMongoDb;
using MediaMongoDb.Repositories;

namespace MediaMongo
{
    public class UnitOfWorkPhotoStorageMongo : IUOWPhotoStorage
    {
        private readonly MediaContext _db;
        private IPhotoStorage _productPhotoStorage;
        private IPhotoStorage _categoryPhotoStorage;
        private IPhotoStorage _userPhotoStorage;

        public UnitOfWorkPhotoStorageMongo() : this("") { }
        public UnitOfWorkPhotoStorageMongo(string connectionString)
        {
            _db = connectionString == "" ? new MediaContext() : new MediaContext(connectionString);
        }
        public IPhotoStorage ProductPhotosStorage => _productPhotoStorage ?? (_productPhotoStorage = new PhotoStorageMongo<ProductPhoto>(new ProductPhotoRepository(_db)));
        public IPhotoStorage CategoryPhotosStorage => _categoryPhotoStorage ?? (_categoryPhotoStorage= new PhotoStorageMongo<CategoryPhoto>(new CategoryPhotoRepository(_db)));
        public IPhotoStorage UserPhotosStorage => _userPhotoStorage ?? (_userPhotoStorage = new PhotoStorageMongo<UserPhoto>(new UserPhotoRepository(_db)));
    }
}
