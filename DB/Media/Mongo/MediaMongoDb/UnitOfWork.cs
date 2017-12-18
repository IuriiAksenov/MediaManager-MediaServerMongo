using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaMongoDb.Repositories;

namespace MediaMongoDb
{
    public class UnitOfWork
    {
        private readonly MediaContext _db;
        private ProductPhotoRepository _productPhotoRepository;
        private CategoryPhotoRepository _categoryPhotoRepository;
        private UserPhotoRepository _userPhotoRepository;

        public UnitOfWork() : this("") { }
        public UnitOfWork(string connectionString)
        {
            _db = connectionString == "" ? new MediaContext() : new MediaContext(connectionString);
        }
        public ProductPhotoRepository ProductPhotos => _productPhotoRepository ?? (_productPhotoRepository = new ProductPhotoRepository(_db));
        public CategoryPhotoRepository CategoryPhotos => _categoryPhotoRepository ?? (_categoryPhotoRepository = new CategoryPhotoRepository(_db));
        public UserPhotoRepository UserPhotos => _userPhotoRepository ?? (_userPhotoRepository = new UserPhotoRepository(_db));
    }
}
