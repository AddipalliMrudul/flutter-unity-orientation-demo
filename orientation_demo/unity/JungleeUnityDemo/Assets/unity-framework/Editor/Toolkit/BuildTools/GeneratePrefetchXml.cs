using XcelerateGames;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;

namespace XcelerateGames.Editor.Build
{
    class GeneratePrefetchXml : ScriptableWizard
    {
        [MenuItem(Utilities.MenuName + "Build/Generate Prefetch XML")]
        public static void GenerateXML()
        {
            //List<string> processedFiles = new List<string>();

            //string[] files = Directory.GetFiles(EditorUtilities.mAssetsDir, "*.*", SearchOption.AllDirectories);
            //foreach (string fName in files)
            //{
            //    if (fName.EndsWith("unity3d") || fName.EndsWith(".xml") || fName.EndsWith(".lzma"))
            //    {
            //        string fileName = fName.Replace("\\", "/");
            //        if (!processedFiles.Contains(fileName))
            //            processedFiles.Add(fileName);
            //    }
            //}

            //PrefetchList pList = new PrefetchList();
            //pList.Bundles = new List<string>();

            //string secPrefFileName = EditorUtilities.mAssetsDir + EditorUtilities.SecondaryPrefetchList;
            //PrefetchList pListSecondary = null;
            //if(File.Exists(secPrefFileName))
            //    pListSecondary = Utilities.Deserialize<PrefetchList>(File.ReadAllText(secPrefFileName));
            //if(pListSecondary == null || pListSecondary.Bundles == null)
            //{
            //    pListSecondary = new PrefetchList();
            //    pListSecondary.Bundles = new List<string>();
            //}
            //ShippedAssets shippedAssets = ResourceManager.GetDefaultVersionList();

            //foreach (string path in processedFiles)
            //{
            //    if (path.Contains(EditorUtilities.PrimaryPrefetchList) || path.Contains(EditorUtilities.SecondaryPrefetchList) || path.Contains(ResourceManager.mAssetVersionListFileName))
            //        continue;

            //    List<string> splitPath = new List<string>(path.Split('/'));
            //    //Remove folder : Assets
            //    splitPath.RemoveAt(0);
            //    //Remove second folder
            //    splitPath.RemoveAt(0);

            //    if (shippedAssets != null && shippedAssets.Assets.Find(e => path.EndsWith(e.Name)) != null)
            //        continue;

            //    string fullSource = string.Join("/", splitPath.ToArray());

            //    if(!pListSecondary.Bundles.Contains(fullSource))
            //        pList.Bundles.Add(fullSource);
            //}

            //using (var writer = new System.IO.StreamWriter(EditorUtilities.mAssetsDir + EditorUtilities.PrimaryPrefetchList))
            //{
            //    var serializer = new XmlSerializer(typeof(PrefetchList));
            //    serializer.Serialize(writer, pList);
            //    writer.Flush();
            //}
        }
    }
}
