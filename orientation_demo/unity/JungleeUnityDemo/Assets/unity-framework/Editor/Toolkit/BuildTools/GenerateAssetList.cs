using System.Collections.Generic;
using System.IO;
using UnityEditor;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Editor
{

    class GenerateAssetList : ScriptableWizard
    {
        class AssetInfo
        {
            public string FileName = null;
            public long Size = 0;
        }
        public string _FileName = "Assets/AssetList.csv";

        //[MenuItem(Utilities.MenuName + "Generate Asset List")]
        //static void GenerateXML()
        //{
        //	DisplayWizard ("Generate Asset List", typeof(GenerateAssetList), "Create");
        //}

        void OnWizardCreate()
        {
            StreamWriter writer = null;
            FileStream fStream = File.Create(_FileName);
            writer = new StreamWriter(fStream);
            List<AssetInfo> assetInfo = new List<AssetInfo>();
            string[] files = Directory.GetFiles(EditorUtilities.mAssetsDir, "*.*", SearchOption.AllDirectories);
            foreach (string fName in files)
            {
                if (fName.EndsWith("unity3d") || ResourceManager.IsTextAsset(fName))
                {
                    AssetInfo info = new AssetInfo();
                    info.FileName = Path.GetFileName(fName);
                    FileInfo fInfo = new FileInfo(fName);
                    info.Size = fInfo.Length;
                    assetInfo.Add(info);
                }
            }

            //Sort the list by high to low
            assetInfo.Sort(delegate (AssetInfo ai1, AssetInfo ai2)
            {
                return ai2.Size.CompareTo(ai1.Size);
            });

            writer.WriteLine("File Name" + ", " + "Size" + ", Importance");

            foreach (AssetInfo aInfo in assetInfo)
                writer.WriteLine(aInfo.FileName + ", " + Utilities.FormatBytes(aInfo.Size) + ", None");
            writer.Close();
        }
    }
}

