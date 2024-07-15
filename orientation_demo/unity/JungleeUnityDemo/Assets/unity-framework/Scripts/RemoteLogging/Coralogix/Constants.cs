using System;

namespace CoralogixCoreSDK
{
  public static class Constants
  {
    public static readonly string EMPTY_STRING = nameof (EMPTY_STRING);
    public static readonly int MAX_LOG_BUFFER_SIZE = 12582912;
    public static readonly int MAX_LOG_CHUNK_SIZE = 1572864;
    public static readonly int INTERVAL_NORMAL_SEND_SPEED = 500;
    public static readonly int INTERVAL_FAST_SEND_SPEED = 100;
    public static readonly int HTTP_TIMEOUT = 30;
    public static readonly int HTTP_SEND_RETRY_COUNT = 5;
    public static readonly int HTTP_SEND_RETRY_INTERVAL = 2;
    public static readonly string CORALOGIX_TIME_DELTA_URL = Environment.GetEnvironmentVariable(nameof (CORALOGIX_TIME_DELTA_URL)) ?? "https://api.coralogix.com:443/sdk/v1/time";
    public static readonly string CORALOGIX_CATEGORY = "CORALOGIX";
    public static readonly int SYNC_TIME_UPDATE_INTERVAL = 5;
    public static readonly string CORALOGIX_LOG_URL = Environment.GetEnvironmentVariable(nameof (CORALOGIX_LOG_URL)) ?? "https://api.coralogix.com:443/api/v1/logs";
    public static readonly string NO_PRIVATE_KEY = "6411e033-3439-d71c-542b-0d45419d6b30";
    public static readonly string NO_APP_NAME = "FAILED_APP_NAME";
    public static readonly string NO_SUB_SYSTEM = "FAILED_SUB_NAME";
    public static readonly string NO_CATEGORY_SYSTEM = "FAILED_CATEGORY_NAME";
    public static readonly string LOG_FILE_NAME = "coralogix.sdk.log";
    public static readonly int NORMAL_SEND_SPEED_INTERVAL = 500;
    public static readonly int FAST_SEND_SPEED_INTERVAL = 100;
  }
}