using System;
using System.IO;
using System.ServiceModel;
using FileTransferServiceNS.Contracts;
using MediaAPI;

namespace FileTransferServiceNS
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class FileTransferService : IFileTransferService, IFileTransferServiceV2
    {
        private readonly IFileSource _fileSource;

        public FileTransferService(IFileSource source)
        {
            _fileSource = source;
        }


        public MediaResponse IsFileExist(MediaRequest request)
        {
            return _fileSource.IsFileExists(request);
        }

        public MediaFile DownloadFile(MediaRequest request)
        {
            return _fileSource.DownloadFile(request);
        }

        public MediaResponse UploadFile(MediaFile file)
        {
            return _fileSource.UploadFile(file);
        }

        public bool IsFileExist(MediaTypes type, int id)
        {
            return (bool)_fileSource.IsFileExists(new MediaRequest() { MediaType = type, MediaId = id }).Result;
        }

        public void UploadFile(UserAvatarData data)
        {
            _fileSource.UploadUserAva(data.image, data.userId);
        }
    }
}
