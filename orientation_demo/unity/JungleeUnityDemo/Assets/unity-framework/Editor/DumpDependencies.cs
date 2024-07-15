using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class DumpDependencies : ScriptableWizard
    {
        [MenuItem(Utilities.MenuName + "Debug/Dump Dependencies")]
        static void DoDumpDependencies()
        {
            DisplayWizard("Dump Dependencies", typeof(DumpDependencies), "Dump", "");
        }

        [MenuItem(Utilities.MenuName + "Dump Dependencies", true)]
        static bool DoDumpDependenciesValidate()
        {
            return Selection.objects.Length > 0;
        }

        void Update()
        {
            if (Selection.objects.Length > 1)
            {
                helpString = "Select only one object";
                isValid = false;
            }
            else
                isValid = true;
        }

        void OnWizardCreate()
        {
            Dictionary<string, List<string>> assets = new Dictionary<string, List<string>>();
            //Create a file with name same as selected object.
            string fileName = "Dependencies_" + Selection.objects[0].name + ".txt";
            StreamWriter writer = new StreamWriter(File.Create(fileName));
            //Execute select dependencies command
            EditorApplication.ExecuteMenuItem("Assets/Select Dependencies");
            //Iterate through each dependency
            foreach (Object obj in Selection.objects)
            {
                string type = GetType(AssetDatabase.GetAssetPath(obj));
                if (!assets.ContainsKey(type))
                    assets.Add(type, new List<string>());
                string data = AssetDatabase.GetAssetPath(obj) + ",   " + EditorUtilities.GetFormattedSize(obj);
                assets[type].Add(data);
            }

            foreach (string type in assets.Keys)
            {
                writer.WriteLine("================================" + type + "=================================");
                foreach (string asset in assets[type])
                    writer.WriteLine("\t" + asset);
                writer.WriteLine("=================================================================================\n\n");
            }

            EditorUtility.DisplayDialog("Done", "Dumped dependencies to " + fileName, "Ok");

            writer.Close();
        }

        string GetType(string inAsset)
        {
            if (inAsset.EndsWith(".shader"))
                return "Shaders";
            if (inAsset.EndsWith(".prefab"))
                return "Prefabs";
            if (inAsset.EndsWith(".unity"))
                return "Scenes";
            if (inAsset.EndsWith(".mat") || inAsset.EndsWith("physicMaterial"))
                return "Materials";
            if (inAsset.EndsWith(".anim") || inAsset.EndsWith(".controller"))
                return "Animations";
            if (inAsset.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                return "Meshes";
            if (inAsset.EndsWith(".cs") || inAsset.EndsWith(".js"))
                return "Scripts";
            if (inAsset.EndsWith(".asset"))
                return "Assets";
            if (inAsset.EndsWith(".ogg") || inAsset.EndsWith(".mp3") || inAsset.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase))
                return "Audio";
            //Default to texture type as there are a lot of texture types as in png, jpeg, jpg etc
            return "Textures";
        }
    }
}