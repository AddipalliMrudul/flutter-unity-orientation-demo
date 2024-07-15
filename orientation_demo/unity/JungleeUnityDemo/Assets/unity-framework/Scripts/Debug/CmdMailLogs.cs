using System.Collections.Generic;
using UnityEngine;
#if USE_FLUTTER_TO_MAIL_LOGS
using XcelerateGames.FlutterWidget;
#endif
using XcelerateGames.IOC;

namespace XcelerateGames.Debugging
{
    public class CmdMailLogs : Command
    {
        [InjectParameter] protected bool mTakeScreenShot = false;
        [InjectParameter] protected (string email, string subject, string message) mMailContent = (null, null, null);

#if USE_FLUTTER_TO_MAIL_LOGS
        [InjectSignal] private SigSendMessageToFlutter mSigSendMessageToFlutter = null;
#endif

        public override void Execute()
        {
            SendEmail();
            base.Execute();
        }

#if USE_FLUTTER_TO_MAIL_LOGS
        protected virtual void SendEmail()
        {
            string[] cc = null;
            string[] bcc = null;
            string ssfilePath = "";
            string logfilename = "";
            if (!LogConsole.FilePath.IsNullOrEmpty())
                logfilename = LogConsole.FilePath;

            Texture2D screenShot = mTakeScreenShot ? ScreenCapture.CaptureScreenshotAsTexture() : null;
            if (screenShot != null)
            {
                byte[] encoded = screenShot.EncodeToPNG();
                string fileName = "Screenshot.png";
                string pathToSave = Application.persistentDataPath;
                ssfilePath = System.IO.Path.Combine(pathToSave, fileName);
                System.IO.File.WriteAllBytes(ssfilePath, encoded);
            }

            FlutterMessage flutterMessage = new FlutterMessage() { type = "MailLogSS" };
            Dictionary<string, string> dataToSend = new Dictionary<string, string>();
            dataToSend.Add("emailitosend", mMailContent.email);
            dataToSend.Add("subject", mMailContent.subject);
            dataToSend.Add("message", mMailContent.message);
            dataToSend.Add("LogFilePath", logfilename);
            dataToSend.Add("SSFilePath", ssfilePath);
            dataToSend.Add("CC", cc.ToJson());
            dataToSend.Add("BCC", bcc.ToJson());
            flutterMessage.data = dataToSend.ToJson();
            mSigSendMessageToFlutter.Dispatch(flutterMessage);
        }
#else
        protected virtual void SendEmail()
        {
            DebugEmail.Send(mMailContent.email, mMailContent.subject, mMailContent.message, mTakeScreenShot);
        }
#endif
    }
}
