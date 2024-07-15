using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Editor.Build
{
    public class UpdateVersionListHash
    {
        [MenuItem(Utilities.MenuName + "Build/Update VersionList Hash")]
        public static void DoUpdateVersionListHash()
        {
            //mURL = BuildAppSettings.pInstance.LocalHostUrl + "addVersionHash";
            string data = GetData();
            WebRequestHandler webRequest = new WebRequestHandler(BuildAppSettings.UpdateVersionHashAPI, UpdateHashOnSuccess, UpdateHashOnFail, null);
            webRequest.Request.Headers = new WebHeaderCollection();
            //webRequest.Request.Headers.Add("X-BADV-EXT-AUTH", FileUtilities.GetMD5OfText(data + SecretKey));
            webRequest.Run(data);
        }

        private static string GetData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();//
            data.Add("platform", PlatformUtilities.GetCurrentPlatform().ToString());
            data.Add("app_version_major", ProductSettings.GetProductVersion(0).ToString());
            data.Add("app_version_minor", ProductSettings.GetProductVersion(1).ToString());
            data.Add("build_number", ProductSettings.pInstance._BuildNumber);
            data.Add("app", ProductSettings.pInstance._AppName);
            data.Add("env", PlatformUtilities.GetEnvironment().ToString());

            string hash = FileUtilities.GetMD5OfFile(EditorUtilities.mAssetsDir + ResourceManager.mAssetVersionListFileName);
            data.Add("hash", hash);
            return data.ToJson();
        }

        private static void UpdateHashOnFail(string obj)
        {
            Debug.LogError($"UpdateHashOnFail : {obj}");
        }

        private static void UpdateHashOnSuccess(string obj, WebHeaderCollection responseHeaders)
        {
            Debug.Log($"UpdateHashOnSuccess : {obj}");
        }
    }
}