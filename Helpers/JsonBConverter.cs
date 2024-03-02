using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace OwlApi.Helpers
{
    public class JsonBConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                JToken t = JToken.FromObject(value);
                t.WriteTo(writer);
            }
            else
            {
                string s = (string)value;
                JToken token = JToken.Parse(s);
                serializer.Serialize(writer, token.ToObject(typeof(T)));
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, token.ToObject(typeof(T)));
            return writer.ToString();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}
