using UnityEngine;using System.Collections;using System.Globalization;using UnityEditor;using System.IO;using XcelerateGames;namespace XcelerateGames.Editor{

    class ExportSourceCode : ScriptableWizard
    {
        public string _FolderName = "Exported";

        [MenuItem(Utilities.MenuName + "Source Code/Export")]
        static void CreateWizard()
        {
            ExportSourceCode esc = ScriptableWizard.DisplayWizard("Export Source Code", typeof(ExportSourceCode), "Export") as ExportSourceCode;
            string[] folders = Application.dataPath.Split('/');
            esc._FolderName = folders[folders.Length - 2];
        }

        void OnWizardCreate()
        {
            string src = Application.dataPath;
            string dst = src.Replace("Assets", _FolderName + "/Assets");
            CopyFolder(src, dst);

            string projFolder = Application.dataPath.Replace("/Assets", "");
            CopyProjectFiles(projFolder, projFolder + "/" + _FolderName);
        }

        void OnWizardUpdate()
        {
        }

        static void CopyFolder(string sourceFolder, string destFolder)
        {
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                string ext = Path.GetExtension(name);
                if (ext.Equals(".cs", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".xml", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".txt", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".dll", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (!Directory.Exists(destFolder))
                        Directory.CreateDirectory(destFolder);
                    File.Copy(file, dest, true);
                }
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        static void CopyProjectFiles(string src, string dst)
        {
            string[] files = Directory.GetFiles(src);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(dst, name);
                string ext = Path.GetExtension(name);
                if (ext.Equals(".unityproj", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".csproj", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".sln", System.StringComparison.OrdinalIgnoreCase)
                    )
                    File.Copy(file, dest, true);
            }
        }
    }}

