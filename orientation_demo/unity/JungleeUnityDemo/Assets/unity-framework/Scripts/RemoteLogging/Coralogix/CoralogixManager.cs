using System;
using System.Collections.Generic;
using CoralogixCoreSDK;
using JungleeGames;
using UnityEngine;

namespace XcelerateGames.RemoteLogging
{
    /// <summary>
    /// This class is used to send logs to coralogix platform
    /// URL: https://api.app.coralogix.in:443/api/v1/logs
    /// AppName: unity-prod
    /// SubSystemName: unity-execption
    /// </summary>
    public class CoralogixManager : BaseBehaviour
    {
        /// <summary>
        /// Coralogix end point for all envs
        /// </summary>
        [SerializeField] protected EnvironmentPath _ApiUrl = null;
        [SerializeField] protected string _ApiPrivateKey = null;
        [SerializeField] protected string _AppName = null;
        [SerializeField] protected string _SubSystemName = null;
        [SerializeField] private Product _Game = Product.None;
        [SerializeField] private float _TimeBetweenLogs = 1f;

        /// <summary>
        /// This is a coralogix logger instance, use to send the logs to coralogix platform viw rest api
        /// </summary>
        protected CoralogixLogger mLogger = null;

        /// <summary>
        /// this is common meta data which is being included send along with log
        /// </summary>
        protected Dictionary<string, object> mCommonMeta = new Dictionary<string, object>();

        /// <summary>
        /// Stores the time of last sent log
        /// </summary>
        private double _LastLogTime;

        protected override void Awake()
        {
            base.Awake();
            Init(_Game.ToString());
            AddCommonMeta();
            Application.logMessageReceived += OnReceivedLogMessage;
        }

        /// <summary>
        /// Initialized Corralogix Logger Instance 
        /// </summary>
        /// <param name="loggerName"></param>
        protected virtual void Init(string loggerName)
        {
            Environment.SetEnvironmentVariable("CORALOGIX_LOG_URL", _ApiUrl.Path);
            mLogger = CoralogixLogger.GetLogger(loggerName);
            mLogger.Configure(_ApiPrivateKey, _AppName, _SubSystemName);
        }

        protected override void OnDestroy()
        {
            Application.logMessageReceived -= OnReceivedLogMessage;
            base.OnDestroy();
        }

        /// <summary>
        /// Logger Callback
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        private void OnReceivedLogMessage(string message, string stackTrace, LogType type)
        {
            if (type != LogType.Exception)
                return;
            bool canSendLog = (DateTime.Now.GetEpochTime() - _LastLogTime) > _TimeBetweenLogs;
            if (canSendLog)
            {
                _LastLogTime = DateTime.Now.GetEpochTime();
                LogMessage(message, stackTrace);
            }
        }

        /// <summary>
        /// Sending Log Exceptions to coralogix platform
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        protected virtual void LogMessage(string message, string stackTrace)
        {
            mCommonMeta["msg"] = message;
            mCommonMeta["stack-trace"] = stackTrace;
            mLogger?.Critical(mCommonMeta.ToJson());
        }

        /// <summary>
        /// Add/update the key & value to common meta data
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected virtual void AddMeta(string key, object value)
        {
            mCommonMeta[key] = value;
        }

        /// <summary>
        /// Remove meta data from common meta data for given input key
        /// </summary>
        /// <param name="key"></param>
        protected virtual bool RemoveMeta(string key)
        {
            if (mCommonMeta.ContainsKey(key))
                return mCommonMeta.Remove(key);
            return false;
        }

        /// <summary>
        /// Add/update the given dictionary's data to common meta data
        /// </summary>
        /// <param name="keyValues"></param>
        protected virtual void AddMeta(Dictionary<string, object> keyValues)
        {
            foreach (var item in keyValues)
                mCommonMeta[item.Key] = item.Value;
        }

        /// <summary>
        /// adding common meta data during initialisation
        /// </summary>
        protected virtual void AddCommonMeta()
        {
            mCommonMeta["game-name"] = _Game.ToString();
            //TODO: Fetch Current Updated App Verison across games
            mCommonMeta["build-version"] = ProductSettings.GetVersionInfo();
            mCommonMeta["device-model"] = SystemInfo.deviceModel;
            mCommonMeta["device-os-version"] = PlatformUtilities.GetOSVersion();
            mCommonMeta["server-type"] = PlatformUtilities.GetEnvironment().ToString();
        }
    }
}
