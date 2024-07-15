namespace XcelerateGames.Cryptography
{
    public static class CryptoUtilities
    {
        public static XGCrypto.Environment GetCryptoEnv()
        {
            XGCrypto.Environment environment = XGCrypto.Environment.Live;
            switch (PlatformUtilities.GetEnvironment())
            {
                case PlatformUtilities.Environment.dev:
                    environment = XGCrypto.Environment.Dev;
                    break;
                case PlatformUtilities.Environment.qa:
                    environment = XGCrypto.Environment.QA;
                    break;
                case PlatformUtilities.Environment.stage:
                    environment = XGCrypto.Environment.Staging;
                    break;
            }
            return environment;
        }

        public static string EncryptOrDecrypt(string data)
        {
            return data;
            //return XGCrypto.EncryptOrDecrypt(GetCryptoEnv(), data);
        }

        public static string GetMd5Hash(string uuid, string ticks, string text)
        {
            return XGCrypto.GetMd5Hash(GetCryptoEnv(), text, uuid, ticks);
        }
    }
}
