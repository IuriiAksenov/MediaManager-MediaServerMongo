using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaAPI
{
    public interface IUOWPhotoStorage
    {
        IPhotoStorage ProductPhotosStorage { get; }
        IPhotoStorage CategoryPhotosStorage { get; }
        IPhotoStorage UserPhotosStorage { get; }

    }
}
