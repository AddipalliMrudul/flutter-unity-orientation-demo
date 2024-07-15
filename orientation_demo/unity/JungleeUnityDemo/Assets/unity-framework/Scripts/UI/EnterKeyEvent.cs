using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Attach this script to the same GameObject as InputField, it will give you a callback when enter is pressed.
/// </summary>

namespace XcelerateGames.UI
{
    [ExecuteInEditMode]
    [AddComponentMenu("Event/EnterKeyEvent")]
    [RequireComponent(typeof(TMP_InputField))]
    public class EnterKeyEvent : MonoBehaviour
    {
        [System.Serializable]
        public class EnterEvent : UnityEvent { }

        public EnterEvent OnEnter;
        private TMP_InputField mInputField = null;

        private void Awake()
        {
            mInputField = GetComponent<TMP_InputField>();
            if (mInputField == null)
            {
                Debug.LogError("Could not find InputField component on " + name + ", EnterKeyEvent & InputField should be on the same object.");
                enabled = false;
            }
            else
                mInputField.onEndEdit.AddListener(OnValueChanged);
        }

        private void OnValueChanged(string arg0)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
                OnEnter.Invoke();
        }
    }
}