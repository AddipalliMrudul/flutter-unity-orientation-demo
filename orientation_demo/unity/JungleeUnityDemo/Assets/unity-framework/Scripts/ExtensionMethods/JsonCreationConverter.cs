using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/*
 * Reference: https://graymattertutorials.wordpress.com/2018/01/15/serialization-with-json-net-in-unity-3d/
 */

namespace XcelerateGames
{
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        public override bool CanWrite { get { return false; } }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject jObject = JObject.Load(reader);
            T target = Create(objectType, jObject);

            if (target != null)
            {
                serializer.Populate(jObject.CreateReader(), target);
            }

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary> 
        /// Create an instance of objectType, based properties in the JSON object 
        /// </summary> 
        /// <param name="objectType">type of object expected</param> 
        /// <param name="jObject">contents of JSON object that will be 
        /// deserialized</param> 
        /// <returns></returns> 
        protected abstract T Create(Type objectType, JObject jObject);
    }
}
