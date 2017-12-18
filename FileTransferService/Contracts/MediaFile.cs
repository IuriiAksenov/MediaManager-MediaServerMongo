using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using MediaAPI;

namespace FileTransferServiceNS.Contracts
{
    [MessageContract]
    public class MediaFile : IDisposable
    {
        [MessageHeader]
        public int MediaId { get; set; }

        [MessageHeader]
        public MediaTypes MediaType { get; set; }

        [MessageHeader]
        public long Size { get; set; }

        [MessageBodyMember(Order = 1)]
        public System.IO.Stream FileByteStream { get; set; }

        public void Dispose()
        {
            if (FileByteStream == null) return;
            FileByteStream.Close();
            FileByteStream = null;
        }

    }
}
