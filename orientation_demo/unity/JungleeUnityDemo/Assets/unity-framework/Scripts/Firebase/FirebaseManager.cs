#if USE_FIREBASE
using UnityEngine;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    /// <summary>
    /// https://firebase.blog/posts/2019/07/firebase-and-tasks-how-to-deal-with
    /// </summary>
    public class FirebaseManager : BaseBehaviour
    {
        [SerializeField] private bool _ReportUncaughtExceptionsAsFatal = false;

        [InjectSignal] private SigFirebaseInitialized mSigFirebaseInitialized = null;

        async void Start()
        {
            DontDestroyOnLoad(gameObject);

            Firebase.DependencyStatus dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
            bool success = false;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;
                // When this property is set to true, Crashlytics will report all
                // uncaught exceptions as fatal events. This is the recommended behavior.
                Firebase.Crashlytics.Crashlytics.ReportUncaughtExceptionsAsFatal = _ReportUncaughtExceptionsAsFatal;
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                Debug.Log("Firebase init done");
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                success = true;
            }
            else
            {
                UnityEngine.Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                // Firebase Unity SDK is not safe to use here.
            }
            mSigFirebaseInitialized.Dispatch(success);
        }
    }
}
#endif //USE_FIREBASE
