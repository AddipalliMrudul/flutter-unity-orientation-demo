#if FB_ENABLED

using Newtonsoft.Json;

namespace XcelerateGames.Social.Facebook
{
    public class FBUserData
    {
        [JsonProperty] public string id { get; private set; }
        [JsonProperty] public string first_name { get; private set; }
        [JsonProperty] public string last_name { get; private set; }
        [JsonProperty] public FBUserDataPicture picture { get; private set; }
    }

    public class FBUserDataPicture
    {
        [JsonProperty] public FBUserDataPictureData data { get; private set; }
    }

    public class FBUserDataPictureData
    {
        [JsonProperty] public int height { get; private set; }
        [JsonProperty] public int width { get; private set; }
        [JsonProperty] public bool is_silhouette { get; private set; }
        [JsonProperty] public string url { get; private set; }
    }

    public enum PictureType
    {
        small,  //50x37
        album,  //50x37
        square, //50x50
        normal, //100x75
        large   //200x150
    }

    public class FacebookRequest
    {
        public string ID;
        public string fromID;
        public string fromName;
        public string message;
        public string data;
        public bool isAccepted = false;
        public bool isSelected = false;
        public bool toDelete = false;
    }
 }
#endif //FB_ENABLED
