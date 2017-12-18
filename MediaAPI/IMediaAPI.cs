namespace MediaAPI
{
    public interface IMediaAPI
    {
        byte[] GetPhotoByProductId(int id);

        void InsertPhotoOfProduct(int productId, byte[] image);

        int GetPhotoSize (int productId);

        void DeletePhotoOfProduct(int productId);
    }
}
