using System;
using System.IO;

namespace CoralogixCoreSDK
{
  internal class DebugLogger
  {
    private const int MaxLogCount = 3;
    private const int MaxLogSize = 10485760;
    private object _Locker;
    private static DebugLogger _Logger;
    private static object _LoggerLocker = new object();

    private DebugLogger()
    {
      this._Locker = new object();
      this.IsDebugMode = false;
    }

    internal static DebugLogger Instance
    {
      get
      {
        if (DebugLogger._Logger == null)
        {
          lock (DebugLogger._LoggerLocker)
          {
            if (DebugLogger._Logger == null)
              DebugLogger._Logger = new DebugLogger();
          }
        }
        return DebugLogger._Logger;
      }
    }

    public bool IsDebugMode { get; private set; }

    public void SetDebugMode(bool isDebug) => this.IsDebugMode = isDebug;

    private string RollLogFile()
    {
      string searchPattern = string.Format("{0}*.log", (object) Constants.LOG_FILE_NAME);
      int num = 0;
      string[] files = Directory.GetFiles(Environment.CurrentDirectory, searchPattern, SearchOption.TopDirectoryOnly);
      if (files.Length != 0)
      {
        Array.Sort<string>(files, 0, files.Length);
        int index1 = files.Length - 1;
        if (files.Length > 3)
        {
          File.Delete(files[0]);
          for (int index2 = 1; index2 < files.Length; ++index2)
            File.Move(files[index2], files[index2 - 1]);
          --index1;
        }
        string fileName = files[index1];
        if (new FileInfo(fileName).Length < 10485760L)
          return fileName;
        num = index1 + 1;
      }
      return string.Format("{0}{1}{2}{3:00}.log", (object) Environment.CurrentDirectory, (object) Path.DirectorySeparatorChar, (object) Constants.LOG_FILE_NAME, (object) num);
    }

    internal void Log(Severity severity, string message)
    {
      if (!this.IsDebugMode)
        return;
      try
      {
        lock (this._Locker)
        {
          using (StreamWriter streamWriter = new StreamWriter(this.RollLogFile(), true))
          {
            streamWriter.AutoFlush = true;
            streamWriter.WriteLine(string.Format("{0} - {1:u} {2}", (object) severity, (object) DateTime.Now, (object) message));
          }
        }
      }
      catch
      {
      }
    }

    internal void Error(string message) => this.Log(Severity.Error, message);

    internal void Warning(string message) => this.Log(Severity.Warning, message);

    internal void Critical(string message) => this.Log(Severity.Critical, message);

    internal void Debug(string message) => this.Log(Severity.Debug, message);

    internal void Info(string message) => this.Log(Severity.Info, message);

    internal void Verbose(string message) => this.Log(Severity.Verbose, message);
  }
}
