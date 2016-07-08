
namespace Srk.BrowseMark.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Text;

    public static class JsonConvertEx
    {
        private static readonly Dictionary<string, DataContractJsonSerializer> serializers = new Dictionary<string, DataContractJsonSerializer>();

        public static T DeserializeObject<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            var type = typeof(T);

            var serializer = GetSerializer(type);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var obj = serializer.ReadObject(stream);
                return (T)obj;
            }
        }

        public static string Serialize(object graph)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");

            var type = graph.GetType();

            var serializer = GetSerializer(type);

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, graph);
                stream.Seek(0L, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var json = reader.ReadToEnd();
                    return json;
                }
            }
        }

        private static DataContractJsonSerializer GetSerializer(Type type)
        {
            DataContractJsonSerializer serializer = null;

            string key = type.FullName;
            if (serializers.ContainsKey(key))
            {
                serializer = serializers[key];
            }

            if (serializer == null)
            {
                serializer = new DataContractJsonSerializer(type);
                serializers.Add(key, serializer);
            }

            return serializer;
        }
    }
}
