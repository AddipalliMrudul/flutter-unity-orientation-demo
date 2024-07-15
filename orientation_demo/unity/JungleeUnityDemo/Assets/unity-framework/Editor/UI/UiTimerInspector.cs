using UnityEditor;
using XcelerateGames.UI;
using TMPro;
using UnityEngine;
using XcelerateGames.Audio;
using UnityEditor.SceneManagement;

namespace XcelerateGames.Editor.Inspectors
{
    [CustomEditor(typeof(UiTimer))]
    public class UiTimerInspector : UnityEditor.Editor
    {
        private UiTimer mTarget = null;

        public override void OnInspectorGUI()
        {
            mTarget = target as UiTimer;

            string msg = GetInfoString(mTarget);
            EditorGUILayout.HelpBox(msg, MessageType.Info);
            mTarget._TimeFormat = (UiTimer.TimeFormat)EditorGUITools.DrawEnum("Time Format", mTarget._TimeFormat);
            mTarget._TextItem = (UiItem)EditorGUILayout.ObjectField("Text Item", mTarget._TextItem, typeof(UiItem), true);
            mTarget._DefaultColor = EditorGUITools.DrawColor("Warning Color", mTarget._DefaultColor);
            mTarget._WarningTime = EditorGUITools.DrawIntDelayed("Warning Time", mTarget._WarningTime);
            mTarget._WarningColor = EditorGUITools.DrawColor("Warning Color", mTarget._WarningColor);

            //Audio Vars related
            mTarget._WarningSFX._SoundClip = (AudioClip)EditorGUILayout.ObjectField("Sound Clip", mTarget._WarningSFX._SoundClip, typeof(AudioClip), true);
            mTarget._WarningSFX._SoundName = EditorGUITools.DrawTextFieldDelayed("Sound Name", mTarget._WarningSFX._SoundName);
            mTarget._WarningSFX._Category = (AudioController.Category)EditorGUITools.DrawEnum("Category", mTarget._WarningSFX._Category);
            mTarget._WarningSFX._Delay = EditorGUITools.DrawFloatDelayed("Delay", mTarget._WarningSFX._Delay);
            mTarget._WarningSFX._Volume = EditorGUITools.DrawFloatDelayed("Volume", mTarget._WarningSFX._Volume);
            mTarget._WarningSFX._Pitch = EditorGUITools.DrawFloatDelayed("Pitch", mTarget._WarningSFX._Pitch);
            mTarget._WarningSFX._Loop = EditorGUITools.DrawToggle("Loop", mTarget._WarningSFX._Loop);
        }

        private string GetInfoString(UiTimer timerObj)
        {
            if (timerObj._TimeFormat == UiTimer.TimeFormat.Short)
                return "> 24, DD:HH else HH:MM";
            if (timerObj._TimeFormat == UiTimer.TimeFormat.ShortNoAbbreviations)
                return "Draws time in DD:HH format if the time is above 24 hours else HH:MM, with no h m & s";
            if (timerObj._TimeFormat == UiTimer.TimeFormat.Standard)
                return "Draws standard timer string in HH:MM:SS format if within 24 hours, else in DD:HH:MM format";
            if (timerObj._TimeFormat == UiTimer.TimeFormat.SmartTimer)
                return ">24, 00d 00h 00m, <24 hours, 00h:00m:00s, < hour, 00m:00s, < minute, 00s";
            if (timerObj._TimeFormat == UiTimer.TimeFormat.ShortNoAbbreviations2)
                return "Draws time in DD:HH format if the time is above 24 hours else HH:MM:SS, with no h m & s";
            if (timerObj._TimeFormat == UiTimer.TimeFormat.ShortWithTwoFields)
                return "> 24, DD:HH else HH:MM else MM:SS";

            return "Please select a format to use";
        }

        private void SetObjectDirty()
        {
            if (Application.isPlaying || mTarget == null)
                return;
            Transform transform = mTarget.transform;
            EditorUtility.SetDirty(mTarget);
            EditorSceneManager.MarkSceneDirty(transform.gameObject.scene);
        }

        private void OnEnable()
        {
            mTarget = (UiTimer)target;
        }

        private void OnDisable()
        {
            SetObjectDirty();
        }
    }
}