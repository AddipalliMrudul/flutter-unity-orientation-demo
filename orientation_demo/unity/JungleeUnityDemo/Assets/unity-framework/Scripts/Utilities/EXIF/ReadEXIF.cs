using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using ExifLib;

// Unity 2019 update to TestUnityExif (from https://www.codeproject.com/Articles/47486/Understanding-and-Reading-Exif-Data)
// ExifReader described here: http://www.takenet.or.jp/~ryuuji/minisoft/exifread/english/ 

namespace XcelerateGames
{
    public class ReadEXIF : MonoBehaviour
    {
        IEnumerator LoadTexture(string imagePath)
        {
            yield return StartCoroutine(LoadByteArrayIntoTexture(imagePath));
        }

        /// <summary>
        /// ExifLib - http://www.codeproject.com/Articles/47486/Understanding-and-Reading-Exif-Data
        /// </summary>
        IEnumerator LoadByteArrayIntoTexture(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.Log(www.error);
            }
            else
            {
                // retrieve results as binary data
                byte[] results = www.downloadHandler.data;

                ExifLib.JpegInfo jpi = ExifLib.ExifReader.ReadJpeg(results, "Sample File");
                Debug.Log("Finished Getting Image -> SIZE: " + results.Length.ToString());

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(results);
                //newTexture = tex;

                // Not sure why, but many images come in flipped 180 degrees
                //newTexture = rotateTexture(newTexture, true); // Rotate clockwise 90 degrees
                //newTexture = rotateTexture(newTexture, true); // Rotate clockwise 90 degrees (again, to flip it)
                //this.texture = newTexture;
            }
        }
    }
}
