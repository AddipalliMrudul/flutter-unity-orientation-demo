using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class UpdateVersionListHashHistory
    {
        public class VersionHashInfo
        {
            public string buildNum { get; set; }
            public string appVersion { get; set; }
            public string hash { get; set; }
            public string PrevHash { get; set; }
            public string dateTime { get; set; }
            public string comment { get; set; }
            public SortedDictionary<string, string> uploadedAssets;

            public VersionHashInfo(string inHash, string msg, string prevHash, SortedDictionary<string, string> inUploadedAssets)
            {
                hash = inHash;
                dateTime = DateTime.Now.ToString();
                buildNum = ProductSettings.pInstance._BuildNumber;
                appVersion = ProductSettings.GetProductVersion();
                comment = msg;
                PrevHash = prevHash;
                uploadedAssets = inUploadedAssets;
            }
        }
        [MenuItem(Utilities.MenuName + "Build/Update VersionList History")]
        public static void UpdateVersionList()
        {
            Update("Zibly", "NA", null);
        }

        public static void Update(string message, string prevHash, SortedDictionary<string, string> uploadedAssets)
        {
            //Get the hash of our version list
            string hash = FileUtilities.GetMD5OfFile(EditorUtilities.mAssetsDir + "version_list.json");
            Debug.LogError("Version List hash : " + hash);

            Dictionary<PlatformUtilities.Platform, List<VersionHashInfo>> history = GetHistory();
            history[PlatformUtilities.GetCurrentPlatform()].Add(new VersionHashInfo(hash, message, prevHash, uploadedAssets));
            File.WriteAllText(GetFileName(), history.ToJson());
        }

        private static Dictionary<PlatformUtilities.Platform, List<VersionHashInfo>> GetHistory()
        {
            string mVersionListHashHistory = GetFileName();

            Dictionary<PlatformUtilities.Platform, List<VersionHashInfo>> history = null;
            if (File.Exists(mVersionListHashHistory))
                history = File.ReadAllText(mVersionListHashHistory).FromJson<Dictionary<PlatformUtilities.Platform, List<VersionHashInfo>>>();
            if (history == null)
                history = new Dictionary<PlatformUtilities.Platform, List<VersionHashInfo>>();
            if (!history.ContainsKey(PlatformUtilities.GetCurrentPlatform()))
                history.Add(PlatformUtilities.GetCurrentPlatform(), new List<VersionHashInfo>());
            return history;
        }

        private static string GetFileName()
        {
            return string.Format("version_list_hash_history_{0}.json", PlatformUtilities.GetEnvironment());
        }
    }
}