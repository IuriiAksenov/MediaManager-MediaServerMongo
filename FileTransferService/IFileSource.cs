using System;
using System.IO;
using System.ServiceModel;
using FileTransferServiceNS.Contracts;

namespace FileTransferServiceNS
{
    public interface IFileSource
    {
        MediaResponse IsFileExists(MediaRequest request);
        MediaFile DownloadFile(MediaRequest request);
        MediaResponse UploadFile(MediaFile file);
        void UploadUserAva(byte[] image, int userid);
    }
}