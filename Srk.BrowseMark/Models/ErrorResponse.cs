
namespace Srk.BrowseMark.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    public class ErrorResponse
    {
        public string Code { get; set; }
    }
}
