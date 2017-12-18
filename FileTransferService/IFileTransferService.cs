using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using FileTransferServiceNS.Contracts;
using MediaAPI;

namespace FileTransferServiceNS
{
    [ServiceContract]
    public interface IFileTransferService
    {
        [OperationContract]
        MediaResponse IsFileExist(MediaRequest request);

        [OperationContract]
        MediaFile DownloadFile(MediaRequest request);

        [OperationContract]
        MediaResponse UploadFile(MediaFile file);
    }

    [ServiceContract]
    public interface IFileTransferServiceV2
    {
        [OperationContract]
        bool IsFileExist(MediaTypes type, int id);

        [OperationContract]
        MediaFile DownloadFile(MediaRequest request);

        [OperationContract]
        void UploadFile(UserAvatarData data);
    }

    [DataContract]
    public class UserAvatarData
    {
        [DataMember] public byte[] image;

        [DataMember] public int userId;
    }
}
