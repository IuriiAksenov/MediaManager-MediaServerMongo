using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaAPI;

namespace MediaSQL
{
    public class UnitOfWorkPhotoStorageSQL : IUOWPhotoStorage
    {

        private readonly string _connectionString;
        private IPhotoStorage _productPhotoStorage;
        private IPhotoStorage _categoryPhotoStorage;
        private IPhotoStorage _userPhotoStorage;

        public UnitOfWorkPhotoStorageSQL() : this("") { }
        public UnitOfWorkPhotoStorageSQL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IPhotoStorage ProductPhotosStorage =>
            _productPhotoStorage ?? (_productPhotoStorage = new BigPhotoStorage(_connectionString));

        public IPhotoStorage CategoryPhotosStorage =>
            _categoryPhotoStorage ?? (_categoryPhotoStorage = new CategoryPhotoStorage(_connectionString));
        public IPhotoStorage UserPhotosStorage => _userPhotoStorage ?? (_userPhotoStorage = new UserPhotoStorage(_connectionString));

    }
}
