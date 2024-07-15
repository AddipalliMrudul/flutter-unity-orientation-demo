#if DEV_BUILD || QA_BUILD
#define ENABLE_DEBUG_LOGS
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// This class is used to control logging. We can enable logs of one or more modules by adding module flags
    /// ### Example
    /// @include Masking.cs
    /// </summary>
    public static class XDebug
    {
        /// <summary>
        /// This class holds log data. To be used for custom console window
        /// </summary>
        public class DebugLog
        {
            public LogType logType;
            public string logString = null;
            public string stack = null;
            public XDebug.Mask mask = XDebug.Mask.None;
            public Priority priority;
            public string timeStamp = null;

            /// <summary>
            /// Constructor to create a log entry
            /// </summary>
            /// <param name="log">The obkject to be logged</param>
            /// <param name="stack">stack trace for this log</param>
            /// <param name="mask">XDebug::Mask used</param>
            /// <param name="priority">XDebug::Priority to be used</param>
            /// <param name="logType">LogType</param>
            public DebugLog(object log, string stack, Mask mask, Priority priority, LogType logType)
            {
                this.logType = logType;
                this.mask = mask;
                this.priority = priority;
                this.logString = log.ToString();
                timeStamp = DateTimeExtensions.DebugDateTimeStamp();

                this.stack = stack;
            }

            public override string ToString()
            {
                return $"{logType}:{timeStamp}:{logString}\n\n{stack}\n-----------------------------------------------------------------------------";
            }
        }

        /// <summary>
        /// Mask for each module. Add a mask using AddMask(Mask), AddMask(string) & remove it by calling RemoveMask() function & clear by calling ClearMask()
        /// </summary>
        [Flags]
        public enum Mask
        {
            None = 0,                   /**< No logs*/
            Resources = 1,              /**< All logs related to asset loading*/
            Prefetching = 1 << 1,       /**< All logs related to downloading assets upfront*/
            UpdateManager = 1 << 2,     /**< All logs related to downloading updated assets*/
            Facebook = 1 << 3,          /**< All logs related to FB*/
            WebService = 1 << 4,        /**< All logs related to API/WebService calls*/
            Sound = 1 << 5,             /**< All logs related to sound/audio*/
            Purchasing = 1 << 6,        /**< All logs related to in-app purchasing*/
            Game = 1 << 7,              /**< All logs related to game play*/
            Optimization = 1 << 8,      /**< All logs related to optimization*/
            UI = 1 << 9,                /**< All logs related to UI flow*/
            Tutorials = 1 << 10,        /**< All logs related to tutorials*/
            Notifications = 1 << 11,    /**< All logs related to notifications*/
            Networking = 1 << 12,       /**< All logs related to networking ex: Websockets*/
            Video = 1 << 13,            /**< All logs related to Video*/
            Ads = 1 << 14,              /**< All logs related to Ads*/
            Analytics = 1 << 15,        /**< All logs related to Analytics*/
            IOC = 1 << 16,              /**< All logs related to IOC framework*/
        }

        /// <summary>
        /// Priority of logs
        /// </summary>
        [System.Serializable]
        public enum Priority : int
        {
            Always = 0,     /**< Log always irrespective of masking*/
            Low = 10,       /**< Log everything*/
            Mid = 50,       /**< Log warnings*/
            High = 100,     /**< Errors & Exceptions, This is default setting*/
            None = 1000,    /**< None*/
        }

        #region Member variables

        public static Priority mLogPriority = Priority.High;/**< Log Priority, Set to Priority::High by default*/

        private static Mask mDebugMask = Mask.None;
#if IN_MEMORY_LOGS || UNITY_EDITOR
        private static readonly int mMaxLogCount = 5000; /**< Max no of longs to be maintained. Once this limit is reached, old logs are removed from the list*/

        /**< Masks enabled, Mask.None by default*/

        public static List<DebugLog> mDebugLogs { get; private set; }
#endif
        #endregion Member variables

        #region Setters & Getters

        public static Mask pMask
        {
            get { return mDebugMask; }
        }

        #endregion Setters & Getters

        #region Private Methods

        /// <summary>
        /// Static constructor. Used to initialise DebugLog for custom logger window
        /// </summary>
        static XDebug()
        {
            if (PlayerPrefs.HasKey(GameConfig.Mask))
            {
                AddMask(PlayerPrefs.GetString(GameConfig.Mask));
            }
#if IN_MEMORY_LOGS || UNITY_EDITOR
            mDebugLogs = new List<DebugLog>();
#endif
        }

        /// <summary>
        /// AddLog
        /// </summary>
        /// <param name="log"></param>
        /// <param name="inMask"></param>
        /// <param name="inPriority"></param>
        /// <param name="logType"></param>
        private static void AddLog(object log, Mask inMask, Priority inPriority, LogType logType)
        {
#if IN_MEMORY_LOGS || UNITY_EDITOR
            mDebugLogs.Add(new DebugLog(log, GetStack(), inMask, inPriority, logType));
            if (mDebugLogs.Count > mMaxLogCount)
                mDebugLogs.RemoveAt(0);
#endif
        }

        private static string GetStack()
        {
            List<string> stack = new List<string>(StackTraceUtility.ExtractStackTrace().Split('\n'));
            stack.RemoveRange(0, 3);
            return string.Join("\n", stack.ToArray());
        }

        #endregion Private Methods

        #region Public Methods
        /// <summary>
        /// To be used instead of Debug.Log to be able to control masked logs
        /// </summary>
        /// <param name="log">log to be printed</param>
        /// <param name="inMask">Mask used for masking</param>
        /// <param name="inPriority">log Priority</param>
        public static void Log(object log, Mask inMask = Mask.None, Priority inPriority = Priority.Low)
        {
#if IN_MEMORY_LOGS
            AddLog(log, inMask, inPriority, LogType.Log);
#endif //IN_MEMORY_LOGS
#if ENABLE_DEBUG_LOGS
            if (CanLog(inMask, inPriority))
                UnityEngine.Debug.Log(log);
#endif //ENABLE_DEBUG_LOGS
        }

        /// <summary>
        /// To be used to log data of an IEnumerable to be able to control masked logs
        /// </summary>
        /// <param name="message">message to be printed</param>
        /// <param name="log">IEnumerable objectd</param>
        /// <param name="inMask">Mask used for masking</param>
        /// <param name="inPriority">log Priority</param>
        public static void Log(string message, IEnumerable log, Mask inMask = Mask.None, Priority inPriority = Priority.Low)
        {
            if (CanLog(inMask, inPriority))
            {
                string data = string.Empty;
                foreach (object val in log)
                    data += val.ToString() + ", ";
#if IN_MEMORY_LOGS
                AddLog(message + data, inMask, inPriority, LogType.Log);
#endif
#if ENABLE_DEBUG_LOGS
                Debug.Log(message + data);
#endif //ENABLE_DEBUG_LOGS
            }
        }

        /// <summary>
        /// To be used instead of Debug.LogWarning to be able to control masked logs
        /// </summary>
        /// <param name="log">log to be printed</param>
        /// <param name="inMask">Mask used for masking</param>
        /// <param name="inPriority">log Priority</param>
        public static void LogWarning(object log, Mask inMask = Mask.None, Priority inPriority = Priority.Mid)
        {
#if IN_MEMORY_LOGS
            AddLog(log, inMask, inPriority, LogType.Warning);
#endif
#if ENABLE_DEBUG_LOGS
            if (CanLog(inMask, inPriority))
                UnityEngine.Debug.LogWarning(log);
#endif //ENABLE_DEBUG_LOGS
        }

        /// <summary>
        /// To be used instead of Debug.LogError to be able to control masked logs
        /// </summary>
        /// <param name="log">log to be printed</param>
        /// <param name="inMask">Mask used for masking</param>
        /// <param name="inPriority">log Priority</param>
        public static void LogError(object log, Mask inMask = Mask.None, Priority inPriority = Priority.High)
        {
#if IN_MEMORY_LOGS
            AddLog(log, inMask, inPriority, LogType.Error);
#endif
#if ENABLE_DEBUG_LOGS
            if (CanLog(inMask, inPriority))
                UnityEngine.Debug.LogError(log);
#endif //ENABLE_DEBUG_LOGS
        }

        /// <summary>
        /// To be used instead of Debug.Log to be able to control masked logs
        /// </summary>
        /// <param name="message">message to be printed</param>
        /// <param name="log">log to be printed</param>
        /// <param name="inMask">Mask used for masking</param>
        /// <param name="inPriority">log Priority</param>
        public static void LogError(string message, IEnumerable log, Mask inMask = Mask.None, Priority inPriority = Priority.High)
        {
            if (CanLog(inMask, inPriority))
            {
                string data = string.Empty;
                foreach (object val in log)
                    data += val.ToString() + ", ";
#if IN_MEMORY_LOGS
                AddLog(log, inMask, inPriority, LogType.Error);
#endif
#if ENABLE_DEBUG_LOGS
                Debug.LogError(message + data);
#endif //ENABLE_DEBUG_LOGS
            }
        }

        /// <summary>
        /// To be used instead of Debug.LogException to be able to control masked logs
        /// </summary>
        /// <param name="log">log to be printed</param>
        /// <param name="inMask">Mask used for masking</param>
        /// <param name="inPriority">log Priority</param>
        public static void LogException(System.Exception ex, Priority inPriority = Priority.High)
        {
            AddLog(ex.Message, Mask.None, inPriority, LogType.Exception);
            UnityEngine.Debug.LogException(ex);
        }

        /// <summary>
        /// To be used instead of Debug.v to be able to control masked logs
        /// </summary>
        /// <param name="log">log to be printed</param>
        /// <param name="inMask">Mask used for masking</param>
        /// <param name="inPriority">log Priority</param>
        public static void LogException(string exceptionMessage, Priority inPriority = Priority.High)
        {
            LogException(new System.Exception(exceptionMessage), inPriority);
        }

        /// <summary>
        /// Assert based on a condition with a message
        /// </summary>
        /// <param name="condition"Condition to check></param>
        /// <param name="message">Message to be shown if condition is false</param>
        public static void Assert(bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }

        /// <summary>
        /// Asset based on condition
        /// </summary>
        /// <param name="condition">Condition to check</param>
        public static void Assert(bool condition)
        {
            UnityEngine.Debug.Assert(condition, Environment.StackTrace);
        }

        /// <summary>
        /// Add a mask to print all logs with that module
        /// </summary>
        /// <param name="inMask">Mask to add</param>
        /// ### Example
        /// @code
        /// XDebug.AddMask(XDebug.Mask.Resources);
        /// To add multiple masks
        /// XDebug.AddMask(XDebug.Mask.Resources | XDebug.Mask.Game);
        /// @endcode
        /// @see AddMask(string)
        public static void AddMask(Mask inMask)
        {
            mDebugMask |= inMask;
        }

        /// <summary>
        /// Add masks from an array of CSV strings
        /// </summary>
        /// <param name="inMask">mask(s) to be added</param>
        public static void AddMaskFromArray(string inMask)
        {
            try
            {
                if (!string.IsNullOrEmpty(inMask))
                {
                    string[] masks = inMask.Split(',');
                    foreach (string m in masks)
                        AddMask((Mask)int.Parse(m));
                }
            }
            catch (Exception e)
            {
                XDebug.LogError("AddMaskFromArray:: Failed to Add mask : " + e.Message);
            }
        }

        /// <summary>
        /// Add mask as an array of CSV string
        /// </summary>
        /// <param name="inMask">Mask to be added</param>
        /// @note Mask must be a string in CSV format
        /// @see AddMask(Mask)
        public static void AddMask(string inMask)
        {
            try
            {
                string[] masks = inMask.Split(',');
                foreach (string m in masks)
                    AddMask((Mask)Enum.Parse(typeof(Mask), m, true));
            }
            catch (Exception e)
            {
                XDebug.LogError("AddMask:: Failed to Add mask : " + e.Message);
            }
        }

        /// <summary>
        /// Clear all masks
        /// </summary>
        public static void ClearMask()
        {
            mDebugMask = Mask.None;
        }

        /// <summary>
        /// Remove a given mask
        /// </summary>
        /// <param name="inMask">Mask to be removed</param>
        public static void RemoveMask(Mask inMask)
        {
            mDebugMask &= ~inMask;
        }

        /// <summary>
        /// Checks if the given mask is enabled to masked logs or if priority is high enough to log
        /// </summary>
        /// <param name="inMask">Mask to check</param>
        /// <param name="inPriority">Priority to check</param>
        /// <returns>true if mask is enabled or priority is higher than set priority, else false</returns>
        public static bool CanLog(Mask inMask, Priority inPriority)
        {
            if ((inPriority >= mLogPriority) || (mDebugMask & inMask) != 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if the given mask is enabled
        /// </summary>
        /// <param name="inMask">Mask to check</param>
        /// <returns>true if mask is enabled else false</returns>
        public static bool CanLog(Mask inMask)
        {
            if ((mDebugMask & inMask) != 0 || mLogPriority == Priority.Always)
                return true;
            else
                return false;
        }

#if IN_MEMORY_LOGS || UNITY_EDITOR
        public static void ClearLogs()
        {
            mDebugLogs.Clear();
        }

        /// <summary>
        /// Returns a list of all logs
        /// </summary>
        /// <returns>list of all logs</returns>
        public static List<string> GetLogs()
        {
            List<string> logs = new List<string>();
            for (int i = 0; i < mDebugLogs.Count; ++i)
            {
                logs.Add(mDebugLogs[i].ToString());
            }
            return logs;
        }

        /// <summary>
        /// Returns a string of all logs
        /// </summary>
        /// <returns>string of all logs</returns>
        public static string GetLogString()
        {
            StringBuilder logs = new StringBuilder();
            for (int i = 0; i < mDebugLogs.Count; ++i)
            {
                logs.Append("\n" + mDebugLogs[i]);
            }
            return logs.ToString();
        }
#endif
        #endregion Public Methods
    }
}