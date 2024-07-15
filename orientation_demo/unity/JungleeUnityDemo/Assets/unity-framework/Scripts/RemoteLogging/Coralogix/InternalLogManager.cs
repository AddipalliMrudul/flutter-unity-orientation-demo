using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace CoralogixCoreSDK
{
  public class InternalLogManager : ILogManager
  {
    private Timer _BufferSenderTimer;
    private Hashtable _BulkTemplate;
    private List<Hashtable> _LogEneteries;
    private CoralogixHTTPSender _CoralogixHTTPSender;
    private Stopwatch sw;
    private long _BufferSize;
    private object _BufferLocker;
    private DateTime _TimeDeltaLastUpdate;
    private TimeSpan _ServerTimeDelta;

    public bool IsConfigured { get; private set; }

    internal InternalLogManager()
    {
      this._ServerTimeDelta = TimeSpan.FromMilliseconds(0.0);
      this._TimeDeltaLastUpdate = DateTime.MinValue;
      this._BufferLocker = new object();
      this._BufferSize = 0L;
      this._LogEneteries = new List<Hashtable>(10000);
      this._BulkTemplate = new Hashtable();
      this._BulkTemplate.Add((object) "privateKey", (object) Constants.NO_PRIVATE_KEY);
      this._BulkTemplate.Add((object) "applicationName", (object) Constants.NO_APP_NAME);
      this._BulkTemplate.Add((object) "subsystemName", (object) Constants.NO_SUB_SYSTEM);
      this._BulkTemplate.Add((object) "logEntries", (object) null);
      this._BufferSenderTimer = new Timer((double) Constants.INTERVAL_NORMAL_SEND_SPEED);
      this._BufferSenderTimer.AutoReset = false;
      this._BufferSenderTimer.Elapsed += new ElapsedEventHandler(this.BufferSenderTimer_Elapsed);
      this._BufferSenderTimer.Start();
      this._CoralogixHTTPSender = new CoralogixHTTPSender();
      this.sw = new Stopwatch();
    }

    private void BufferSenderTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        this.SendBulk();
        int num = this._BufferSize > (long) (Constants.MAX_LOG_CHUNK_SIZE / 2) ? Constants.FAST_SEND_SPEED_INTERVAL : Constants.NORMAL_SEND_SPEED_INTERVAL;
        DebugLogger.Instance.Debug(string.Format("Next buffer check is scheduled in {0} seconds", (object) num));
        this._BufferSenderTimer.Interval = (double) num;
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error(ex.ToString());
      }
      finally
      {
        this._BufferSenderTimer.Start();
      }
    }

    private void SendBulk()
    {
      this.UpdateTimeInterval();
      Hashtable bulk;
      lock (this._LogEneteries)
      {
        if (this._LogEneteries.Count == 0)
          return;
        int num1 = 0;
        this.sw.Start();
        long num2;
        for (num2 = 0L; num2 < (long) Constants.MAX_LOG_CHUNK_SIZE && num1 < this._LogEneteries.Count; ++num1)
        {
          string s = this.Serialize((object) this._LogEneteries[num1]);
          num2 += (long) Encoding.UTF8.GetByteCount(s);
        }
        this.sw.Stop();
        DebugLogger.Instance.Info(string.Format("JSON parse duration: {0:g}", (object) TimeSpan.FromTicks(this.sw.ElapsedTicks)));
        this.sw.Reset();
        this._BulkTemplate[(object) "logEntries"] = (object) this._LogEneteries.Take<Hashtable>(num1).ToList<Hashtable>();
        this._LogEneteries.RemoveRange(0, num1);
        this._BufferSize -= num2;
        bulk = (Hashtable) this._BulkTemplate.Clone();
      }
      if (bulk == null || !bulk.ContainsKey((object) "logEntries") || ((IEnumerable<Hashtable>) bulk[(object) "logEntries"]).Count<Hashtable>() <= 0)
        return;
      this._CoralogixHTTPSender.SendBulk(bulk);
    }

    public double DateTimeToUnixTimestamp(DateTime dateTime) => (double) (dateTime.ToUniversalTime().Ticks - 621355968000000000L);

    private void UpdateTimeInterval()
    {
      try
      {
        if (!(DateTime.Now.ToUniversalTime().Subtract(this._TimeDeltaLastUpdate) > TimeSpan.FromMinutes((double) Constants.SYNC_TIME_UPDATE_INTERVAL)))
          return;
        long timeSync = this._CoralogixHTTPSender.GetTimeSync();
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.Add(TimeSpan.FromMilliseconds((double) timeSync));
        DateTime universalTime = DateTime.Now.ToUniversalTime();
        this._TimeDeltaLastUpdate = universalTime;
        this._ServerTimeDelta = dateTime.Subtract(universalTime);
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error("Failed sync time from server");
        DebugLogger.Instance.Error(ex.ToString());
      }
    }

    internal bool AddLogLine(
      string message,
      Severity severity,
      string category = "",
      string className = "",
      string methodName = "",
      string threadId = "")
    {
      try
      {
        double unixTimestamp = this.DateTimeToUnixTimestamp(DateTime.Now);
        if (this._BufferSize < (long) Constants.MAX_LOG_BUFFER_SIZE)
        {
          lock (this._LogEneteries)
          {
            Hashtable hashtable = new Hashtable();
            hashtable.Add((object) "text", (object) message);
            hashtable.Add((object) nameof (severity), (object) severity);
            hashtable.Add((object) nameof (category), (object) category);
            double num = (unixTimestamp - (double) this._ServerTimeDelta.Ticks) / 10000.0;
            hashtable.Add((object) "timestamp", (object) num);
            hashtable.Add((object) nameof (className), (object) className);
            hashtable.Add((object) nameof (methodName), (object) methodName);
            hashtable.Add((object) nameof (threadId), (object) threadId);
            this._BufferSize += (long) Encoding.UTF8.GetByteCount(this.Serialize((object) hashtable));
            this._LogEneteries.Add(hashtable);
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error("Failed adding log line to buffer");
        DebugLogger.Instance.Error(ex.ToString());
      }
      return false;
    }

    internal void DisableProxy() => this._CoralogixHTTPSender.DisableProxy();

    internal void SetProxy(string proxy) => this._CoralogixHTTPSender.SetProxy(proxy);

    private string Serialize(object obj) => JsonConvert.SerializeObject(obj, (Formatting) 1, new JsonSerializerSettings()
    {
      NullValueHandling = (NullValueHandling) 1,
      ReferenceLoopHandling = (ReferenceLoopHandling) 1
    });

    private string GetLocalIPAddress()
    {
      try
      {
        foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
          if (address.AddressFamily == AddressFamily.InterNetwork)
            return address.ToString();
        }
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error("Failed to get IPAdress");
        DebugLogger.Instance.Error(ex.ToString());
      }
      return string.Empty;
    }

    public void SetDebugBreak(bool isEnabled) => this._CoralogixHTTPSender.EnableDebugBreake = isEnabled;

    internal bool Configure(string privateKey, string applicationName, string subsystemName)
    {
      try
      {
        privateKey = string.IsNullOrEmpty(privateKey) ? Constants.NO_PRIVATE_KEY : privateKey;
        applicationName = string.IsNullOrEmpty(applicationName) ? Constants.NO_APP_NAME : applicationName;
        subsystemName = string.IsNullOrEmpty(subsystemName) ? Constants.NO_SUB_SYSTEM : subsystemName;
        this._BulkTemplate[(object) nameof (privateKey)] = (object) privateKey;
        this._BulkTemplate[(object) nameof (applicationName)] = (object) applicationName;
        this._BulkTemplate[(object) nameof (subsystemName)] = (object) subsystemName;
        this._BulkTemplate.Add((object) "computerName", (object) Environment.GetEnvironmentVariable("COMPUTERNAME"));
        this._BulkTemplate.Add((object) "IPAddress", (object) this.GetLocalIPAddress());
        string str = typeof (LogManager).Assembly.GetName().Version.ToString();
        this.AddLogLine(string.Format("The Application Name: {0} and Subsystem Name: {1} from the .Net SDK, version: {2} has started to send data.", (object) applicationName, (object) subsystemName, (object) str), Severity.Info, Constants.CORALOGIX_CATEGORY);
        this.IsConfigured = true;
        return true;
      }
      catch (Exception ex)
      {
        this.IsConfigured = false;
        DebugLogger.Instance.Error("Failed to configure");
        DebugLogger.Instance.Error(ex.ToString());
        return false;
      }
    }
  }
}