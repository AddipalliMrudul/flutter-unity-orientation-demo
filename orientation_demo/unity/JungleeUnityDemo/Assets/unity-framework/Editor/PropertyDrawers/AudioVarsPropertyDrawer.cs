using UnityEditor;
using UnityEngine;
using XcelerateGames.Audio;

namespace XcelerateGames.Editor
{
    [CustomPropertyDrawer(typeof(AudioVars))]
    public class AudioVarsPropertyDrawer : XGPropertyDrawer
    {
        public AudioVarsPropertyDrawer()
        {
            mNumElements = 9;
            mHeight = 20;
        }   

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            DrawLabel(label);
            EditorGUI.indentLevel++;

            DrawProperty("_SoundClip");
            DrawProperty("_SoundName");
            DrawProperty("_Category");
            DrawProperty("_Delay");
            DrawProperty("_Volume");
            DrawProperty("_Pitch");
            DrawProperty("_Loop");

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
