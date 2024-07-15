using UnityEngine;
using System.Net;
using System.Text;
using System;
using System.IO;

namespace XcelerateGames.Editor
{
    public class WebRequestHandler
    {
        string mDestinationURL;
        Action<string, WebHeaderCollection> mSuccessCallback;
        Action<string> mFailCallback;
        Action<float> mProgressCallback;

        HttpWebRequest _request;
        //IAsyncResult _responseAsyncResult;
        public HttpWebRequest Request => _request;

        public WebRequestHandler(string destinationUrl, Action<string, WebHeaderCollection> successCallback, Action<string> failCallback, Action<float> progressCallback)
        {
            mDestinationURL = destinationUrl;
            mSuccessCallback = successCallback;
            mFailCallback = failCallback;
            mProgressCallback = progressCallback;
            _request = (HttpWebRequest)WebRequest.Create(mDestinationURL);
        }

        public void Run(string data)
        {
            Debug.Log($"Calling {mDestinationURL} with data: \n{data}");
            _request.Method = "POST";

            // Content type is JSON.
            _request.ContentType = "application/json";

            // Fill body.
            byte[] contentBytes = new UTF8Encoding().GetBytes(data);
            _request.ContentLength = contentBytes.LongLength;
            _request.GetRequestStream().Write(contentBytes, 0, contentBytes.Length);

            try
            {
                //_responseAsyncResult = _request.BeginGetResponse(new AsyncCallback(Progress), null);
                using (HttpWebResponse response = (HttpWebResponse)_request.GetResponse())
                {
                    if ((int)response.StatusCode == 200)
                    {
                        Stream dataStream = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(dataStream, Encoding.UTF8);
                        string webResponse = streamReader.ReadToEnd();
                        Debug.Log($"URL : {mDestinationURL} \n Response : {webResponse}");
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
            //Debug.LogError(_responseAsyncResult.IsCompleted);
            //HttpWebResponse response = (HttpWebResponse)state;
            //var response = _request.EndGetResponse(_responseAsyncResult) as HttpWebResponse;
            //long contentLength = response.ContentLength;
            //if (contentLength == -1)
            //{
            //    // You'll have to figure this one out.
            //}
            //Stream responseStream = response.GetResponseStream();
            //GetContentWithProgressReporting(responseStream, contentLength);
            //response.Close();

            mProgressCallback?.Invoke(0);
        }
    }
}
