
namespace Srk.BrowseMark.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    public class ApiClientException : InvalidOperationException
    {
        public ApiClientException()
        {
        }

        public ApiClientException(string message)
            : base(message)
        {
        }

        public ApiClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ApiClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


    }
}
