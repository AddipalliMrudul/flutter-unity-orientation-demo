using System.Collections;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.IOC;

namespace JungleeGames.UnityDemo
{
    public class OrientationController : BaseBehaviour
    {
        [InjectSignal] private SigOpenLobby mSigOpenLobby = null;
        [InjectSignal] private SigInitDone mSigInitDone = null;
        private Coroutine co_checkOrientation;

        protected override void Awake()
        {
            base.Awake();
            
            mSigOpenLobby.AddListener(OnLobbyOpen);
            mSigInitDone.AddListener(OnInitDone);
        }
        
        protected override void OnDestroy()
        {
            mSigOpenLobby.RemoveListener(OnLobbyOpen);
            mSigInitDone.RemoveListener(OnInitDone);
            StopOrientationCheckCoroutine();
            base.OnDestroy();
        }
        private void OnDisable()
        {
            StopOrientationCheckCoroutine();
        }

        private IEnumerator CheckForOrientationChange()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                var newOrientation = Input.deviceOrientation switch
                {
                    DeviceOrientation.LandscapeRight => ScreenOrientation.LandscapeRight,
                    DeviceOrientation.LandscapeLeft => ScreenOrientation.LandscapeLeft,
                    _ => Screen.orientation
                };
                
                if (Screen.orientation != newOrientation)
                {
                    ChangeOrientation(newOrientation);
                    yield return new WaitUntil(() => Screen.orientation == newOrientation);
                }
            }
        }

        private void ChangeOrientation(ScreenOrientation newScreenOrientation)
        {
            GameUtilities.SetGameOrientation(newScreenOrientation);
        }

        private void OnLobbyOpen()
        {
            gameObject.SetActive(false);
        }

        private void OnInitDone()
        {
            gameObject.SetActive(true);
            StopOrientationCheckCoroutine();
            co_checkOrientation = StartCoroutine(CheckForOrientationChange());
        }

        private void StopOrientationCheckCoroutine()
        {
            if (co_checkOrientation != null)
            {
                StopCoroutine(co_checkOrientation);
                co_checkOrientation = null;
            }
        }
    }
}