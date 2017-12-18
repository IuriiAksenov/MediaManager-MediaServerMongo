using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FileTransferServiceNS.Contracts
{
    [MessageContract]
    public class MediaResponse
    {

        public MediaResponse(bool isSuccessful, string errorMessage) : this(isSuccessful, errorMessage, null){}
        public MediaResponse(bool isSuccessful, string errorMessage, Object result)
        {
            if (errorMessage == null || (errorMessage == null && !isSuccessful))
                throw new ArgumentException(
                    "You should pass correct error. " +
                    "(Error.NoError for successful operation and new Error for unsuccessful).");

            ErrorMessage = errorMessage;
            IsSuccessful = isSuccessful;
            Result = result;
        }

        public static MediaResponse NoError => new MediaResponse(true, "Operation was successful.");
        public static MediaResponse NoErrorWithResult(Object result) => new MediaResponse(true, "Operation was successful.", result);
        public static MediaResponse Error => new MediaResponse(false, "Operation was in trouble.");
        public static MediaResponse ErrorWithMessage(String errorMessage) => new MediaResponse(true, "Operation was in trouble. " + errorMessage);

        [MessageBodyMember]
        public Boolean IsSuccessful { get; private set; }

        [MessageBodyMember]
        public String ErrorMessage { get; private set; }

        [MessageBodyMember]
        public Object Result { get; private set; }
    }
}
