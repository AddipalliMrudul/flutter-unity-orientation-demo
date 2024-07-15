using TMPro;
using UnityEditor;
using UnityEngine;

/* Author : Altaf
 * Purpose : Test sprite sheet for letters by assigning a sprite tag.
 * Usage : Under a parent create 26 objects, each object must have TextMeshProUGUI as a child.
 */

namespace XcelerateGames.Editor
{
    public class LetterSpriteSheetHelper : ScriptableWizard
    {
        [MenuItem(Utilities.MenuName + "UI/Create Letter Sprites")]
        static void CreateLetterSprites()
        {
            Transform parent = Selection.activeTransform;
            for (int i = 0; i < parent.childCount; ++i)
            {
                TextMeshProUGUI textMesh = parent.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
                textMesh.text = $"<sprite={i}>";
            }
        }
    }
}