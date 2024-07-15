using XcelerateGames.IOC;

namespace XcelerateGames.FlutterWidget
{
    public class SigOnFlutterMessage : Signal<FlutterMessage> { }
    public class SigSendMessageToFlutter : Signal<FlutterMessage> { }
    public class SigSendLogToFlutter : Signal<FlutterMessage> { }
}
