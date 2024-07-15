using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class TransformHierarchy
    {
        [MenuItem("GameObject/Move Up", false, 10)]
        public static void MoveTransformUp()
        {
            if (Selection.activeTransform != null)
            {
                int index = Selection.activeTransform.GetSiblingIndex();
                --index;
                index = Mathf.Max(index, 0);
                Selection.activeTransform.SetSiblingIndex(index);
            }
        }

        [MenuItem("GameObject/Move Down", false, 10)]
        public static void MoveTransformDown()
        {
            if(Selection.activeTransform != null)
            {
                int index = Selection.activeTransform.GetSiblingIndex();
                ++index;
                index = Mathf.Min(index, Selection.activeTransform.parent.childCount - 1);
                Selection.activeTransform.SetSiblingIndex(index);
            }
        }
    }
}
