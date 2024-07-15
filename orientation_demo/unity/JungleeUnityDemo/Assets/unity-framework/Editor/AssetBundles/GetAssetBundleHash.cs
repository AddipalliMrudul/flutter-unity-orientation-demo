using XcelerateGames;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Author : Altaf
/// Date : July 3, 2017
/// Purpose : This script dumps hash for all asset bundles.
/// </summary>

namespace XcelerateGames.Editor.AssetBundles
{
    internal class GetAssetBundleHash : ScriptableWizard
    {
        [MenuItem(BuildAssetBundle.AssetBundleMenu + "Get All AssetBundle Hash", false, 33)]
        public static void DoGetAssetBundleHash()
        {
            AssetBundleManifest assetBundleManifest = EditorUtilities.LoadManifest();
            List<string> allBundles = new List<string>(Directory.GetFiles(EditorUtilities.mAssetsDir, "*.unity3d"));
            StreamWriter fOut = new StreamWriter("AssetBundleHash.txt");
            ;
            for (int i = 0; i < allBundles.Count; ++i)
            {
                string fileName = Path.GetFileName(allBundles[i]);
                string data = fileName + " : " + assetBundleManifest.GetAssetBundleHash(fileName);
                Debug.Log(data);
                fOut.WriteLine(data);
            }

            fOut.Close();
        }
    }
}