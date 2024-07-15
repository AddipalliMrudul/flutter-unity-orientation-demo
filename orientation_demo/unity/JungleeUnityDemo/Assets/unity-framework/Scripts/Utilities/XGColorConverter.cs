using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// This class handles serialization & deserialization of Color data
    /// @example [JsonConverter(typeof(XGColorConverter))] public Color colorVal;
    /// </summary>
    public class XGColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                ColorUtility.TryParseHtmlString("#" + reader.Value, out Color loadedColor);
                return loadedColor;
            }
            catch(Exception ex)
            {
                XDebug.LogError($"Failed to parse color {objectType} : {ex.Message}");
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string val = ColorUtility.ToHtmlStringRGB((Color)value);
            writer.WriteValue(val);
        }
    }
}
