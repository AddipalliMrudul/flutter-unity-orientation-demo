using Newtonsoft.Json.Converters;


namespace XcelerateGames
{
    /// <summary>
    /// This class is used to control the way DateTime object is serialized & Deserialized
    /// Ex 1: [JsonProperty] [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")] public DateTime? birthday { get; set; }
    /// Ex 2: [JsonProperty] [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd:hh:mm:ss")] public DateTime? birthday { get; set; }
    /// with GMT offset
    /// Ex 3: [JsonProperty][JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddTHH:mm:ss+zzz")] public DateTime nextRefillTime { get; private set; }
    /// 
    /// "hh"	The hour, using a 12-hour clock from 01 to 12.
    /// "HH"	The hour, using a 24-hour clock from 00 to 23.
    /// https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
    /// </summary>
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            base.DateTimeFormat = format;
        }
    }
}
