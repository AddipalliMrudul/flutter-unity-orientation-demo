using System;
using System.IO;
using System.Net;
using System.Text;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

//using WebRequest = System.Net.WebRequest;
using UnityEngine;

namespace XcelerateGames.Editor.Locale
{
	public class CSVHelper
	{
		public static bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			bool isOk = true;
			// If there are errors in the certificate chain, look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None) {
				for (int i=0; i<chain.ChainStatus.Length; i++) {
					if (chain.ChainStatus [i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						bool chainIsValid = chain.Build ((X509Certificate2)certificate);
						if (!chainIsValid) {
							isOk = false;
						}
					}
				}
			}

			Debug.Log("RemoteCertificateValidation, OK: " +isOk);
			return isOk;
		}
		
		public static string FetchCSV(string sheet_src, string[] targetpaths, string sheet_name = "")
		{
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback; //fix for mono on windows

			
			string url =  sheet_src +"/gviz/tq?tqx=out:csv";

			if(!string.IsNullOrEmpty(sheet_name))
				url = url + "&sheet=" +sheet_name;

			WebClientEx wc = new WebClientEx(new CookieContainer());
            Debug.Log("CSV url : " + url);

            byte[] byte_data;
          	try
          	{
				byte_data = wc.DownloadData(url);
			}
			catch (System.Net.WebException e)
            {

            Debug.LogError("Error downloading CSV: " +e);
            return null;

			}

			//convert encoding to utf8 without bom
			Encoding utf8noBOM = new UTF8Encoding(false);  
			var fetched_csv_data = utf8noBOM.GetString(byte_data ?? new byte[] {});

			if(string.IsNullOrEmpty(fetched_csv_data))
			{
				Debug.LogError("GSheetResponse is empty");
				return null;
			}

			foreach(var p in targetpaths)
			{
				var target = "Assets/" +p;

				Debug.Log("Writing Data to: "+target);
				File.WriteAllText(target, fetched_csv_data, utf8noBOM);
			}

			return fetched_csv_data;
		}

		public class WebClientEx : WebClient
		{
			public WebClientEx(CookieContainer container)
			{
				this.container = container;
			}

			private readonly CookieContainer container = new CookieContainer();

			protected override System.Net.WebRequest GetWebRequest(Uri address)
			{
				System.Net.WebRequest r = base.GetWebRequest(address);
				var request = r as HttpWebRequest;
				if (request != null)
				{
					request.CookieContainer = container;
				}
				return r;
			}

			protected override System.Net.WebResponse GetWebResponse(System.Net.WebRequest request, IAsyncResult result)
			{
				System.Net.WebResponse response = base.GetWebResponse(request, result);
				ReadCookies(response);
				return response;
			}

			protected override System.Net.WebResponse GetWebResponse(System.Net.WebRequest request)
			{
				System.Net.WebResponse response = base.GetWebResponse(request);
				ReadCookies(response);
				return response;
			}

			private void ReadCookies(System.Net.WebResponse r)
			{
				var response = r as HttpWebResponse;
				if (response != null)
				{
					CookieCollection cookies = response.Cookies;
					container.Add(cookies);
				}
			}
		}
	}

	#region EXPERIMENT
//			public static void Main()
//	        {
//				Debug.Log("Starting fetch");
//	            /*
//	             1. Your Google SpreadSheet document must be set to 'Anyone with the link' can view it
//	
//	             2. To get URL press SHARE (top right corner) on Google SpreeadSheet and copy "Link to share".
//	
//	             3. Now add "&output=csv" parameter to this link
//	
//	             4. Your link will look like:
//	
//	                https://docs.google.com/spreadsheet/ccc?key=1234abcd1234abcd1234abcd1234abcd1234abcd1234&usp=sharing&output=csv
//	            */
//				string url = @"https://docs.google.com/spreadsheets/d/1zMHUmB8e_JIcu2OlfSI__NRGJLg-r5HzO5TvBabUzlQ/export?output=csv"; // REPLACE THIS WITH YOUR URL
//	
//	            WebClientEx wc = new WebClientEx(new CookieContainer());
//	            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:22.0) Gecko/20100101 Firefox/22.0");
//	            wc.Headers.Add("DNT", "1");
//				wc.Headers.Add("Accept", "text/plain;q=0.9,*/*;q=0.8");
//	            wc.Headers.Add("Accept-Encoding", "deflate");
//	            wc.Headers.Add("Accept-Language", "en-US,en;q=0.5");
//	
//				byte[] dt = wc.DownloadData(url);
//				var outputCSVdata = System.Text.Encoding.UTF8.GetString(dt ?? new byte[] {});
//	        }
	#endregion
}
