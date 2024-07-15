namespace XcelerateGames
{
    //Reference https://en.wikipedia.org/wiki/Media_type
    public static class MimeType
    {
        public const string IMAGES = "image/*";
        public const string JPEG = "image/jpeg";
        public const string PNG = "image/png";
        public const string JSON = "application/json";
        public const string PDF = "application/pdf";
        public const string VIDEO = "video/mp4";
        public const string AUDIO = "audio/ogg";
        public const string ANY = "*/*";
    }

    public enum MimeTypeId
    {
        None,
        Images,
        Jpeg,
        Jpg,
        Png,
        Pdf,
        Video,
        Audio,
    }
}
