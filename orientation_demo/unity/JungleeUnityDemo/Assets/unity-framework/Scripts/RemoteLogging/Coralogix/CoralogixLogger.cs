using Newtonsoft.Json;
using System;
using System.Threading;

namespace CoralogixCoreSDK
{
  public class CoralogixLogger
  {
    private string _Category;

    private CoralogixLogger(string name) => this._Category = name;

    public void DisableProxy() => LogManager.Instance.DisableProxy();

    public bool Configure(string privateKey, string applicationName, string subsystemName)
    {
      try
      {
        if (!LogManager.Instance.IsConfigured)
          return LogManager.Instance.Configure(privateKey, applicationName, subsystemName);
        DebugLogger.Instance.Info(string.Format("Already conigured! params: privateKey:{0}, applicationName:{1}, subsystemName:{2}", (object) privateKey, (object) applicationName, (object) subsystemName));
        return true;
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error(string.Format("Failed configurating logger! params: privateKey:{0}, applicationName:{1}, subsystemName:{2}", (object) privateKey, (object) applicationName, (object) subsystemName));
        DebugLogger.Instance.Error(ex.ToString());
        return false;
      }
    }

    public void SetDebugBreak(bool isEnabled) => LogManager.Instance.SetDebugBreak(isEnabled);

    public void SetProxy(string proxy) => LogManager.Instance.SetProxy(proxy);

    public bool IsDebug => DebugLogger.Instance.IsDebugMode;

    public void SetDebugMode(bool isDebug) => DebugLogger.Instance.SetDebugMode(isDebug);

    public static CoralogixLogger GetLogger(string name) => new CoralogixLogger(name);

    public void Log(
      Severity severity,
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      try
      {
        category = string.IsNullOrEmpty(category) ? this._Category : category;
        threadId = string.IsNullOrEmpty(threadId) ? Thread.CurrentThread.ManagedThreadId.ToString() : threadId;
        message = string.IsNullOrEmpty(message) ? Constants.EMPTY_STRING : message;
        LogManager.Instance.AddLogLine(message, severity, category, className, methodName, threadId);
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error("Failed logging, add log line");
        DebugLogger.Instance.Error(ex.ToString());
      }
    }

    public void Info(
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(Severity.Info, message, category, className, methodName, threadId);
    }

    public void Warning(
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(Severity.Warning, message, category, className, methodName, threadId);
    }

    public void Verbose(
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(Severity.Verbose, message, category, className, methodName, threadId);
    }

    public void Error(
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(Severity.Error, message, category, className, methodName, threadId);
    }

    public void Critical(
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(Severity.Critical, message, category, className, methodName, threadId);
    }

    public void Debug(
      string message,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      this.Log(Severity.Debug, message, category, className, methodName, threadId);
    }

    private string Serialize(object obj) => JsonConvert.SerializeObject(obj, (Formatting) 1, new JsonSerializerSettings()
    {
      NullValueHandling = (NullValueHandling) 1,
      ReferenceLoopHandling = (ReferenceLoopHandling) 1
    });
  }
}