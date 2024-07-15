using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class DeletePlayerPrefs : EditorWindow
    {
        public string mKeyName = "";
        public string mEditorPrefsKeyName = "";

        [MenuItem(Utilities.MenuName + "Delete Player Prefs")]
        static void CreateWizard()
        {
            GetWindow<DeletePlayerPrefs>();
        }

        void OnWizardCreate()
        {
        }

        void OnGUI()
        {
            GUILayout.Label("Game Player Prefs", EditorStyles.boldLabel);
            if (GUILayout.Button("Delete All"))
            {
                UnityEngine.PlayerPrefs.DeleteAll();
                Debug.Log("Deleted all player prefs.");
                UnityEngine.PlayerPrefs.Save();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete"))
            {
                if (!string.IsNullOrEmpty(mKeyName))
                {
                    if (UnityEngine.PlayerPrefs.HasKey(mKeyName))
                    {
                        UnityEngine.PlayerPrefs.DeleteKey(mKeyName);
                        UnityEngine.PlayerPrefs.Save();
                        Debug.Log("Deleted " + mKeyName + " from player prefs.");
                    }
                    else
                        Debug.LogError(mKeyName + " is not present in player prefs.");
                }
            }
            mKeyName = EditorGUILayout.TextField(mKeyName);
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Editor Player Prefs", EditorStyles.boldLabel);
            if (GUILayout.Button("Delete All"))
            {
                UnityEditor.EditorPrefs.DeleteAll();
                Debug.Log("Deleted all editor player prefs.");
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete"))
            {
                if (!string.IsNullOrEmpty(mEditorPrefsKeyName))
                {
                    if (UnityEditor.EditorPrefs.HasKey(mEditorPrefsKeyName))
                    {
                        UnityEditor.EditorPrefs.DeleteKey(mEditorPrefsKeyName);
                        Debug.Log("Deleted " + mEditorPrefsKeyName + " from editor player prefs.");
                    }
                    else
                        Debug.LogError(mEditorPrefsKeyName + " is not present in editor player prefs.");
                }
            }
            mEditorPrefsKeyName = EditorGUILayout.TextField(mEditorPrefsKeyName);
            EditorGUILayout.EndHorizontal();
        }
    }
}