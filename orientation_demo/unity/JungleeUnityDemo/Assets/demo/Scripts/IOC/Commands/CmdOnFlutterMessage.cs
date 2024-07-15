using System;
using System.Diagnostics;
using UnityEngine;
using XcelerateGames.FlutterWidget;
using XcelerateGames.IOC;

namespace JungleeGames.UnityDemo
{
    public class CmdOnFlutterMessage : Command
    {
        [InjectParameter] FlutterMessage flutterMessage;
        [InjectModel] private GameDataModel mGameDataModel = null;
        [InjectSignal] private SigSendMessageToFlutter mSigSendMessageToFlutter = null;
        [InjectSignal] private SigInitDone mSigInitDone = null;

        public override void Execute()
        {
            UnityEngine.Debug.Log("DevLogs :: CmdOnFlutterMessage :: " + flutterMessage.type);

            switch (flutterMessage.type)
            {
                case FlutterMessageType.IsUnityReady:
                    OnMessageIsUnityReady();
                    break;
                
                case FlutterMessageType.Init:
                    OnMessageInit();
                    break;
            }
            base.Execute();
        }

        private void OnMessageIsUnityReady()
        {
            if (mGameDataModel.IsUnityReady)
            {
                mSigSendMessageToFlutter.Dispatch(new FlutterMessage() { type = MessageType.UnityReady });
            }
        }
        
        private void OnMessageInit()
        {
            GameUtilities.ToggleFullScreen(true);
            var screenOrientation = ScreenOrientation.LandscapeLeft;
            if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
                screenOrientation = ScreenOrientation.LandscapeRight;
            GameUtilities.SetGameOrientation(screenOrientation);
            mSigInitDone.Dispatch();
        }
    }
}