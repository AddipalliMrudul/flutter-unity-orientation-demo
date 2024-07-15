using System.Net;
using JungleeGames.Authentication;
using UnityEditor;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.Editor;

namespace JungleeGames.Editor
{
    public class CookieGeneratorTool : EditorWindow
    {
        #region Member Variables
        private string mMobileNumber = null;
        private string mOTP = null;
#if QA_BUILD
        private const string mBaseAPI = "https://api-hzt-inc-qa1.howzat.com/api/";
#else
        private const string mBaseAPI = "https://mercury.howzat.com/api/";
#endif

        #endregion Member Variables

        #region Static Methods
        [MenuItem(Utilities.MenuName + "Cookie Generator", false, 3)]
        public static void OpenFrameworkFeaturesWindow()
        {
            GetWindow<CookieGeneratorTool>(true, "Cookie Generator", true);
        }
#endregion Static Methods

#region Private/Protected Methods

        private void OnGUI()
        {
            mMobileNumber = EditorGUILayout.TextField("Mobile Number: ", mMobileNumber);
            if (GUILayout.Button("Generate OTP"))
            {
                GenerateOTP();
            }
            EditorGUILayout.Space();
            mOTP = EditorGUILayout.TextField("OTP: ", mOTP);
            if (GUILayout.Button("Validate OTP"))
            {
                ValidateOTP();
            }
        }

#region Generate OTP Group
        public void GenerateOTP()
        {
            WebRequestHandler webRequest = new WebRequestHandler(mBaseAPI + "auth/send-otp", OnGenerateOTPSuccess, OnGenerateOTPFail, null);
            WSRequestOTPParams wSRequest = new WSRequestOTPParams(10, "1234") { mobile = mMobileNumber };
            webRequest.Run(wSRequest.ToJson());
        }

        private void OnGenerateOTPSuccess(string response, WebHeaderCollection responseHeaders)
        {
            Debug.Log("OnGenerateOTPSuccess :" + response);
        }

        private void OnGenerateOTPFail(string response)
        {
            Debug.LogError("Failed to generate OTP:" + response);
        }
#endregion Generate OTP Group

#region Generate OTP Group
        private void ValidateOTP()
        {
            WebRequestHandler webRequest = new WebRequestHandler(mBaseAPI + "auth/authenticate-otp/2", OnValidateOTPSuccess, OnValidateOTPFail, null);
            WSPerformOTPSignIn signIn = new WSPerformOTPSignIn(10, "1234") { mobile = mMobileNumber, password = int.Parse(mOTP) };
            webRequest.Run(signIn.ToJson());
        }

        private void OnValidateOTPSuccess(string obj, WebHeaderCollection responseHeaders)
        {
            string cookie = responseHeaders["Set-Cookie"];
            Debug.Log("Cookie :" + cookie);
            GUIUtility.systemCopyBuffer = cookie;

            Debug.Log("Cookie copied to clipboard");
        }

        private void OnValidateOTPFail(string response)
        {
            Debug.LogError("Failed to validate OTP: " + response);
        }
#endregion Generate OTP Group
#endregion Private/Protected Methods
    }
}
