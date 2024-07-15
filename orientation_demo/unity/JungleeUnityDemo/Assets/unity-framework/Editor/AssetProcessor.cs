//#define ENABLE_ASSETPROCESSOR

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.Editor;

#if ENABLE_ASSETPROCESSOR

namespace XcelerateGames.Editor
{


    [InitializeOnLoad]
    public class AssetProcessor : AssetPostprocessor
    {
        private const string mKey = "DoNotPreProcessAssets";
        private const string NoOptimize = "NoOptimize";
        private const string mPreProcessAssetsMenuCmd = Utilities.MenuName + "No Asset Processing";

        private List<string> mIgnoreFolders = new List<string>()
    {
        "Assets/Plugins/Android",
        "Assets/Spine/Editor",
        "Quick Search",
    };

        private static void SetStatus()
        {
            Menu.SetChecked(mPreProcessAssetsMenuCmd, EditorPrefs.HasKey(mKey));
        }

        [MenuItem(mPreProcessAssetsMenuCmd, false, 1)]
        public static void TogglePreProcessAssets()
        {
            Menu.SetChecked(mPreProcessAssetsMenuCmd, !EditorPrefs.HasKey(mKey));
            if (Menu.GetChecked(mPreProcessAssetsMenuCmd))
                EditorPrefs.SetInt(mKey, 0);
            else
                EditorPrefs.DeleteKey(mKey);
        }

        //Added this function to just set the pre-process status before nay menu item is clicked
        [MenuItem(mPreProcessAssetsMenuCmd, true, 1)]
        public static bool TogglePreProcessAssetsValidate()
        {
            SetStatus();
            return true;
        }

        private bool CanPreProcess(string assetPath)
        {
            bool canProcess = false;
            if (!EditorPrefs.HasKey(mKey))
            {
                string path = mIgnoreFolders.Find(e => assetPath.Contains(e));
                if (string.IsNullOrEmpty(path))
                    canProcess = true;
            }
            return canProcess;
        }

        private void OnPreprocessModel()
        {
            if (!CanPreProcess(assetPath))
                return;
            //Debug.Log("Importing : " + assetPath);
            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter != null)
            {
                if (!EditorUtilities.HasLabel(assetPath, NoOptimize))
                {
                    modelImporter.isReadable = false;
                    modelImporter.meshCompression = ModelImporterMeshCompression.High;
                    modelImporter.optimizeGameObjects = true;
                    modelImporter.optimizeMeshPolygons = true;
                    modelImporter.optimizeMeshVertices = true;
                    modelImporter.animationCompression = ModelImporterAnimationCompression.KeyframeReductionAndCompression;
                }
            }
        }

        private void OnPreprocessTexture()
        {
            if (!CanPreProcess(assetPath))
                return;
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                if (!EditorUtilities.HasLabel(assetPath, NoOptimize))
                {
                    textureImporter.isReadable = false;
                    //if (textureImporter.textureType == TextureImporterType.Sprite)
                    textureImporter.mipmapEnabled = false;
                }
            }
        }

        private void OnPreprocessAudio()
        {
            if (!CanPreProcess(assetPath))
                return;
            AudioImporter audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (audioImporter != null)
            {
                if (!EditorUtilities.HasLabel(assetPath, NoOptimize))
                    audioImporter.forceToMono = true;
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            //foreach (string str in importedAssets)
            //{
            //    if (str.EndsWith(".xml"))
            //        HandleBOM(str);
            //}
            //foreach (string str in deletedAssets)
            //{
            //    Debug.Log("Deleted Asset: " + str);
            //}

            //for (int i = 0; i < movedAssets.Length; i++)
            //{
            //    Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
            //}
        }

        //if the given file has BOM encoding, saves the file again by removing it.
        static void HandleBOM(string inFile)
        {
            //if(HasBOM(inFile))
            //{
            //    Debug.LogWarning("BOM detected in " + inFile + ", Fixing now");
            //    StreamReader stringReader = new StreamReader(inFile, Encoding.Default);
            //    string fileContents = stringReader.ReadToEnd();
            //    stringReader.Close();
            //    //Write file back without utf-8 BOM
            //    File.WriteAllText(inFile, fileContents, new UTF8Encoding(false));
            //}
        }

        //Returns true if the given file has BOM encoding
        static bool HasBOM(string inFile)
        {
            bool hasBOM = false;
            using (FileStream fs = new FileStream(inFile, FileMode.Open))
            {
                byte[] bits = new byte[3];
                fs.Read(bits, 0, 3);

                // UTF8 byte order mark is: 0xEF,0xBB,0xBF
                if (bits[0] == 0xEF && bits[1] == 0xBB && bits[2] == 0xBF)
                    hasBOM = true;
            }
            return hasBOM;
        }
    }
}
#endif //ENABLE_ASSETPROCESSOR
