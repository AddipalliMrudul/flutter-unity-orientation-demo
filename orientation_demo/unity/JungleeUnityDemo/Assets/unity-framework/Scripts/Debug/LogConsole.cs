using System;
using System.IO;
using UnityEngine;

namespace XcelerateGames
{
    public class LogConsole : BaseBehaviour
    {
        public string _FileName = "DeviceLog";
        public bool _EnableLog = true;
        public bool _ShowLog = false;
        public bool _ShowWarning = true;
        public bool _ShowError = true;

#if UNITY_EDITOR
        public bool _EnableLogOnEditor = false;
#endif
        public bool _UniqueFile = true;
        public bool _ShowStackTrace = true;

        public static string FilePath => mInstance.mFullPath;
        private StreamWriter mWriter = null;
        private string mFullPath = null;

        private static int mNumExceptions = 0;
        private static LogConsole mInstance = null;

        public static int pNumExceptions
        {
            get { return mNumExceptions; }
        }

        public static LogConsole pInstance { get { return mInstance; } }

        protected override void Awake()
        {
            //Debug.Log($"LTS: LogConsole Awake: {Time.frameCount} : {DateTime.Now.ToString("HH:mm:ss.fff")}");

#if UNITY_EDITOR
            if (!_EnableLogOnEditor)
                _EnableLog = false;
#endif

#if UNITY_ANDROID || UNITY_IOS || UNITY_METRO || UNITY_STANDALONE
            if (_EnableLog && mInstance == null)
            {
                GameObject.DontDestroyOnLoad(gameObject);
                System.AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(OnUnresolvedExceptionHandler);
                mInstance = this;
                CreateLogFile();
            }
            else
                GameObject.Destroy(gameObject);
#else
			GameObject.Destroy(gameObject);
#endif
        }

        protected override void OnDestroy()
        {
            if (mWriter != null)
                mWriter.Close();
            Application.logMessageReceived -= OnDebugLogCallbackHandler;
            base.OnDestroy();
        }

        public string CreateLogFile()
        {
            //#if LIVE_BUILD || BETA_BUILD
            _UniqueFile = false;
            //		_ShowLog = false;
            //		_ShowWarning = false;
            //		_ShowError = false;
            //#endif
            string prefix = "";
            if (_UniqueFile)
            {
                //Create a file with current date & time to avoid overwriting to same file.
                DateTime dt = DateTime.Now;
                prefix = "-" + dt.Day + "-" + dt.Month + "-" + dt.Year + "(" + dt.Hour + "-" + dt.Minute + ").txt";
            }
            else
                prefix = ".txt";

            if (mWriter != null)
                mWriter.Close();
            mFullPath = Application.persistentDataPath + "/" + _FileName + prefix;
#if !NETFX_CORE
            if (!_UniqueFile)
            {
                string prevFileName = Application.persistentDataPath + "/" + _FileName + "_Prev" + prefix;
                //Delete the prev log file.
                if (File.Exists(prevFileName))
                    File.Delete(prevFileName);
                //Now rename the file.
                if (File.Exists(mFullPath))
#if UNITY_ANDROID || UNITY_IOS || UNITY_METRO
                    File.Move(mFullPath, prevFileName);
#else
                File.Copy(mFullPath, prevFileName, true);
#endif
            }
#endif

            Debug.Log("Logging to file : " + mFullPath);
            FileStream fStream = File.Create(mFullPath);
            mWriter = new StreamWriter(fStream);
            mWriter.WriteLine("Unity Version : " + Application.unityVersion);
            mWriter.WriteLine("Platform : " + Application.platform);
            mWriter.WriteLine("OS Version : " + PlatformUtilities.GetOSVersion());
            mWriter.WriteLine("Product Version : " + ProductSettings.GetProductVersion());
            mWriter.WriteLine("Device Name : " + SystemInfo.deviceName);
            mWriter.WriteLine("Device Model : " + SystemInfo.deviceModel);
            mWriter.WriteLine("System Memory : " + SystemInfo.systemMemorySize + " MB");
            mWriter.WriteLine("Graphics Memory : " + SystemInfo.graphicsMemorySize + " MB");
            mWriter.WriteLine("Build Version : " + ProductSettings.GetVersionInfo());

            mWriter.WriteLine("Server Type : " + PlatformUtilities.GetEnvironment());
            if (PlatformUtilities.IsLocalBuild())
                mWriter.WriteLine("Build Type : Local");
            else
                mWriter.WriteLine("Build Type : OTA");

            mWriter.WriteLine("---------------------------------------------------\n");
            mWriter.AutoFlush = true;
            Application.logMessageReceived += OnDebugLogCallbackHandler;

            return _FileName;
        }

        private void OnUnresolvedExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exception = null;
            exception = (Exception)args.ExceptionObject;
            if (args == null || exception == null)
                return;

            if (exception.GetType() != typeof(Exception)) { return; }

            mNumExceptions++;

            if (mWriter != null)
            {
                try
                {
                    mWriter.WriteLine("UnhandledException");
                    mWriter.WriteLine(exception.Source);
                    mWriter.WriteLine(exception.Message);
                    mWriter.WriteLine(exception.StackTrace);
                    mWriter.Flush();
                }
                catch (Exception e)
                {
                    mWriter.WriteLine(e.Message);
                }
            }
        }

        private void OnDebugLogCallbackHandler(string logString, string inStack, LogType inType)
        {
            //UiConsole.LogHandler(logString, inStack, inType);
#if FIREBASE_CRASHLYTICS_ENABLED
            if (inType == LogType.Exception)
                Firebase.Crashlytics.Crashlytics.LogException(new Exception($"{logString}\n{inStack}"));
            else
                Firebase.Crashlytics.Crashlytics.Log(logString);
#endif //FIREBASE_CRASHLYTICS_ENABLED
            if (_EnableLog)
            {
                if (XDebug.mLogPriority != XDebug.Priority.Always)
                {
                    if (inType == LogType.Log && !_ShowLog)
                        return;

                    if (inType == LogType.Warning && !_ShowWarning)
                        return;

                    if ((inType == LogType.Error || inType == LogType.Exception || inType == LogType.Assert) && !_ShowError)
                        return;
                }

                if (logString.Contains("Exception"))
                {
                    if (!logString.Contains("SocketException") && !logString.Contains("FileNotFoundException") && !logString.Contains("HostException"))
                        mNumExceptions++;
                }
                if (mWriter != null)
                {
                    mWriter.WriteLine($"{DateTimeExtensions.DebugTimeStamp()}:{logString}");

                    if (_ShowStackTrace)
                    {
                        mWriter.WriteLine("");
                        mWriter.WriteLine(inStack);
                    }
                    mWriter.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
                }
            }
        }

        public bool ClearLog()
        {
            if (File.Exists(mFullPath))
            {
                mWriter.Close();
                File.Delete(mFullPath);
                CreateLogFile();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the logs from the log file.
        /// @note: this will close the current log file, read the contents, delete the old log file, create a new log file
        /// </summary>
        /// <returns>byte[] of logs</returns>
        public static byte[] GetLogs()
        {
            byte[] data = null;
            if (mInstance != null && File.Exists(mInstance.mFullPath))
            {
                //Unsubscribe from the logs first.
                Application.logMessageReceived -= mInstance.OnDebugLogCallbackHandler;

                //Close the stream, else we will get access violation exception
                mInstance.mWriter.Close();

                //Read the data from the log file
                data = FileUtilities.ReadBytes(mInstance.mFullPath);

                //Delete the current log file
                File.Delete(mInstance.mFullPath);

                //Create a new log file
                mInstance.CreateLogFile();
            }
            return data;
        }
    }
}
