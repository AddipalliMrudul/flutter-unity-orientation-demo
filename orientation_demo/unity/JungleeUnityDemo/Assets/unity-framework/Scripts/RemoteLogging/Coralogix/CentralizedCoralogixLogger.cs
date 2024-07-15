using System;
using System.Collections.Generic;
using System.Threading;

namespace CoralogixCoreSDK
{
  public class CentralizedCoralogixLogger
  {
    private Dictionary<string, InternalLogManager> _ConfiguredLoggersDictionary;
    private string _privateKey;

    public CentralizedCoralogixLogger(string privateKey)
    {
      this._privateKey = privateKey;
      this._ConfiguredLoggersDictionary = new Dictionary<string, InternalLogManager>();
    }

    public bool Configure(string applicationName, string subsystemName)
    {
      try
      {
        lock (this._ConfiguredLoggersDictionary)
        {
          InternalLogManager logger = this.GetLogger(applicationName, subsystemName);
          if (!logger.IsConfigured)
            return logger.Configure(this._privateKey, applicationName, subsystemName);
          DebugLogger.Instance.Info(string.Format("Already conigured! params: privateKey:{0}, applicationName:{1}, subsystemName:{2}", (object) this._privateKey, (object) applicationName, (object) subsystemName));
          return true;
        }
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error(string.Format("Failed configurating logger! params: privateKey:{0}, applicationName:{1}, subsystemName:{2}", (object) this._privateKey, (object) applicationName, (object) subsystemName));
        DebugLogger.Instance.Error(ex.ToString());
        return false;
      }
    }

    private InternalLogManager GetLogger(string applicationName, string subsystemName)
    {
      string key = string.Format("{0}_{1}", (object) applicationName, (object) subsystemName);
      InternalLogManager logger;
      if (!this._ConfiguredLoggersDictionary.TryGetValue(key, out logger))
      {
        logger = new InternalLogManager();
        this._ConfiguredLoggersDictionary.Add(key, logger);
      }
      return logger;
    }

    public void Log(
      string applicationName,
      string subsystemName,
      Severity severity,
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      try
      {
        lock (this._ConfiguredLoggersDictionary)
        {
          InternalLogManager logger = this.GetLogger(applicationName, subsystemName);
          if (!logger.IsConfigured)
            logger.Configure(this._privateKey, applicationName, subsystemName);
          category = string.IsNullOrEmpty(category) ? Constants.NO_CATEGORY_SYSTEM : category;
          threadId = string.IsNullOrEmpty(threadId) ? Thread.CurrentThread.ManagedThreadId.ToString() : threadId;
          message = string.IsNullOrEmpty(message) ? Constants.EMPTY_STRING : message;
          logger.AddLogLine(message, severity, category, className, methodName, threadId);
        }
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error("Failed logging, add log line");
        DebugLogger.Instance.Error(ex.ToString());
      }
    }

    public void Info(
      string applicationName,
      string subsystemName,
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(applicationName, subsystemName, Severity.Info, message, category, className, methodName, threadId);
    }

    public void Warning(
      string applicationName,
      string subsystemName,
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(applicationName, subsystemName, Severity.Warning, message, category, className, methodName, threadId);
    }

    public void Verbose(
      string applicationName,
      string subsystemName,
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(applicationName, subsystemName, Severity.Verbose, message, category, className, methodName, threadId);
    }

    public void Error(
      string applicationName,
      string subsystemName,
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(applicationName, subsystemName, Severity.Error, message, category, className, methodName, threadId);
    }

    public void Critical(
      string applicationName,
      string subsystemName,
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(applicationName, subsystemName, Severity.Critical, message, category, className, methodName, threadId);
    }

    public void Debug(
      string applicationName,
      string subsystemName,
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(applicationName, subsystemName, Severity.Debug, message, category, className, methodName, threadId);
    }
  }
}