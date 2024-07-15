using UnityEngine;
using XcelerateGames;
using XcelerateGames.AssetLoading;
using XcelerateGames.Cryptography;

/*Attach this script to a GameObject in scene & set a proper URL.
 * Note: Make sure ResourceManager is intialized
 */
#if UNITY_EDITOR
public class AESCryptographyTest : MonoBehaviour
{
    public string _Url;
    public bool _Encrypt = true;
    //Length must be 16 or 32
    public string _Key = null;
    //Initialization Vector, Length must be 16
    public string _IV = null;
    byte[] key = null;
    byte[] iv = null;

    void Start()
    {
        key = System.Text.Encoding.UTF8.GetBytes(_Key);
        iv = System.Text.Encoding.UTF8.GetBytes(_IV);

        //key = Convert.FromBase64String(_Key);
        //iv = Convert.FromBase64String(_IV);
        Debug.LogError($"{key.Length}, {iv.Length}");


        ResourceManager.LoadURL(_Url, OnTextureLoaded, ResourceManager.ResourceType.Texture);
    }

    private void OnTextureLoaded(ResourceEvent inEvent, string arg2, object arg3, object arg4)
    {
        if (inEvent == ResourceEvent.COMPLETE)
        {
            byte[] bytes = ResourceManager.GetBytes(_Url, true);
            if(_Encrypt)
                bytes = AesCryptography.Encrypt(bytes, key, iv);
            FileUtilities.WriteToFile("encrypted.png", bytes);

            if(_Encrypt)
            {
                bytes = AesCryptography.Decrypt(bytes, key, iv);
                FileUtilities.WriteToFile("decrypted.png", bytes);
            }
        }
        else if (inEvent == ResourceEvent.ERROR)
            Debug.LogError($"Failed to load {_Url}");
    }
}
#endif