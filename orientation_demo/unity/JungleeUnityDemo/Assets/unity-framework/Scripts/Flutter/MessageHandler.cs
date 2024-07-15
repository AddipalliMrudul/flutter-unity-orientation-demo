using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace XcelerateGames.FlutterWidget
{
    public class FlutterMessage
    {
        public string type;
        public string data;
    }

    public class FlutterWindowsMessage: FlutterMessage
    {
        public long process_id;

        public FlutterWindowsMessage(){}
        public FlutterWindowsMessage(FlutterMessage flutterMessage)
        {
            type = flutterMessage.type;
            data = flutterMessage.data;
        }

    }

    //public class MessageHandler
    //{
    //    public int id;
    //    public string seq;

    //    public String name;
    //    private JToken data;

    //    public static MessageHandler Deserialize(string message)
    //    {
    //        JObject m = JObject.Parse(message);
    //        MessageHandler handler = new MessageHandler(
    //            m.GetValue("id").Value<int>(),
    //            m.GetValue("seq").Value<string>(),
    //            m.GetValue("name").Value<string>(),
    //            m.GetValue("data")
    //        );
    //        return handler;
    //    }

    //    public T getData<T>()
    //    {
    //        return data.Value<T>();
    //    }

    //    public MessageHandler(int id, string seq, string name, JToken data)
    //    {
    //        this.id = id;
    //        this.seq = seq;
    //        this.name = name;
    //        this.data = data;
    //    }

    //    public void send(object data)
    //    {
    //        JObject o = JObject.FromObject(new
    //        {
    //            id = id,
    //            seq = "end",
    //            name = name,
    //            data = data
    //        });
    //        //UnityMessageManager.Instance.SendMessageToFlutter(UnityMessageManager.MessagePrefix + o.ToString());
    //    }
    //}

    //public class UnityMessage
    //{
    //    public String name;
    //    public JObject data;
    //    public Action<object> callBack;
    //}

    //#if UNITY_IOS || UNITY_TVOS
    //public class NativeAPI
    //{
    //    [DllImport("__Internal")]
    //    public static extern void OnUnityMessage(string message);

    //    [DllImport("__Internal")]
    //    public static extern void OnUnitySceneLoaded(string name, int buildIndex, bool isLoaded, bool IsValid);
    //}
    //#endif
}