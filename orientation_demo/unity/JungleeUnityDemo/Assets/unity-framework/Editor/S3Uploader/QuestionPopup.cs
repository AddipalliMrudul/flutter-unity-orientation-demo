using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class QuestionPopup : EditorWindow
    {
        private System.Action<bool> OnCompleteEvent = null;
        private string mTitle = null;

        private int mArg1 = 1;
        private int mArg2 = 1;
        private int mAnswer = 0;

        public static void ShowUI(string title, System.Action<bool> inCallback)
        {
            QuestionPopup popup = GetWindow<QuestionPopup>();
            popup.titleContent.text = "Validation";
            popup.OnCompleteEvent += inCallback;
            popup.mArg1 = Random.Range(1, 10);
            popup.mArg2 = Random.Range(1, 10);
            popup.mTitle = title;
            popup.ShowAuxWindow();
        }

        private void OnGUI()
        {
            EditorGUILayout.TextArea(mTitle, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(mArg1 + " + " + mArg2 + " =??");
            mAnswer = EditorGUILayout.IntField(mAnswer);
            if (GUILayout.Button("Submit"))
                Close();
            EditorGUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            if (OnCompleteEvent != null)
                OnCompleteEvent((mArg1 + mArg2) == mAnswer);
        }
    }
}