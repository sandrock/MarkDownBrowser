
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
    public class ProcessCommandRequest
    {
        private static readonly DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProcessCommandRequest));

        [DataMember]
        public string Command { get; set; }

        [DataMember]
        public int? ProcessId { get; set; }
        
        [DataMember]
        public Guid? Id { get; set; }

        [DataMember]
        public string Value { get; set; }

        public static ProcessCommandRequest FromJson(string json)
        {
            if (json == null)
                throw new ArgumentNullException("json");
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("The value cannot be empty", "json");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var obj = serializer.ReadObject(stream);
                return (ProcessCommandRequest)obj;
            }
        }

        public override string ToString()
        {
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, this);
                stream.Seek(0L, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var json = reader.ReadToEnd();
                    return json;
                }
            }
        }

        internal static ProcessCommandRequest OpenFile(string file, int? processId)
        {
            var item = new ProcessCommandRequest();
            item.Id = Guid.NewGuid();
            item.Command = "OpenFile";
            item.ProcessId = processId;
            item.Value = file;
            return item;
        }

        internal static ProcessCommandRequest Hello(int? processId)
        {
            var item = new ProcessCommandRequest();
            item.Id = Guid.NewGuid();
            item.Command = "Hello";
            item.ProcessId = processId;
            return item;
        }

        internal static ProcessCommandRequest BringToFront(int? processId)
        {
            var item = new ProcessCommandRequest();
            item.Id = Guid.NewGuid();
            item.Command = "BringToFront";
            item.ProcessId = processId;
            return item;
        }
    }
}
