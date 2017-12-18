using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using MediaAPI;

namespace FileTransferServiceNS.Contracts
{
    [MessageContract]
    public class MediaRequest
    {
        [MessageHeader]
        public int MediaId { get; set; }

        [MessageHeader]
        public MediaTypes MediaType { get; set; }
    }
}
