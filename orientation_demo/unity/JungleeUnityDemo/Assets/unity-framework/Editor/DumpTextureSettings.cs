using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class DumpTextureSettings : EditorWindow
    {
        private int mIndex = -1;
        private List<string> mAllAssets = null;
        private StreamWriter fOut = null;
        private string mFileName = "TextureSettings.txt";
        private string mFormat = "Any";

        [MenuItem(Utilities.MenuName + "Debug/Dump Texture Settings")]
        static void DoLayerSearch()
        {
            GetWindow<DumpTextureSettings>();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("File Name ", EditorStyles.boldLabel, new GUILayoutOption[0] { });
            mFileName = GUILayout.TextField(mFileName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Format Type to write: ", EditorStyles.boldLabel, new GUILayoutOption[0] { });
            mFormat = GUILayout.TextField(mFormat);
            GUILayout.EndHorizontal();

            if (mAllAssets != null && mAllAssets.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Processing : " + mIndex + "/" + mAllAssets.Count, EditorStyles.boldLabel, new GUILayoutOption[0] { });
                GUILayout.EndHorizontal();
            }

            if (mIndex == -1 && GUILayout.Button("Dump"))
            {
                mAllAssets = new List<string>(AssetDatabase.GetAllAssetPaths());
                mAllAssets.RemoveAll(e => !IsTexture(e));
                fOut = new StreamWriter(mFileName);
                fOut.WriteLine("{0,-100} {1,-20} {2,-7}", "Path".PadLeft(50), "Format".PadLeft(10), "Compressed?");
                fOut.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------");
                mIndex = 0;
            }
        }

        private void Update()
        {
            if (mAllAssets == null || mIndex >= mAllAssets.Count)
                return;

            string strPath = mAllAssets[mIndex];

            TextureImporter textureImporter = AssetImporter.GetAtPath(strPath) as TextureImporter;
            if (textureImporter != null)
            {
                //string data = strPath;
                TextureImporterPlatformSettings ps = textureImporter.GetPlatformTextureSettings(EditorUtilities.GetCurrentPlatform());
                if (ps != null)
                {
                    if (mFormat.Equals("Any") || ps.format.ToString() == mFormat)
                        fOut.WriteLine("{0,-100} {1,-20} {2,-7}", strPath, ps.format, ps.crunchedCompression);
                }
                else
                    fOut.WriteLine("{0,-100} NULL", strPath);
            }
            mIndex++;
            if (mIndex >= mAllAssets.Count)
            {
                fOut.Close();
                Close();
            }
            Repaint();
        }

        private bool IsTexture(string assetPath)
        {
            assetPath = assetPath.ToLower();
            if (assetPath.EndsWith(".ttf") || assetPath.EndsWith(".bmp") || assetPath.EndsWith(".psd") || assetPath.EndsWith(".png") || assetPath.EndsWith(".tga") || assetPath.EndsWith(".jpg") || assetPath.EndsWith(".jpeg") || assetPath.EndsWith(".tif"))
                return true;
            return false;
        }
    }
}