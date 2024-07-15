using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class XGPropertyDrawer : PropertyDrawer
    {
        //Update this value from derived class
        protected int mNumElements = 5;
        protected int mOffset = 5;
        protected int mHeight = 20;

        protected Rect mPosition;
        protected SerializedProperty mProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * mNumElements + mOffset;
        }

        public Rect Next(int height = 16)
        {
            mPosition.y += height;
            return mPosition;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mPosition = position;
            mPosition.height = mHeight;
            mProperty = property;
        }

        public virtual void DrawLabel(GUIContent label)
        {
            EditorGUI.LabelField(mPosition, label);
        }

        public virtual void DrawProperty(string propertyName)
        {
            EditorGUI.PropertyField(Next(mHeight), mProperty.FindPropertyRelative(propertyName));
        }

        //Draw fields - passs GUIContent.none to each so they are drawn without labels
        public virtual void DrawPropertyWithoutName(string propertyName)
        {
            EditorGUI.PropertyField(Next(mHeight), mProperty.FindPropertyRelative(propertyName), GUIContent.none);
        }

        public virtual void DrawProperty(Rect rect, string propertyName)
        {
            EditorGUI.PropertyField(rect, mProperty.FindPropertyRelative(propertyName));
        }
    }
}
