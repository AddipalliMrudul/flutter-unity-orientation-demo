using System;
using System.Collections.Generic;
using JungleeGames.WebServices;
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class CmdUploadDeviceLogs : Command
    {
        [InjectParameter] protected Dictionary<string, string> mData = null;
        [InjectParameter] protected Action<bool> mCallback = null;
        [InjectParameter] protected string mException = null;

        protected UploadDeviceLogs mUploadDeviceLogs = null;
        protected static string mLastExceptionHash = null;

        //Derived class must call Start()
        public override void Execute()
        {
            if (CanUpload())
                Init();
            else
                Release();
        }

        protected virtual void Init()
        {
            mUploadDeviceLogs = new GameObject("UploadDeviceLogs").AddComponent<UploadDeviceLogs>();
        }

        protected virtual void Start()
        {
            if(mUploadDeviceLogs != null)
                mUploadDeviceLogs.StartUpload(mData, OnUploadDone);
        }

        protected virtual bool CanUpload()
        {
#if UNITY_EDITOR
            return false;
#else
            //This is an explicit upload request like RAP, lets upload
            if (mException.IsNullOrEmpty())
                return true;
            string hash = Utilities.MD5Hash(mException);
            if (hash.Equals(mLastExceptionHash))
            {
                Debug.Log("Similar exception log, skipping upload");
                return false;
            }
            mLastExceptionHash = hash;
            return true;
#endif
        }

        private void OnUploadDone(bool uploaded)
        {
            mCallback?.Invoke(uploaded);
            Release();
        }
    }
}
