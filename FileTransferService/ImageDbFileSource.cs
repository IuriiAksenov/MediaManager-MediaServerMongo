using System;
using System.IO;
using System.Runtime.Serialization;
using FileTransferServiceNS.Contracts;
using MediaAPI;
using Logger;

namespace FileTransferServiceNS
{

    public class ImageDbFileSource : IFileSource
    {
        private readonly IUOWPhotoStorage _uowPhotoMongo;
        private readonly IUOWPhotoStorage _uowPhotoSQL;

        public ImageDbFileSource(
            IUOWPhotoStorage uowPhotoStorageMongo,
            IUOWPhotoStorage uowPhotoStorageSQL)
        {
            _uowPhotoMongo = uowPhotoStorageSQL;
            _uowPhotoSQL = uowPhotoStorageSQL;

        }
        
        public MediaResponse IsFileExists(MediaRequest request)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(ImageDbFileSource)}.{nameof(IsFileExists)}: IsFileExists is started");
                bool result;
                if (request.MediaId % 2 == 0)
                {
                    result = IsFileExists(_uowPhotoSQL, request.MediaType, request.MediaId);

                }
                else
                {
                    result = IsFileExists(_uowPhotoMongo, request.MediaType, request.MediaId);
                }
                
                Log.Instance.LogAsInfo($"{nameof(ImageDbFileSource)}.{nameof(IsFileExists)}: IsFileExists is successfully completed");

                return MediaResponse.NoErrorWithResult(result);
            }
            catch (Exception ex)
            {
                Log.Instance.ExceptionInfo(ex).LogAsInfo($"{nameof(ImageDbFileSource)}.{nameof(IsFileExists)}: Error");
                return MediaResponse.ErrorWithMessage(ex.Message);
            }
        }



        public MediaFile DownloadFile(MediaRequest request)
        {

            try
            {
                Log.Instance.LogAsInfo($"{nameof(ImageDbFileSource)}.{nameof(DownloadFile)}: DownloadFile is started");
                byte[] image;
                if (request.MediaId % 2 == 0)
                {
                    image = DownloadFile(_uowPhotoSQL, request.MediaType, request.MediaId);
                }
                else
                {
                    image = DownloadFile(_uowPhotoMongo, request.MediaType, request.MediaId);
                }
                
                MediaFile result = new MediaFile()
                {
                    MediaId = request.MediaId,
                    MediaType = request.MediaType,
                    FileByteStream = new MemoryStream(image),
                    Size = image.LongLength
                };
                Log.Instance.LogAsInfo($"{nameof(ImageDbFileSource)}.{nameof(DownloadFile)}: DownloadFile is successfully completed");
                return result;
            }
            catch (Exception ex)
            {
                Log.Instance.ExceptionInfo(ex).LogAsInfo($"{nameof(ImageDbFileSource)}.{nameof(DownloadFile)}: Error");
                return new MediaFile()
                {
                    MediaId = -1
                };
            }



        }

        public MediaResponse UploadFile(MediaFile file)
        {
            try
            {
                Log.Instance.LogAsInfo(nameof(ImageDbFileSource), nameof(UploadFile), "UploadFile is started");
                bool result;
                Log.Instance.LogAsInfo(nameof(ImageDbFileSource), nameof(UploadFile), "Convert Stream to bytes array");

                
                long size = file.Size;
                byte[] fileBytes = new byte[size];
                int bytesRead = 0;
                do
                {
                    bytesRead = file.FileByteStream.Read(fileBytes, bytesRead, (int)size-bytesRead);
                } while (bytesRead > 0);

                Log.Instance.LogAsInfo(nameof(ImageDbFileSource), nameof(UploadFile), "Convert Stream to bytes array is complet");

                if (file.MediaId % 2 == 0)
                {
                    result = UploadFile(_uowPhotoSQL, fileBytes, file.MediaType, file.MediaId);
                    
                }
                else
                {
                    result = UploadFile(_uowPhotoMongo, fileBytes, file.MediaType, file.MediaId);
                }
                
                Log.Instance.LogAsInfo(nameof(ImageDbFileSource), nameof(UploadFile), "UploadFile is successfully completed");
                return MediaResponse.NoErrorWithResult(result);
            }
            catch (Exception ex)
            {
                Log.Instance.ExceptionInfo(ex).LogAsInfo(nameof(ImageDbFileSource), nameof(UploadFile), " Error");
                return MediaResponse.ErrorWithMessage(ex.Message);
            }
            
        }

        private bool IsFileExists(IUOWPhotoStorage uowPhotoStorage, MediaTypes mediaTypes, int mediaId)
        {
            switch (mediaTypes)
            {
                case MediaTypes.ProductPhoto:
                    return uowPhotoStorage.ProductPhotosStorage.IsImageExist(mediaId);
                case MediaTypes.CategoryPhoto:
                    return uowPhotoStorage.CategoryPhotosStorage.IsImageExist(mediaId);
                case MediaTypes.UserPhoto:
                    return uowPhotoStorage.UserPhotosStorage.IsImageExist(mediaId);
                default:
                    throw new InvalidCastException("Incorrect media type");
            }
        }

        private byte[] DownloadFile(IUOWPhotoStorage uowPhotoStorage, MediaTypes mediaTypes, int mediaId)
        {
            switch (mediaTypes)
            {
                case MediaTypes.ProductPhoto:
                    return uowPhotoStorage.ProductPhotosStorage.GetImageBytesById(mediaId) ?? new byte[0];
                case MediaTypes.CategoryPhoto:
                    return uowPhotoStorage.CategoryPhotosStorage.GetImageBytesById(mediaId) ?? new byte[0];
                case MediaTypes.UserPhoto:
                    return uowPhotoStorage.UserPhotosStorage.GetImageBytesById(mediaId) ?? new byte[0];
                default:
                    throw new InvalidCastException("Incorrect media type");
            }
        }

        private bool UploadFile(IUOWPhotoStorage uowPhotoStorage, byte[] image, MediaTypes mediaTypes, int mediaId)
        {
            //Log.Instance.LogAsInfo($"{nameof(ImageDbFileSource)}.{nameof(UploadFile)}: UploadFile is started");
            switch (mediaTypes)
            {
                case MediaTypes.ProductPhoto:
                    return uowPhotoStorage.ProductPhotosStorage.AddOrUpdateFile(image, mediaId);
                case MediaTypes.CategoryPhoto:
                    return uowPhotoStorage.CategoryPhotosStorage.AddOrUpdateFile(image, mediaId);
                case MediaTypes.UserPhoto:
                    return uowPhotoStorage.UserPhotosStorage.AddOrUpdateFile(image, mediaId);
                default:
                    throw new InvalidCastException("Incorrect media type");
            }
        }

        public void UploadUserAva(byte[] image, int userid)
        {
            _uowPhotoSQL.UserPhotosStorage.AddOrUpdateFile(image, userid);
        }
    }
}