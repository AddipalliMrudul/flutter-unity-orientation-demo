using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Editor.Build
{

    public class AnalyzeBuildSize : EditorWindow
    {
        public class AssetInfo : IComparable
        {
            public string _Name = null;
            public long _Size = 0;

            public AssetInfo(string name, long size)
            {
                _Name = name;
                _Size = size;
            }

            public int CompareTo(object obj)
            {
                AssetInfo ai = obj as AssetInfo;
                return ai._Size.CompareTo(_Size);
            }
        }

        private string mBundledAssetsInfo = null;
        private string mDownloadedAssetsInfo = null;
        private List<string> mPreftechList = null;
        private Dictionary<string,string> mShippedAssets = null;
        private List<AssetInfo> mBundledAssets = new List<AssetInfo>();
        private List<AssetInfo> mDownloadableAssets = new List<AssetInfo>();
        private Vector2 mScroll = Vector2.zero;

        [MenuItem(Utilities.MenuName + "Build/Analyze Build Size")]
        private static void CreateWizard()
        {
            AnalyzeBuildSize obj = GetWindow<AnalyzeBuildSize>();
            obj.titleContent.text = "Analyze Build Size";
            obj.OnWizardCreate();
        }

        private void OnWizardCreate()
        {
            GetBundledAssetsSize();
            GetDownloadAssetsSize();
        }

        private long GetBundledAssetsSize()
        {
            long size = 0;
            mShippedAssets = ResourceManager.GetDefaultVersionList();
            mBundledAssets.Clear();
            foreach (string asset in mShippedAssets.Keys)
            {
                if (File.Exists(EditorUtilities.mAssetsDir + asset))
                {
                    FileInfo fInfo = new FileInfo(EditorUtilities.mAssetsDir + asset);
                    size += fInfo.Length;
                    mBundledAssets.Add(new AssetInfo(asset, fInfo.Length));
                }
            }
            mBundledAssets.Sort();
            mBundledAssetsInfo = string.Format("Files : {0}, Size : {1}", mShippedAssets.Count, EditorUtility.FormatBytes(size));
            return size;
        }

        private long GetDownloadAssetsSize()
        {
            mPreftechList = ShipAssetWithApp.GetSecondaryPrefetchList();
            long size = 0;
            mDownloadableAssets.Clear();
            foreach (string asset in mPreftechList)
            {
                if (File.Exists(EditorUtilities.mAssetsDir + asset))
                {
                    FileInfo fInfo = new FileInfo(EditorUtilities.mAssetsDir + asset);
                    size += fInfo.Length;
                    mDownloadableAssets.Add(new AssetInfo(asset, fInfo.Length));
                }
            }
            mDownloadableAssets.Sort();
            mDownloadedAssetsInfo = string.Format("Files : {0}, Size : {1}", mPreftechList.Count, EditorUtility.FormatBytes(size));
            return size;
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Bundled Assets");
            GUILayout.Label(mBundledAssetsInfo);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Downloadable Assets");
            GUILayout.Label(mDownloadedAssetsInfo);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Export as CSV");
            if(GUILayout.Button("Export"))
            {
                string path = EditorUtility.SaveFilePanel("Build Size Analysis", string.Empty, FileUtilities.GetTimeStamp("BuildSizeAnalysis.csv"), "csv");
                if(!string.IsNullOrEmpty(path))
                {
                    Export(path);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            mScroll = GUILayout.BeginScrollView(mScroll);
            GUILayout.Label("Bundled Assets", EditorStyles.boldLabel, new GUILayoutOption[0] { });
            int index = 1;
            foreach (AssetInfo asset in mBundledAssets)
            {
                GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
                GUILayout.Label(index++ + ".", GUILayout.Width(30f));
                GUILayout.Button(asset._Name, "TextField", GUILayout.Height(20f));
                GUILayout.Label(EditorUtility.FormatBytes(asset._Size), GUILayout.Width(80));
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Downloadable Assets", EditorStyles.boldLabel, new GUILayoutOption[0] { });
            index = 1;
            foreach (AssetInfo asset in mDownloadableAssets)
            {
                GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));
                GUILayout.Label(index++ + ".", GUILayout.Width(30f));
                GUILayout.Button(asset._Name, "TextField", GUILayout.Height(20f));
                GUILayout.Label(EditorUtility.FormatBytes(asset._Size), GUILayout.Width(80));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void Export(string filePath)
        {
            int index = 1;
            List<string> data = new List<string>();
            data.Add(string.Format("Bundled Assets, {0}", EditorUtility.FormatBytes(GetBundledAssetsSize())));
            foreach (AssetInfo asset in mBundledAssets)
            {
                data.Add(string.Format("{0}, {1}, {2}", index++, asset._Name, EditorUtility.FormatBytes(asset._Size)));
            }

            data.Add(string.Format("\n\nDownloadable Assets, {0}", EditorUtility.FormatBytes(GetDownloadAssetsSize())));
            index = 1;
            foreach (AssetInfo asset in mDownloadableAssets)
            {
                data.Add(string.Format("{0}, {1}, {2}", index++, asset._Name, EditorUtility.FormatBytes(asset._Size)));
            }

            File.WriteAllLines(filePath, data.ToArray());
        }
    }
}