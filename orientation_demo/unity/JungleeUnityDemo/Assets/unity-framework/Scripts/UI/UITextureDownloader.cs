using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.AssetLoading;

namespace XcelerateGames
{
    [RequireComponent(typeof(RawImage))]
    public class UITextureDownloader : MonoBehaviour
    {
        [SerializeField] private string _URL = null;

        private void OnTextureLoaded(ResourceEvent inEvent, string inURL, object inObject, object inUserData)
        {
            if (inEvent == ResourceEvent.COMPLETE)
            {
                _URL = inURL;
                RawImage tex = GetComponent<RawImage>();
                if(tex != null)
                    tex.texture = inObject as Texture;
            }
            else if (inEvent == ResourceEvent.ERROR)
                Debug.LogError("Error loading Texture :" + inURL);
        }

        public void Set(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (url.StartsWith("http"))
                    ResourceManager.LoadURL(url, OnTextureLoaded, ResourceManager.ResourceType.Texture);
                else//Load form bundle
                    ResourceManager.Load(url, OnTextureLoaded, ResourceManager.ResourceType.Object);
            }
        }

        private void Start()
        {
            Set(_URL);
        }
    }
}