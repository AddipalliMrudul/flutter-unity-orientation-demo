namespace CoralogixCoreSDK
{
  internal class LogManager
  {
    private static InternalLogManager _Instance;
    private static object _SingletonLocker = new object();

    private LogManager()
    {
    }

    public static InternalLogManager Instance
    {
      get
      {
        if (LogManager._Instance == null)
        {
          lock (LogManager._SingletonLocker)
          {
            if (LogManager._Instance == null)
              LogManager._Instance = new InternalLogManager();
          }
        }
        return LogManager._Instance;
      }
    }
  }
}