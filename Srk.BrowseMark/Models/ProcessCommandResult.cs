
namespace Srk.BrowseMark.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    [DataContract]
    public class ProcessCommandResult
    {
        [DataMember]
        public Guid? Id { get; set; }

        [DataMember]
        public bool Success { get; set; }

        public static ProcessCommandResult FromRequest(ProcessCommandRequest request, bool success)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var item = new ProcessCommandResult();
            item.Id = request.Id;
            item.Success = success;
            return item;
        }

        public string Code { get; set; }
    }
}
