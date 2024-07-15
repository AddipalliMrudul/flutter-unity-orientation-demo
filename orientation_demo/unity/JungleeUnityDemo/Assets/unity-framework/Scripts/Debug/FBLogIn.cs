#if FB_ENABLED
using Facebook.Unity;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.AssetLoading;
using XcelerateGames.Social.Facebook;
using XcelerateGames.IOC;
using System;

#if UNITY_EDITOR
#if !SIMULATE_FB_SDK
using Facebook.Unity.Editor.Dialogs;
#endif
using System.Reflection;

#endif

/// <summary>
/// Author : Altaf
/// Date : March 23, 2016
/// Purpose : This script is used to simulate FB login on IDE. When connected to FB, on device FB.IsLoggedIn is true,
/// but on IDE we expliccitly have to login, This causes device specific issues. To simulate the same behaviour on IDE add SIMULATE_FB_LOGIN flag
/// Ugage:
///     1. Create a new scene & name it "fblogin"
///     2. Add the newly created scene to editor build settings
///     3. Create an empty game object & add FBLogin script.
///     4. Add the name of the scene to be loaded after FB login done.
///     5. Enable _SimulateFBLogin in ProductSettings asset.
///     6. Save & run
/// </summary>
public class FBLogIn : BaseBehaviour
{
    [SerializeField] public string _OnFBLogin = "game";
#if UNITY_EDITOR
    [SerializeField] bool _AutoLogin = true;
    public const string FBDebugKey = "Debug-FBAccessToken";
#endif  //UNITY_EDITOR

    #region Signals & Models
    [InjectSignal] private SigFacebookLogin mSigFacebookLogin = null;
    #endregion Signals & Models

    private void Start()
    {
        if (PlatformUtilities.IsUnityFacebook() || PlatformUtilities.IsFBSimulation())
        {
            if (FB.IsInitialized)
                OnInitComplete();
            else
                FB.Init(OnInitComplete);
#if UNITY_EDITOR
            if (_AutoLogin)
                AutoLogin();
#endif  //UNITY_EDITOR
        }
    }

    private void OnInitComplete()
    {
        //Supress all FB connected messages
        PlayerPrefs.SetInt(GameConfig.FBLinked, 0);

        bool loggedIn = FB.IsLoggedIn;
        if (AccessToken.CurrentAccessToken != null)
        {
            Debug.LogError("Expiry : " + AccessToken.CurrentAccessToken.ExpirationTime + "\n Now : " + System.DateTime.Now);

            if (AccessToken.CurrentAccessToken.ExpirationTime > System.DateTime.Now)
            {
                Debug.LogError("Logged in & has valid token.");
                //loggedIn = true;
            }
            else
            {
                Debug.LogError("Logged in, but token expired, logging in again");
                loggedIn = false;
            }
        }
        if (loggedIn)
        {
            XDebug.Log("FBLogin : Already logged in", XDebug.Mask.Facebook);

            ResourceManager.LoadScene(_OnFBLogin);
        }
        else
        {
            XDebug.Log("FBLogin : NOT logged in, logging-in now", XDebug.Mask.Facebook);
            mSigFacebookLogin.Dispatch(OnLoginComplete);
        }
    }

    private void OnLoginComplete(bool result, bool cancelled)
    {
        if (!result)
        {
            Debug.LogError("Error while logging in.");
#if UNITY_EDITOR
            PlayerPrefs.DeleteKey(FBDebugKey);
#endif //UNITY_EDITOR
            Application.Quit();
        }
        else
        {
#if UNITY_EDITOR
            if (AccessToken.CurrentAccessToken != null)
                PlayerPrefs.SetString(FBDebugKey, AccessToken.CurrentAccessToken.TokenString);
#endif //UNITY_EDITOR
            Debug.Log("FB login complete, loading next scene");
            //if (FacebookManager.Instance != null && FacebookManager.pIsLoggedIn)
            //    FacebookManager.Instance.OnLoginToFBComplete();
            ResourceManager.LoadScene(_OnFBLogin);
        }
    }

#if UNITY_EDITOR

    private void AutoLogin()
    {
        if (PlayerPrefs.HasKey(FBDebugKey))
            SetFBAccessToken(PlayerPrefs.GetString(FBDebugKey), true);
    }

    public static bool SetFBAccessToken(string accessToken, bool applyAndRun)
    {
#if !SIMULATE_FB_SDK
        string objectName = "UnityFacebookSDKPlugin";
        GameObject gameO = GameObject.Find(objectName);
        if (gameO != null)
        {
            MonoBehaviour[] components = gameO.GetComponents<MonoBehaviour>();
            string componentName = "MockLoginDialog";
            MonoBehaviour dlg = Array.Find(components, e => e.GetType().Name.Contains(componentName));
            if (dlg != null)
            {
                FieldInfo[] fields = (dlg.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
                FieldInfo field = Array.Find(fields, e => e.Name == "accessToken");
                if (field != null)
                {
                    field.SetValue(dlg, accessToken);
                    if (applyAndRun)
                    {
                        dlg.SendMessage("SendSuccessResult");
                        dlg.enabled = false;
                    }
                    return true;
                }
                else
                    Debug.LogError($"Could not find field \"accessToken\" under {componentName}");
            }
            else
                Debug.LogError($"Error! Could not find componenet of type {componentName}");
        }
        else
            Debug.LogError($"Could not find GameObject with name {objectName}");
#endif
        return false;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        SetFBAccessToken("EAAFr4O9uiSwBAL4cnKZA5TJR2o4ViFCZBQdbLX6dZBM2pkRk0q4EuATBRYDUonzdfdxjwppZAqZA1mqjFJqsYAhUEiMZBkPuIiykNv6ALROPyyNjTLKpVfmNdoVu8DMlOX3TgZChSzy1fzRyDlbSZAe2tw42MOFteLUZAgHVX0bdAEONy4GX11Jr4NyRWEiZAZAMVOY754Yf41ZCXKZAhoGVZAgPPSvQkqBoSYOaq01lVjFIHPgZCJuh5MgxoSi", true);
    //    }
    //}

#endif  //UNITY_EDITOR
}
#endif //FB_ENABLED