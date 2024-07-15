using UnityEngine;
using UnityEditor;

namespace XcelerateGames.EditorTools
{
    public class FindMissingScripts : EditorWindow
    {
        [MenuItem("XcelerateGames/EditorTools/FindMissingScripts")]
        public static void ShowWindow()
        {
            GetWindow(typeof(FindMissingScripts)).minSize = new Vector2(190f, 60f);
            GetWindow(typeof(FindMissingScripts)).maxSize = new Vector2(190f, 60f);
        }
        public void OnGUI()
        {
            if (GUI.Button(new Rect(20.0f, 20.0f, 150.0f, 20.0f), "Find Missing Scripts"))
            {
                Transform[] ts = Selection.GetTransforms(SelectionMode.Deep);
                foreach (Transform t in ts)
                {
                    Component[] components = t.GetComponents<Component>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        if (components[i] == null)
                        {
                            Debug.Log($"{t.name} has an empty script attached in position: \n{t.gameObject.GetObjectPath()}");
                        }
                    }
                }
            }
        }
    }
}
