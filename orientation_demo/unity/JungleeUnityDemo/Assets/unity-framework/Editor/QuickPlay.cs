using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class QuickPlay : ScriptableObject
    {
        protected static bool OpenScene(string scenePath)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.isPlaying = false;

            bool canOpen = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (canOpen)
                EditorSceneManager.OpenScene(scenePath);
            return canOpen;
        }
    }
}
