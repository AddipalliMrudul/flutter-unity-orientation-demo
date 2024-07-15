using UnityEngine;
using UnityEditor;
using XcelerateGames;

namespace XcelerateGames.Editor
{
    public class ClearUnityCache : ScriptableObject
    {
        [MenuItem(Utilities.MenuName + "ClearCache")]
        public static void Clear_Unity_Cache()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
            UnityEngine.PlayerPrefs.Save();
            string message = "Player Prefs : Deleted";
            if (System.IO.Directory.Exists(PlatformUtilities.GetPersistentDataPath()))
            {
                System.IO.Directory.Delete(PlatformUtilities.GetPersistentDataPath(), true);
                message += "\n\nLocal Cache : Deleted";
            }
            if (System.IO.Directory.Exists(Application.persistentDataPath))
            {
                System.IO.Directory.Delete(Application.persistentDataPath, true);
                message += "\n\nLocal Cache : Deleted";
            }
            else
                message += "\n\nLocal Cache : Empty";

            if (Caching.ClearCache())
                message += "\n\nCleared Unity Cache";
            else
                message += "\n\nFailed to Clear Unity Cache";

            EditorUtility.DisplayDialog("Clear Cache", message, "Ok");
        }
    }
}
