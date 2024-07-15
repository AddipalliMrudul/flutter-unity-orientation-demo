using Newtonsoft.Json;

namespace XcelerateGames.AssetLoading
{
    public class AssetData
    {
        [JsonProperty] public string hash { get; set; }
        //In bytes
        [JsonProperty] public ulong size { get; set; }
        [JsonProperty] public string module { get; set; }

        public AssetData()
        {

        }

        public AssetData(string hash, ulong size, string module)
        {
            this.hash = hash;
            this.size = size;
            this.module = module;
        }

#if UNITY_EDITOR
        public static AssetData Create(string filePath, string moduleName)
        {
            return new AssetData()
            {
                hash = FileUtilities.GetMD5OfFile(filePath),
                size = FileUtilities.GetFileSize(filePath),
                module = moduleName
            };
        }
#endif
    }
}
