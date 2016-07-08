
namespace Srk.BrowseMark.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    [DataContract]
    public class DebugInfo
    {
        [DataMember]
        public int ProcessId { get; set; }

        [DataMember]
        public int ProcessSessionId { get; set; }
    }
}
