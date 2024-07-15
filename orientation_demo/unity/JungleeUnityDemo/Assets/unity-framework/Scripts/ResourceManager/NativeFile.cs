using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace XcelerateGames.AssetLoading
{
    public class NativeFile : MonoBehaviour
    {
        private Action<UnityWebRequest, bool> mCallback = null;

        public static void Load(string filePath, Action<UnityWebRequest, bool> callback)
        {
            NativeFile obj = new GameObject("NativeFile").AddComponent<NativeFile>();
            filePath = "file:///" + filePath;
            obj.mCallback = callback;
            obj.StartCoroutine(obj.StartLoading(filePath));
        }

        private IEnumerator StartLoading(string filePath)
        {
            UnityWebRequest request = UnityWebRequest.Get(filePath);
            request.downloadHandler = new DownloadHandlerTexture(true);
            DownloadHandlerTexture downloadHandler = new DownloadHandlerTexture(true);

            request.disposeDownloadHandlerOnDispose = false;
            UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                yield return request;
            }
            DownloadHandlerTexture mTextureHandler = request.downloadHandler as DownloadHandlerTexture;
#if UNITY_2020_2_OR_NEWER
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
            if (request.isHttpError || request.isNetworkError)
#endif
            {
                Debug.LogException(new Exception($"NativeFile::Failed to load {filePath}"));
                mCallback.Invoke(request, false);
            }
            else
            {
                Debug.Log($"Loaded {filePath}");
                //FileUtilities.WriteToFile("loaded.jpg", mTextureHandler.data);
                mCallback.Invoke(request, true);
            }
            GameObject.Destroy(gameObject);
        }
    }
}
