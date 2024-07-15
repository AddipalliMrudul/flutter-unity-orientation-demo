using System.Collections.Generic;
using UnityEditor;

namespace XcelerateGames.Editor
{
    /// <summary>
    /// Script to Add/Remove scene from the scenes added in build settings
    /// </summary>
    public class EditorBuildSettingsSceneHandler
    {
        /// <summary>
        /// Script to add a scene to the editpor build settings
        /// scene name must be a fully qualified path.
        /// @example: Assets/Carrom/Scenes/carrom.unity
        /// </summary>
        /// <param name="sceneName">relative path of scene name with extension</param>
        public static void AddScene(string sceneName)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            if (!Exists(sceneName))
            {
                GUID guid = AssetDatabase.GUIDFromAssetPath(sceneName);
                scenes.Add(new EditorBuildSettingsScene() { enabled = true, path = sceneName, guid = guid });

                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        /// <summary>
        /// Checks if the given scene is added in EditrBuildSettings
        /// scene name must be a fully qualified path.
        /// @example: Assets/Carrom/Scenes/carrom.unity
        /// </summary>
        /// <param name="sceneName">relative path of scene name with extension</param>
        /// <returns>true if exists else false</returns>
        public static bool Exists(string sceneName)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            EditorBuildSettingsScene scene = scenes.Find(e => e.path == sceneName);
            return scene == null ? false : true;
        }
    }
}
