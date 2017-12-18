using System.Drawing;

namespace Media
{
    public interface IPhotoStorage
    {
        bool CheckExistingImage(int productId);
        Image GetImageByProductId(int productId);
        byte[] GetImageBytesByProductId(int productId);
    }
}