//using System;
//using System.Collections;
////using Firebase.DynamicLinks;
//using UnityEngine;
//using UnityEngine.UI;
//using XcelerateGames.IOC;
//using XcelerateGames.UI;

//namespace XcelerateGames.DeepLinking
//{
//     public class FirebaseDynamicLink : BaseBehaviour
//     {
//        #region Properties
//        #endregion //Properties

//        #region Signals
//        #endregion //Signals

//        #region UI Callbacks
//        #endregion //UI Callbacks

//        #region Private Methods
//        protected override void Awake()
//        {
//            base.Awake();
//        }
        
//        private void OnDestroy()
//        {
//        }

//        void Start()
//        {
//            Debug.Log($"FirebaseDynamicLink created");
//            //DynamicLinks.DynamicLinkReceived += OnDynamicLink;
//            DontDestroyOnLoad(gameObject);
//        }

//        // Display the dynamic link received by the application.
//        void OnDynamicLink(object sender, EventArgs args)
//        {
//            //var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
//            //Debug.Log($"Received dynamic link {dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString}");
//        }

//        #endregion //Private Methods

//        #region Public Methods
//        #endregion //Public Methods
//    }
//}
