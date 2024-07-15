using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.UI
{
    public class UiLoadingScreen : UiBase
    {
        public UiProgressBar _ProgressBar = null;

        public System.Action _OnLoadingComplete = null;

        public float _AutoProgressTime = 0.5f;
        [Range(0, 1)]
        public float _MaxAutoProgress = 0.9f;
        [Range(0, 1)]
        public float _AutoProgressStep = 0.01f;

        private float mProgress = 0f;
        private float mElapsedTime = 0f;
        public List<string> _NotchDevices;
        public Vector3 _NotchDevicesOffset = new Vector3(0, 76, 0);

        protected override void Awake()
        {
            base.Awake();
            if (_NotchDevices.Contains(SystemInfo.deviceModel))
                _ProgressBar.transform.localPosition += _NotchDevicesOffset;
            DontDestroyOnLoad(gameObject);
        }

        public void DestroySelf()
        {
            _MaxAutoProgress = 1f;
            //Speed-up fake progress.
            _AutoProgressTime /= 4;
            _AutoProgressStep *= 25;
        }

        protected override void Update()
        {
            base.Update();

            mElapsedTime += Time.deltaTime;
            if (mElapsedTime >= _AutoProgressTime && mProgress < _MaxAutoProgress)
            {
                mProgress += _AutoProgressStep;
                SetProgress(mProgress);
            }
        }

        /// <summary>
        /// inProgress must be between 0 & 1
        /// </summary>
        /// <param name="inProgress"></param>
        public void SetProgress(float inProgress)
        {
            if (mProgress <= inProgress)
            {
                mProgress = inProgress;
                mElapsedTime = 0f;
                if (_ProgressBar != null)
                {
                    _ProgressBar.SetProgress(inProgress);
                    _ProgressBar.SetText((Mathf.FloorToInt(inProgress * 100)) + "%");
                }

                if (mProgress >= 1f)
                {
                    if (_OnLoadingComplete != null)
                        _OnLoadingComplete();
                    _OnLoadingComplete = null;

                    if (XDebug.CanLog(XDebug.Mask.Resources))
                        Debug.Log("Destroying Loading Screen");

                    Destroy(transform.root.gameObject);
                }
            }
        }

        public void PauseAutoUpdate(bool isPaused)
        {
            enabled = !isPaused;
        }
    }
}