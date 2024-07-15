using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace CoralogixCoreSDK
{
  internal class CoralogixHTTPSender
  {
    private WebProxy _WebProxy;
    private Stopwatch sw;
    private object _GetTimeObjectLocker;
    private long _ServerMiliSeconds;

    internal CoralogixHTTPSender()
    {
      this.sw = new Stopwatch();
      this._GetTimeObjectLocker = new object();
      this.EnableDebugBreake = false;
    }

    internal void SetProxy(string proxyAddress) => this._WebProxy = new WebProxy(proxyAddress);

    internal void SetProxy(WebProxy proxy) => this._WebProxy = proxy;

    internal void DisableProxy() => this._WebProxy = (WebProxy) null;

    private HttpWebRequest GetWebRequest(string url, string method)
    {
      HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(url);
      webRequest.Proxy = (IWebProxy) this._WebProxy;
      webRequest.Timeout = Constants.HTTP_TIMEOUT * 1000;
      webRequest.ContentType = "application/json";
      webRequest.Method = method;
      webRequest.KeepAlive = true;
      return webRequest;
    }

    public long GetTimeSync()
    {
      try
      {
        DebugLogger.Instance.Info("GetTimeSync - Enter");
        lock (this._GetTimeObjectLocker)
        {
          int num1 = 0;
          while (num1 < Constants.HTTP_SEND_RETRY_COUNT)
          {
            ++num1;
            try
            {
              using (StreamReader streamReader = new StreamReader(this.GetWebRequest(Constants.CORALOGIX_TIME_DELTA_URL, "GET").GetResponse().GetResponseStream()))
              {
                string end = streamReader.ReadToEnd();
                streamReader.Close();
                long num2 = long.Parse(end) / 10000L;
                DebugLogger.Instance.Info(string.Format("Successfully read time from Coralogix server. Result is: #{0}", (object) end));
                DebugLogger.Instance.Info("GetTimeSync - Exit");
                this._ServerMiliSeconds = num2;
              }
            }
            catch (Exception ex)
            {
              DebugLogger.Instance.Error(ex.ToString());
            }
          }
        }
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error("Failed GetTimeSync");
        DebugLogger.Instance.Error(ex.ToString());
      }
      return this._ServerMiliSeconds;
    }

    public long ServerTimeInMiliSeconds => this._ServerMiliSeconds;

    public bool EnableDebugBreake { get; set; }

    public bool SendBulk(Hashtable bulk)
    {
      DebugLogger.Instance.Info("SendBulk - Enter");
      try
      {
        int num = 0;
        this.sw.Reset();
        while (num < Constants.HTTP_SEND_RETRY_COUNT)
        {
          ++num;
          DebugLogger.Instance.Info(string.Format("About to send bulk to Coralogix server.Attempt number: {0}", (object) num));
          try
          {
            string str = this.Serialize((object) bulk);
            this.sw.Start();
            HttpWebRequest webRequest = this.GetWebRequest(Constants.CORALOGIX_LOG_URL, "POST");
            if (this.EnableDebugBreake)
            {
              Debugger.Log(1, "Test", "test");
              Debugger.Break();
            }
            using (Stream requestStream = webRequest.GetRequestStream())
            {
              using (StreamWriter streamWriter = new StreamWriter(requestStream))
              {
                streamWriter.Write(str);
                streamWriter.Flush();
                streamWriter.Close();
              }
            }
            this.sw.Stop();
            DebugLogger.Instance.Info(string.Format("Request send duration: {0:g}", (object) TimeSpan.FromTicks(this.sw.ElapsedTicks)));
            using (StreamReader streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
              string end = streamReader.ReadToEnd();
              streamReader.Close();
              DebugLogger.Instance.Info(string.Format("Successfully sent bulk to Coralogix server. Result is: {0}", (object) end));
            }
            DebugLogger.Instance.Info("SendBulk - Exit, Success:true");
            return true;
          }
          catch (Exception ex)
          {
            DebugLogger.Instance.Error(ex.ToString());
          }
          Thread.Sleep(Constants.HTTP_SEND_RETRY_INTERVAL);
        }
      }
      catch (Exception ex)
      {
        DebugLogger.Instance.Error("Failed sending bulk");
        DebugLogger.Instance.Error(ex.ToString());
      }
      DebugLogger.Instance.Info("SendBulk - Exit, Success:false");
      return false;
    }

    private string Serialize(object obj) => JsonConvert.SerializeObject(obj, (Formatting) 1, new JsonSerializerSettings()
    {
      NullValueHandling = (NullValueHandling) 1,
      ReferenceLoopHandling = (ReferenceLoopHandling) 1
    });
  }
}