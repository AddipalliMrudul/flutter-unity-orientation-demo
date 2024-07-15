using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.Editor.UI
{
    /// <summary>
    /// Script to Replace one Sprite with another Sprite.
    /// Select the prefab(s) in Project window & select "UI/Replace Sprite" from menu.
    /// Select the Sprite to be searched & replacd with.
    /// Intensionally ot checking for null mFrom or mTo, as there may be a requirement of replacing a null sprite with a new one.
    /// </summary>
    public class ReplaceSpriteInImage : EditorWindow
    {
        Sprite mFrom = null;
        Sprite mTo = null;

        [MenuItem(Utilities.MenuName + "UI/Replace Sprite")]
        static void CreateWizard()
        {
            ReplaceSpriteInImage window = GetWindow<ReplaceSpriteInImage>();
            window.titleContent.text = "Replace Sprite";
        }

        /// <summary>
        /// Draw the UI to select mFrom & mTo sprite
        /// </summary>
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            mFrom = (Sprite)EditorGUILayout.ObjectField("From", mFrom, typeof(Sprite), false);
            mTo = (Sprite)EditorGUILayout.ObjectField("To", mTo, typeof(Sprite), false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.Height(32), GUILayout.Width(64)))
            {
                foreach (UnityEngine.Object obj in Selection.objects)
                {
                    GameObject gameObject = obj as GameObject;
                    bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(obj);
                    if (gameObject != null)
                    {
                        Image[] images = gameObject.GetComponentsInChildren<Image>(true);
                        Array.ForEach(images, image => Swap(image));
                        if (isPrefab)
                            PrefabUtility.SavePrefabAsset(gameObject);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Swap the Sprite if its using the Sprite same as set in in _From sprite
        /// </summary>
        /// <param name="image"></param>
        void Swap(Image image)
        {
            if(image.sprite == mFrom)
            {
                image.sprite = mTo;
            }
        }
    }
}
