using UnityEngine;
using System.Collections;
using System.Globalization;
using UnityEditor;
using System.IO;
using XcelerateGames;

namespace XcelerateGames.Editor
{

    class ImportSourceCode : ScriptableWizard
    {
        public bool _ImportAll = false;

        public string _SourceFolder = "Click on Import & choose Assets folder.";
        [MenuItem(Utilities.MenuName + "Source Code/Import")]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard("Import Source Code", typeof(ImportSourceCode), "Import");
        }

        void OnWizardCreate()
        {
            _SourceFolder = EditorUtility.OpenFolderPanel("Choose Folder", "Assets", "Assets");
            if (_SourceFolder.EndsWith("/Assets"))
            {
                string dst = Application.dataPath;
                CopyFolder(_SourceFolder, dst);
            }
            else
                Debug.LogError("You must choose Assets folder");
        }

        void OnWizardUpdate()
        {
        }

        void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            int count = 0;
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                if (file.Contains("StreamingAssets"))
                    continue;
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                string ext = Path.GetExtension(name);
                if (ext.Equals(".cs", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".xml", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".txt", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".mm", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".h", System.StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals(".dll", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (_ImportAll || HasChanged(file, dest))
                    {
                        File.Copy(file, dest, true);
                        count++;
                    }
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

        bool HasChanged(string newFile, string oldFile)
        {
            System.DateTime dt1 = File.GetLastWriteTimeUtc(newFile);
            System.DateTime dt2 = File.GetLastWriteTimeUtc(oldFile);
            if (dt1 > dt2)
                return true;
            return false;
        }
    }
}
