using Newtonsoft.Json;

namespace XcelerateGames.Socket
{
    /// <summary>
    /// Class to desrialise messages sent from server. If success is true, we extract **data** node & parse it based on *type* of packet
    /// If success is false, error will be used to pint the error message
    /// </summary>
    public class SM_PacketBase : PacketBase
    {
        [JsonProperty("success")] public bool success { get; set; }       /**<true if command processed successfully */
        [JsonProperty("seqid")] public int sequenceId { get; set; }       /**<Packet sequence ID, at this point, its not being used */
        [JsonProperty("error")] public PacketError error { get; set; }    /**<If there was any error processing the command, this data structure will have error code & reason for failure */
        [JsonProperty] public MetaData meta { get; set; }                 /**<Every response from server will have MetaData */
        [JsonProperty] public long socketId { get; set; }                 /**<Every response from server will have MetaData */
        [JsonProperty("table_id")] public long tableId { get; set; }      /**< Table Id will be set only in case of server error**/

        public bool HasError => error != null && !string.IsNullOrEmpty(error.code);                   /**<Helper property to check if server responded with an error */
    }

    /// <summary>
    /// Base class for all packets being sent & received
    /// </summary>
    public class PacketBase
    {
        [JsonProperty("type")] public int type { get; set; }    /**<Type of command */
    }

    /// <summary>
    /// Meta data, every server response will have meta data
    /// </summary>
    public class MetaData
    {
        [JsonProperty] public long ts { get; private set; } /**<time stamp in seconds */
    }
}
