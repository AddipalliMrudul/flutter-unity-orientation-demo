using System;

namespace CoralogixCoreSDK
{
  internal interface ILogManager
  {
    bool IsConfigured { get; }

    double DateTimeToUnixTimestamp(DateTime dateTime);

    void SetDebugBreak(bool isEnabled);
  }
}