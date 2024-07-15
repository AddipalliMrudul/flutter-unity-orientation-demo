using UnityEngine;
using System.Net;
using System.Text;
using System;
using System.IO;

namespace XcelerateGames.Webrequets
{
    public class WebRequestHandler2
    {
        string mDestinationURL;
        Action<string, WebHeaderCollection> mSuccessCallback;
        Action<string> mFailCallback;
        Action<float> mProgressCallback;

        HttpWebRequest _request;
        public HttpWebRequest Request => _request;

        public WebRequestHandler2(string destinationUrl, Action<string, WebHeaderCollection> successCallback, Action<string> failCallback, Action<float> progressCallback)
        {
            mDestinationURL = destinationUrl;
            mSuccessCallback = successCallback;
            mFailCallback = failCallback;
            mProgressCallback = progressCallback;
            _request = (HttpWebRequest)WebRequest.Create(mDestinationURL);
        }

        public void Run(string data, string cookie)
        {
            if(XDebug.CanLog(XDebug.Mask.Analytics))
                XDebug.Log($"Calling {mDestinationURL} with data: \n{data}", XDebug.Mask.Analytics);
            _request.Method = "POST";

            // Content type is JSON.
            _request.ContentType = "application/json";

            // Fill body.
            byte[] contentBytes = new UTF8Encoding().GetBytes(data);
            _request.ContentLength = contentBytes.LongLength;
            if (!cookie.IsNullOrEmpty())
                _request.Headers.Add("cookie", cookie);
            _request.GetRequestStream().Write(contentBytes, 0, contentBytes.Length);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)_request.GetResponse())
                {
                    if ((int)response.StatusCode == 200)
                    {
                        Stream dataStream = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(dataStream, Encoding.UTF8);
                        string webResponse = streamReader.ReadToEnd();
                        if(XDebug.CanLog(XDebug.Mask.Analytics))
                            XDebug.Log($"URL : {mDestinationURL} \n Response : {webResponse}", XDebug.Mask.Analytics);
                        response.Close();
                        dataStream.Close();
                        streamReader.Close();
                        OnComplete(webResponse, response.Headers);
                    }
                    else
                    {
                        OnFail(response.StatusDescription);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{mDestinationURL} :{e}");
                OnFail(e.ToString());
            }
            _request.Abort();
        }

        private void OnComplete(string webResponse, WebHeaderCollection responseHeaders)
        {
            mSuccessCallback?.Invoke(webResponse, responseHeaders);
        }

        private void OnFail(string webResponse)
        {
            mFailCallback?.Invoke(webResponse);
        }

        private void Progress(object state)
        {
            mProgressCallback?.Invoke(0);
        }
    }
}