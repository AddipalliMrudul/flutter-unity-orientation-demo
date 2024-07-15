using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XcelerateGames
{
    /// <summary>
    /// This class is added to fix the following exception in Unity Json lib
    /// Exception: No parameterless constructor defined for 'Newtonsoft.Json.Converters.StringEnumConverter'
    /// </summary>
    public class XGStringEnumConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch(Exception ex)
            {
#if LIVE_BUILD
                UnityEngine.Debug.Log($"Failed to parse enum of type {objectType} : {ex.Message}");
#else
                XDebug.LogError($"Failed to parse enum of type {objectType} : {ex.Message}");
#endif
                return Utilities.GetDefault(objectType);
            }
        }
    }
}
