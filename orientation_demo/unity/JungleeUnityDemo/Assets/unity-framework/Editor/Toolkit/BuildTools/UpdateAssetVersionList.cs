using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XcelerateGames.AssetLoading;

/*
 * Author : Altaf
 * Date : Dec 2018.
 * Purpose : Inserts hash of selected asset(s) into version list
*/

namespace XcelerateGames.Editor.Build
{
#if !UNITY_WEBPLAYER
    class UpdateAssetVersionList
    {
        //[MenuItem("Assets/Insert Asset hash")]
        //public static void GetAssetHash()
        //{
        //    SortedDictionary<string, string> assetsList = EditorUtilities.GetVersionList();
        //    foreach (Object asset in Selection.objects)
        //    {
        //        string path = AssetDatabase.GetAssetPath(asset);
        //        string hash = FileUtilities.GetMD5OfFile(path);
        //        assetsList[asset.name] = hash;
        //        Debug.LogError(path + " : " + hash);
        //    }

        //    //Now remove version_list.son fromt the list
        //    assetsList.Remove(ResourceManager.mAssetVersionListFileName);
        //    string filePath = EditorUtilities.mAssetsDir + ResourceManager.mAssetVersionListFileName;
        //    File.WriteAllText(filePath, assetsList.ToJson());
        //    Debug.LogError(filePath + " : " + FileUtilities.GetMD5OfFile(filePath));
        //}
    }
#endif //UNITY_WEBPLAYER
}