
using UnityEngine;
using System.Net;
using System.Text;
using System;
using System.IO;
using System.Collections;
using UnityEngine.UI;

namespace XcelerateGames.Webrequets
{
    public class WebRequestHandlerAsync
    {
        string mDestinationURL;
        string mData = null;
        Action<string, WebHeaderCollection> mSuccessCallback;
        Action<string> mFailCallback;
        Action<float> mProgressCallback;

        HttpWebRequest _request;
        IAsyncResult _responseAsyncResult;
        public HttpWebRequest Request => _request;

        public WebRequestHandlerAsync(string destinationUrl, Action<string, WebHeaderCollection> successCallback, Action<string> failCallback, Action<float> progressCallback)
        {
            mDestinationURL = destinationUrl;
            mSuccessCallback = successCallback;
            mFailCallback = failCallback;
            mProgressCallback = progressCallback;
            _request = (HttpWebRequest)WebRequest.Create(mDestinationURL);
        }

        public void Run(string data, string cookie)
        {
            Debug.Log($"Calling {mDestinationURL} with data: \n{data}");
            _request.Method = "POST";
            mData = data;

            // Content type is JSON.
            _request.ContentType = "application/json";
            Debug.LogError("here 1");
            _responseAsyncResult = _request.BeginGetResponse(new AsyncCallback(GetRequestStreamCallback), _request);
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // End the operation
            Stream postStream = request.EndGetRequestStream(asynchronousResult);
            // Fill body.
            byte[] contentBytes = new UTF8Encoding().GetBytes(mData);
            _request.ContentLength = contentBytes.LongLength;
            //if (!cookie.IsNullOrEmpty())
            //    _request.Headers.Add("cookie", cookie);
            postStream.Write(contentBytes, 0, mData.Length);
            postStream.Close();
            Debug.LogError("here 2");
            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // End the operation
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            string responseString = streamRead.ReadToEnd();
            // Close the stream object
            streamResponse.Close();
            streamRead.Close();

            // Release the HttpWebResponse
            response.Close();
            Debug.LogError(responseString);
            mSuccessCallback?.Invoke(responseString, response.Headers);
        }
    }
}