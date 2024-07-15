using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XcelerateGames
{
    public interface IConsole
    {
        void OnHelp();
        bool OnExecute(string[] args, string baseCommand);
    }

    public class UiConsole : MonoBehaviour
    {
        public int _MaxLength = 5000;
        public InputField _InputField = null;
        public Transform _Panel = null;
        public Text _TextField = null;
        public int[] _UnlockPattern = {5, 1, 5};
        public float _Time = 5f;
        public float _DragHoldTime = 1f;

        protected List<string> mHistory = new List<string>();
        protected bool mIsUnlocked = false;
        protected bool mIsVisible = false;
        protected int mCommandIndex = 0;
        protected Canvas mCanvas = null;
        private float mClickTime = 0f;

        public static bool pEnableConsole = false;
        private int mIndex = 0;
        private float mTimer = 0f;
        internal static Dictionary<string, IConsole> mConsoles = new Dictionary<string, IConsole>();
        private static UiConsole mInstance = null;
        public static UiConsole Instance => mInstance;

        void Awake()
        {
            if (mInstance == null)
            {
                mCanvas = GetComponent<Canvas>();
                mInstance = this;
                //These are the core commands used by the console itself.
                mConsoles["cls"] = null;
                ShowUI(false);
                DontDestroyOnLoad(gameObject);

#if DEV_BUILD || QA_BUILD
                mIsUnlocked = true;
                pEnableConsole = true;
#endif

                if (RemoteSettings.HasKey(RemoteKeys.ConsoleEnablePattern))
                {
                    string[] data = RemoteSettings.GetString(RemoteKeys.ConsoleEnablePattern).Split(',');
                    _UnlockPattern = new int[data.Length];
                    int i = 0;
                    foreach(string s in data)
                    {
                        _UnlockPattern[i] = int.Parse(s);
                        i++;
                    }
                }
            }
            else
                Destroy(gameObject);
        }

        public static void Register(string baseCommand, IConsole listener)
        {
            if (string.IsNullOrEmpty(baseCommand))
            {
                WriteLine("Error! command cannot be empty.");
                //Debug.LogError("Error! command cannot be empty.");
                return;
            }

            baseCommand = baseCommand.ToLower();
            if (!mConsoles.ContainsKey(baseCommand))
                mConsoles.Add(baseCommand, listener);
            else
            {
                WriteLine("Error! " + baseCommand + " already exists.");
                //Debug.LogError("Error! " + baseCommand + " already exists.");
            }
        }

        void Start()
        {
        }

        void Update()
        {
#if UNITY_EDITOR ||  UNITY_STANDALONE
            if(!mIsVisible && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.BackQuote) && pEnableConsole)
            {
                ShowUI(true);
            }

            if(mIsVisible)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    PreviousCommand(true);
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    PreviousCommand(false);
                else if (Input.GetKeyDown(KeyCode.Escape))
                    ShowUI(false);
            }
#else
            if(pEnableConsole)
                CheckTouchInput();
#endif
        }

        public void OnCheatEntered()
        {
            if (!string.IsNullOrEmpty(_InputField.text))
            {
                if (!Application.isEditor && !mIsUnlocked)
                {
                    string hashPwd2 = Utilities.MD5Hash(_InputField.text);
                    //Default pwd: consolepwd
                    if ("493A3D41BDDDACD2EC2804426AA6C978" == hashPwd2)
                    {
                        mIsUnlocked = true;
                        WriteLine("Console Unlocked!");
                    }
                    else
                        WriteLine("Console is locked");

                    return;
                }

                //Strip extra white spaces.
                string myString = Regex.Replace(_InputField.text.Trim(), @"\s+", " ");
                string[] args = myString.Split(' ');
                string baseCommand = args[0];
                if (mConsoles.ContainsKey(baseCommand))
                {
                    if (args.Length == 1)
                    {
                        if (baseCommand.Equals("cls", StringComparison.OrdinalIgnoreCase))
                            _TextField.text = string.Empty;
                        else
                        {
                            //Invalid number of args, show help
                            mConsoles[baseCommand].OnHelp();
                        }
                    }
                    else
                    {
                        WriteLine(myString);
                        //Remove the main command & pass the rest of stuff.
                        ArrayHelper.DeleteArrayElement(ref args, 0);
                        //Call OnExecute, if it returns false then args might be wrong. Show help message.
                        if (!mConsoles[baseCommand].OnExecute(args, baseCommand))
                            mConsoles[baseCommand].OnHelp();
                        mHistory.Add(myString);
                        mCommandIndex = mHistory.Count - 1;
                        _InputField.text = string.Empty;
                    }
                }
                else
                {
                    WriteLine("Invalid command : " + _InputField.text);
                    //Debug.LogError("Invalid command : " + _InputField.text);
                }
            }
        }

        private void CheckTouchInput()
        {
            if (!mIsVisible)
            {
                if (Input.touchCount == _UnlockPattern[mIndex])
                {
                    if (mIndex == 0)
                        mTimer = _Time;
                    mIndex++;
                    XDebug.Log(mIndex + " TC : " + Input.touchCount);
                    if (mIndex >= _UnlockPattern.Length && mTimer >= 0f)
                    {
                        XDebug.Log("Showing console...");
                        mIndex = 0;
                        ShowUI(true);
                    }
                    else
                        XDebug.Log("Waiting for : " + _UnlockPattern[mIndex]);
                }
                else
                {
                    mTimer -= Time.deltaTime;
                    if (mTimer <= 0)
                        mIndex = 0;
                }
            }
        }

        public void PreviousCommand(bool showPrev)
        {
            if (mHistory.Count > 0)
            {
                if (showPrev)
                    mCommandIndex--;
                else
                    mCommandIndex++;

                mCommandIndex = Mathf.Clamp(mCommandIndex, 0, mHistory.Count);

                if (mCommandIndex >= 0 && mCommandIndex < mHistory.Count)
                    _InputField.text = mHistory[mCommandIndex];
            }
        }

        public void ShowUI(bool show)
        {
            mIsVisible = show;
            mCanvas.enabled = show;
            if (mIsVisible)
            {
                EventSystem.current.SetSelectedGameObject(_InputField.gameObject);
                _InputField.OnPointerClick(new PointerEventData(EventSystem.current));
            }
            Input.ResetInputAxes();
        }

        public void Toggle()
        {
            ShowUI(!mIsVisible);
        }

        public static void WriteLine(string line)
        {
            if (mInstance._TextField.text.Length > mInstance._MaxLength)
                mInstance._TextField.text = string.Empty;
            mInstance._TextField.text += "\n" + line;
        }

        public static void LogHandler(string logString, string stackTrace, LogType type)
        {
			if (mInstance != null && !string.IsNullOrEmpty(logString))
            {
                string[] splitStr = logString.Split('\n');
                for (int i = 0; i < Mathf.Min(3, splitStr.Length); i++)
                    WriteLine(splitStr[i]);
            }
            //		if (!string.IsNullOrEmpty(stackTrace))
            //			WriteLine(stackTrace);
        }

        public void OnDrag(BaseEventData eventData)
        {
            if((Time.realtimeSinceStartup - mClickTime) > _DragHoldTime)
                _Panel.position = Input.mousePosition;
        }

        public void OnSelect(BaseEventData eventData)
        {
            mClickTime = Time.realtimeSinceStartup;
        }
    }
}
