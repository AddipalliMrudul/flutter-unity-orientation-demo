using System;
using System.IO;
using UnityEngine;
using XcelerateGames.AssetLoading;
using XcelerateGames.IOC;
using XcelerateGames.UI.Animations;
//using XcelerateGames.Social.Facebook;

namespace XcelerateGames.Debugging
{
    public class DebugConsole : BaseBehaviour, IConsole
    {
        [InjectSignal] private SigSendMailLog mSigSendMailLog = null;

        private void Start()
        {
            UiConsole.Register("dbg", this);
        }

        public void OnHelp()
        {
            UiConsole.WriteLine("Help from DebugConsole");
        }

        public bool OnExecute(string[] args, string baseCommand)
        {
            if (Utilities.Equals(args[0], "DeviceInfo"))
            {
                string data = "Model : " + PlatformUtilities.GetDeviceModel();
                data += "\n Graphics Memory : " + SystemInfo.graphicsMemorySize;
                data += "\n RAM(Unity API) : " + SystemInfo.systemMemorySize;
                data += "\n graphicsDeviceName : " + SystemInfo.graphicsDeviceName;
                data += "\n graphicsDeviceType : " + SystemInfo.graphicsDeviceType;
                data += "\n graphicsDeviceVendor : " + SystemInfo.graphicsDeviceVendor;
                UiConsole.WriteLine(data);
                UnityEngine.Debug.Log(data);
            }
            else if (args[0] == "tfr")
            {
                int targetFPS = Application.targetFrameRate;
                int.TryParse(args[1], out targetFPS);
                Application.targetFrameRate = targetFPS;
                UiConsole.WriteLine("Target Frame Rate set to : " + targetFPS);
            }
            else if (Utilities.Equals(args[0], "dump"))
            {
                Debug.Log("Dumping to : " + Application.persistentDataPath + "/Dump.txt");
                StreamWriter fout = new StreamWriter(Application.persistentDataPath + "/Dump.txt");
                bool verbose = args.Length >= 2 && Utilities.Equals(args[1], "v");
                ResourceManager.Dump(fout, verbose);
                //LevelInit lvlInit = Utilities.FindObjectOfType<LevelInit>();
                //if (lvlInit != null)
                    //lvlInit.Dump(fout);
                fout.Close();
            }
            else if (args[0] == "mail")
            {
                UiConsole.Instance.ShowUI(false);
                mSigSendMailLog.Dispatch(false, ("altaf@jungleegames.com", "Default Test Mail", "This mail is being triggerred from Debug Console."));
            }
            else if (args[0] == "memdump")
            {
                string identifier = string.Empty;
                if (args.Length == 2)
                    identifier = args[1];
                MemDump.WriteToFile(identifier);
            }
            else if (args[0] == "excpt" || args[0] == "exception")
            {
                string msg = "test exception please ignore " + DateTime.Now.ToString();
                UiConsole.WriteLine(msg);
                throw new Exception(msg);
            }
            else if (args[0] == "crash")
            {
                UiConsole.WriteLine("Crashing the app");
                UnityEngine.Diagnostics.ForcedCrashCategory crashType = UnityEngine.Diagnostics.ForcedCrashCategory.Abort;
                if(args.Length == 2)
                {
                    crashType = args[1].ToEnum<UnityEngine.Diagnostics.ForcedCrashCategory>();
                }
                Debug.Log($"Causing crash of type: {crashType}");
                if(!Application.isEditor)
                    UnityEngine.Diagnostics.Utils.ForceCrash(crashType);
            }
            else if (args[0] == "dpp")
            {
                if (args.Length == 1)
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    UiConsole.WriteLine("Deleted all PlayerPrefs.");
                }
                else
                {
                    if (PlayerPrefs.HasKey(args[1]))
                    {
                        PlayerPrefs.DeleteKey(args[1]);
                        PlayerPrefs.Save();
                        UiConsole.WriteLine("Deleted " + args[1] + " from PlayerPrefs.");
                    }
                    else
                        UiConsole.WriteLine("Could not find " + args[1] + " in PlayerPrefs.");
                }
            }
            else if (args[0] == "fr")
            {
                if (args.Length == 2)
                {
                    bool show = args[1] == "on";
                    UiDebugInfo objs = Utilities.FindObjectOfType<UiDebugInfo>();
                    if (objs != null)
                        objs.Show(show);
                }
            }
            else if (args[0] == "mask")
            {
                if (args.Length == 3)
                {
                    if (args[1] == "add")
                    {
                        int mask = 1 << int.Parse(args[2]);
                        XDebug.AddMask((XDebug.Mask)mask);
                    }
                    else if (args[1] == "remove" || args[1] == "rm")
                    {
                        int mask = 1 << int.Parse(args[2]);
                        XDebug.RemoveMask((XDebug.Mask)mask);
                    }
                    UiConsole.WriteLine("Updated mask : " + XDebug.pMask);
                }
                else
                    UiConsole.WriteLine("Current mask : " + XDebug.pMask);
            }
            else if (args[0] == "logpriority")
            {
                if (args.Length == 2)
                {
                    if (args[1] == "all" || args[1] == "a")
                        XDebug.mLogPriority = XDebug.Priority.Always;
                    else if (args[1] == "high" || args[1] == "h")
                        XDebug.mLogPriority = XDebug.Priority.High;
                    else if (args[1] == "mid" || args[1] == "m")
                        XDebug.mLogPriority = XDebug.Priority.Mid;
                    else if (args[1] == "low" || args[1] == "l")
                        XDebug.mLogPriority = XDebug.Priority.Low;
                    UiConsole.WriteLine("Logging Priority : " + XDebug.mLogPriority);
                }
                else
                    UiConsole.WriteLine("Invalid aruments. usage : dbg logpriority all/high/mid/low");
            }
            else if (args[0] == "log")
            {
                if (args.Length == 2)
                {
                    LogConsole.pInstance._ShowLog = args[1].Contains("l");
                    LogConsole.pInstance._ShowWarning = args[1].Contains("w");
                    LogConsole.pInstance._ShowError = args[1].Contains("e");
                    LogConsole.pInstance._ShowStackTrace = args[1].Contains("s");
                }
                else
                    UiConsole.WriteLine("Invalid aruments. usage : dbg log lwes");
            }
            else if (args[0] == "save")
            {
                PlayerPrefs.SetInt(GameConfig.LogPriority, (int)XDebug.mLogPriority);
                PlayerPrefs.SetString(GameConfig.Mask, XDebug.pMask.ToString());

                UiConsole.WriteLine("Saved all debug info.");
            }
            else if (args[0] == "clear")
            {
                PlayerPrefs.DeleteKey(GameConfig.LogPriority);
                PlayerPrefs.DeleteKey(GameConfig.Mask);

                UiConsole.WriteLine("Cleared all debug info.");
            }
            else if (args[0] == "mail")
            {
                Utilities.SendEmail("");
            }
            //else if (args[0] == "localtime")
            //{
            //    bool useLocalTime = args[1] == "on";
            //    if (useLocalTime)
            //        UnityEngine.PlayerPrefs.SetInt(LSG.GameConfig.LocalTimeKey, 1);
            //    else
            //        UnityEngine.PlayerPrefs.DeleteKey(LSG.GameConfig.LocalTimeKey);
            //    ServerTime.pLocalTime = useLocalTime;
            //    UiConsole.WriteLine(useLocalTime ? "Using local time." : "Use Server time.");
            //}
            else if (args[0] == "capture")
            {
                if (args[1] == "on")
                {
                    int fr = 25;
                    if (args.Length == 3)
                        fr = int.Parse(args[2]);
                    //CaptureFrames.StartCapture(fr);
                    UiConsole.WriteLine("Started Screen Capture");
                }
                else if (args[1] == "off")
                {
                    //CaptureFrames.EndCapture();
                    UiConsole.WriteLine("Ended Screen Capture");
                }
            }
            else if (args[0] == "load")
            {
                if (args.Length == 2)
                    ResourceManager.LoadScene(args[1]);
                else
                    UiConsole.WriteLine("Error!! Invalid number of args. Enter scene name to load.");
            }
            else if (args[0] == "unload")
            {
                if (args.Length == 1)
                {
                    ResourceManager.UnloadUnusedAssets();
                    UiConsole.WriteLine("UnloadUnusedAssets called");
                }
                else
                {
                    UiConsole.WriteLine("Error!! Invalid number of args. useage dbg unload");
                }
            }
            //else if (args[0] == "wsdbg")
            //{
            //    if (args.Length >= 2)
            //    {
            //        bool useDbgUrl = args[1].Equals("on");
            //        if (useDbgUrl)
            //        {
            //            if (args.Length == 3)
            //            {
            //                string url = "http://" + args[2] + "/";
            //                UnityEngine.PlayerPrefs.SetString(BAWebRequestKeys.DbgApiUrl, url);
            //                UiConsole.WriteLine("Web Service Debug enabled, Debug API : " + url);
            //            }
            //            else
            //                UiConsole.WriteLine("Must specify API/IP address and port: " + useDbgUrl);
            //        }
            //        else
            //        {
            //            UnityEngine.PlayerPrefs.DeleteKey(BAWebRequestKeys.DbgApiUrl);
            //            UiConsole.WriteLine("Web Service Debug disabled.");
            //        }
            //    }
            //}
            else if (args[0] == "uianim")
            {
                if (args.Length == 2)
                {
                    UiAnimBase.pDebugAll = args[1].Equals("on") || args[1].Equals("1");
                    UiConsole.WriteLine("UiAnim Debug enabled? : " + UiAnimBase.pDebugAll);
                }
            }
            //else if (args[0] == "logout")
            //{
            //    FacebookManager.Instance.Logout();
            //    UiConsole.WriteLine("Logged out of FB account, Quit & launch now");
            //}
            return true;
        }
    }
}