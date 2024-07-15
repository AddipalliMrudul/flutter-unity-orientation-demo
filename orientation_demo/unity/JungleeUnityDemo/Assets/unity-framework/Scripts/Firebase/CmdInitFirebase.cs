using UnityEngine;
//using XcelerateGames.DeepLinking;
using XcelerateGames.IOC;

namespace XcelerateGames
{
    public class CmdInitFirebase : Command
    {
        public override void Execute()
        {
            Init();
            base.Execute();
        }

        private void Init()
        {
#if FIREBASE_CRASHLYTICS_ENABLED

            Debug.Log("Initializing Firebase");
            // Initialize Firebase
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    // Crashlytics will use the DefaultInstance, as well;
                    // this ensures that Crashlytics is initialized.
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                    // Set a flag here for indicating that your project is ready to use Firebase.
                    Firebase.Crashlytics.Crashlytics.SetCustomKey("uuid", Utilities.GetUniqueID());
                    Firebase.Crashlytics.Crashlytics.SetCustomKey("build-info", ProductSettings.GetVersionInfo());
                    Debug.Log("Firebase Initializing complete");

                    //new GameObject("FirebaseDynamicLink").AddComponent<FirebaseDynamicLink>();
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    // Firebase Unity SDK is not safe to use here.
                }
            });
#endif
        }
    }
}