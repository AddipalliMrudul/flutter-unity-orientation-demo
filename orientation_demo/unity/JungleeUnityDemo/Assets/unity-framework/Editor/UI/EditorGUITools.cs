using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace XcelerateGames.Editor.Inspectors
{
    public static class EditorGUITools
    {
        static public bool minimalisticLook
        {
            get { return false; }
        }

        public static bool DrawToggle(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = GUILayout.Toggle(value, "");
            GUILayout.EndHorizontal();
            return value;
        }

        public static int DrawInt(string label, int value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.IntField(value);
            GUILayout.EndHorizontal();
            return value;
        }

        //int The value entered by the user. Note that the return value will not change until the user has pressed enter or focus is moved away from the int field. 
        public static int DrawIntDelayed(string label, int value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.DelayedIntField(value);
            GUILayout.EndHorizontal();
            return value;
        }

        public static float DrawFloat(string label, float value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.FloatField(value);
            GUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// Draws a float value & makes sure the value is clamped between min & max 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float DrawFloatRange(string label, float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.FloatField(value);
            value = Mathf.Clamp(value, min, max);
            GUILayout.EndHorizontal();
            return value;
        }

        public static float DrawFloatDelayed(string label, float value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.DelayedFloatField(value);
            GUILayout.EndHorizontal();
            return value;
        }

        public static Vector3 DrawVector3(string label, Vector3 value)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.Vector3Field(label, value);
            GUILayout.EndHorizontal();
            return value;
        }

        public static Color DrawColor(string label, Color value)
        {
            GUILayout.BeginHorizontal();
            value = EditorGUILayout.ColorField(label, value);
            GUILayout.EndHorizontal();
            return value;
        }

        public static AnimationCurve DrawAnimationCurve(string label, AnimationCurve curve, float width = 348f, float height = 16f)
        {
            GUILayout.BeginHorizontal();
            curve = EditorGUILayout.CurveField(label, curve, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.EndHorizontal();
            return curve;
        }

        public static string DrawTextField(string label, string value)
        {
            if (value == null)
                value = string.Empty;
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.TextField(value);
            GUILayout.EndHorizontal();
            return value;
        }

        //int The value entered by the user. Note that the return value will not change until the user has pressed enter or focus is moved away from the int field. 
        public static string DrawTextFieldDelayed(string label, string value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.DelayedTextField(value);
            GUILayout.EndHorizontal();
            return value;
        }

        public static System.Enum DrawEnum(string label, System.Enum mode)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            mode = EditorGUILayout.EnumPopup(mode);
            GUILayout.EndHorizontal();
            return mode;
        }

        public static float DrawSlider(string label, float value, float leftValue, float rightValue)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.Slider(value, leftValue, rightValue);
            GUILayout.EndHorizontal();
            return value;
        }


        public static void DrawHeader(string text)
        {
            GUILayout.Label(text, EditorStyles.largeLabel, new GUILayoutOption[0] { });
        }

        public static void DrawHeaderBold(string text)
        {
            GUILayout.Label(text, EditorStyles.boldLabel, new GUILayoutOption[0] { });
        }

        public static void DrawPropertyRelative(string relativePropertyName, SerializedProperty parentProperty, SerializedObject serializedObject)
        {
            SerializedProperty relativeProperty = parentProperty.FindPropertyRelative(relativePropertyName);
            if (relativeProperty != null)
            {
                EditorGUILayout.PropertyField(relativeProperty);
                if (GUI.changed)
                    serializedObject.ApplyModifiedProperties();
            }
            else
                UnityEngine.Debug.LogError("Could not find relative property : " + relativePropertyName);
        }

        public static void DrawProperty(string propertyName, SerializedObject serializedObject)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property);
                if (GUI.changed)
                    serializedObject.ApplyModifiedProperties();
            }
            else
                UnityEngine.Debug.LogError("Could not find property : " + propertyName);
        }

        static public bool DrawDropDownHeader(string text) { return DrawDropDownHeader(text, text, false, minimalisticLook); }
        static public bool DrawDropDownHeader(string text, bool forceOn) { return DrawDropDownHeader(text, text, forceOn, minimalisticLook); }

        static public bool DrawDropDownHeader(string text, string key, bool forceOn, bool minimalistic)
        {
            bool state = EditorPrefs.GetBool(key, true);

            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;

                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC " + text;
                else text = "\u25BA " + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }

            if (GUI.changed) EditorPrefs.SetBool(key, state);

            if (!minimalistic) GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }

        /// <summary>
        /// Begin drawing the content area.
        /// </summary>

        static public void BeginContents() { BeginContents(minimalisticLook); }

        static bool mEndHorizontal = false;

        /// <summary>
        /// Begin drawing the content area.
        /// </summary>

        static public void BeginContents(bool minimalistic)
        {
            if (!minimalistic)
            {
                mEndHorizontal = true;
                GUILayout.BeginHorizontal();
                EditorGUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(10f));
            }
            else
            {
                mEndHorizontal = false;
                EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
                GUILayout.Space(10f);
            }
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }

        /// <summary>
        /// End drawing the content area.
        /// </summary>

        static public void EndContents()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (mEndHorizontal)
            {
                GUILayout.Space(3f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(3f);
        }
    }
}
