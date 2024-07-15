using UnityEditor;
using UnityEngine;

namespace XcelerateGames.WebServices
{
    [CustomEditor(typeof(WebRequestSettings))]
    public class WebRequestSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Save", GUILayout.Height(40)))
                WebRequestSettings.Save();

            if (GUILayout.Button("Load", GUILayout.Height(40)))
                WebRequestSettings.Load();

            if (GUILayout.Button("Clear", GUILayout.Height(40)))
                WebRequestSettings.Clear();
        }
    }
}
