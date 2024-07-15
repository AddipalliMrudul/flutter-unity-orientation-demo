using UnityEngine;
using System.Collections;
using System;

namespace XcelerateGames
{
    public class UtBehaviour : MonoBehaviour
    {
        public delegate void OnAppQuitDelegate();
        public static Action<bool> OnAppPauseEvent;

        static readonly UtBehaviour mInstance = Init();
        public static UtBehaviour pInstance
        {
            get { return mInstance; }
        }

        public static OnAppQuitDelegate OnAppQuit;

        public static UtBehaviour Init()
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject("UtBehaviour");
                UtBehaviour instance = go.AddComponent<UtBehaviour>();
                DontDestroyOnLoad(go);
                return instance;
            }

            return mInstance;
        }

        void OnApplicationQuit()
        {
            if (OnAppQuit != null)
                OnAppQuit();

            OnAppQuit = null;
        }

        void OnApplicationPause(bool isPaused)
        {
            if (OnAppPauseEvent != null)
                OnAppPauseEvent(isPaused);
        }

        static public void RunCoroutine(IEnumerator inCorutine)
        {
            if (mInstance == null)
            {
                Debug.LogError("UtBehaviour not initialized.");
                return;
            }

            mInstance.StartCoroutine(inCorutine);
        }

        /// <summary>
        /// Stop all coroutines started from this object.
        /// </summary>
        static public void StopAll()
        {
            if (mInstance == null)
            {
                Debug.LogError("UtBehaviour not initialized.");
                return;
            }

            mInstance.StopAllCoroutines();
        }

        static public void Stop(IEnumerator inCorutine)
        {
            if (mInstance == null)
            {
                Debug.LogError("UtBehaviour not initialized.");
                return;
            }

            mInstance.StopCoroutine(inCorutine);
        }
    }
}
