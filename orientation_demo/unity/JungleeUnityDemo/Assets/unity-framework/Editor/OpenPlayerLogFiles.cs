#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    //https://docs.unity3d.com/2020.3/Documentation/Manual/LogFiles.html
    public class OpenPlayerLogFiles : ScriptableWizard
    {
#if UNITY_STANDALONE_OSX
        public const string mLogFilePath = "~/Library/Logs/{0}/{1}/";
#elif UNITY_STANDALONE_WIN
        public const string mLogFilePath = @"%USERPROFILE%\\AppData\\LocalLow\\{0}\\{1}\";
#else
        public const string mLogFilePath = "Not Supported yet";
#endif

        [MenuItem(Utilities.MenuName + "Open player log files")]
        static void OpenSavePath()
        {
            OpenLogFile(mLogFilePath, "Player.log");
        }

        private static void OpenLogFile(string dirPath, string fileName)
        {
            dirPath = string.Format(dirPath, Application.companyName, Application.productName);
            Debug.Log($"Opening log file: {dirPath}{fileName}");
            EditorUtility.RevealInFinder(dirPath);
        }
    }
}
#endif //UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
