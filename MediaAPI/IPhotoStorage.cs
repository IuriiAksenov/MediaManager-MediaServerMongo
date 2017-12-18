using System.Drawing;
using System.IO;

namespace MediaAPI
{
    public interface IPhotoStorage
    {
        bool IsImageExist(int id);
        byte[] GetImageBytesById(int id);
        long GetImageSize(int id);
        bool AddOrUpdateFile(byte[] file, int id);
    }
}