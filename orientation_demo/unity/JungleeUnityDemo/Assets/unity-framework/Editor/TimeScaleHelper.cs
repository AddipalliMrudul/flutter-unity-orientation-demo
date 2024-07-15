using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    /// <summary>
    /// A helper calss thta runs only on IDE to help speedup or slow down the game
    /// </summary>
    public class TimeScaleHelper : EditorWindow
    {
        [MenuItem(Utilities.MenuName + "Utils/Time Scale")]
        private static void Init()
        {
            EditorWindow.GetWindow<TimeScaleHelper>("Time Scale");
        }

        private void OnGUI()
        {
            if(!EditorApplication.isPlaying)
            {
                GUIColor.Push(Color.red);
                GUILayout.Label("Time Scale can be updated in play mode only!");
                GUIColor.Pop();
            }

            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("0"))
                Time.timeScale = 0F;
            if (GUILayout.Button("0.12"))
                Time.timeScale = .12F;
            if (GUILayout.Button("0.25"))
                Time.timeScale = .25F;
            if (GUILayout.Button("0.5"))
                Time.timeScale = .5F;
            if (GUILayout.Button("1x"))
                Time.timeScale = 1F;
            if (GUILayout.Button("2X"))
                Time.timeScale = 2F;
            if (GUILayout.Button("4X"))
                Time.timeScale = 4F;
            if (GUILayout.Button("10X"))
                Time.timeScale = 10F;
            if (GUILayout.Button("60X"))
                Time.timeScale = 60F;

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Value : ");
            Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0f, 5f, GUILayout.Width(256));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Label("Time Scale: " + Time.timeScale);
            EditorGUI.EndDisabledGroup();

            Repaint();
        }
    }
}