using System.Collections;
using UnityEngine;
using XcelerateGames.IOC;
using XcelerateGames.UI;

namespace XcelerateGames.SpinWheel
{
    public class UiSpinWheel : UiBase
    {
        [SerializeField] private UiItem _SpinBtn = null;
        [SerializeField] private UiItem _CollectBtn = null;
        [SerializeField] private Transform _WheelTransform = null;
        [SerializeField] private AnimationCurve _AnimationCurve;

        //float timer;
        [SerializeField] private int _RotationTime = 8;
        private float mMaxAngle;
        [SerializeField] private int _NumOfSpokes = 9;

        //Uncomment below signals and their invokations for this to work in
        //initialized framework
        //[InjectSignal] private SigSpinStarted mSigSpinStarted = null;
        //[InjectSignal] private SigSpinComplete mSigSpinComplete = null;

        protected override void Start()
        {
            base.Start();
            _SpinBtn.SetActive(true);
            _CollectBtn.SetActive(false);
        }
        #region UI Callbacks

        /// <summary>
        /// Starts the spin
        /// </summary>
        /// <param name="rewardIndex">the index of spoke at which to stop</param>
        public virtual void OnClickSpin(int rewardIndex)
        {
            //mSigSpinStarted.Dispatch();
            _SpinBtn.SetActive(false);
            _BackBtn.SetActive(false);
            SpinWheelPlain spinWheel = new SpinWheelPlain();
            mMaxAngle = spinWheel.GetWheelStopAngle(rewardIndex, _NumOfSpokes, _RotationTime);
            StartCoroutine(RotateWheelCoroutine());
        }

        public virtual void OnClickCollect()
        {

        }
        #endregion UI Callbacks

        /// <summary>
        /// Rotates the wheel tranform using animation curve(selecting one random out of 5)
        /// on local Euler angles
        /// </summary>
        /// <returns></returns>
        private IEnumerator RotateWheelCoroutine()
        {
            float timer = 0.0f;
            _WheelTransform.localEulerAngles = Vector3.zero;

            float startAngle = _WheelTransform.localEulerAngles.z;

            while (timer < _RotationTime)
            {
                float angle = mMaxAngle * _AnimationCurve.Evaluate(timer / _RotationTime);
                _WheelTransform.localEulerAngles = Vector3.back * angle;
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            _CollectBtn.SetActive(true);
            _WheelTransform.localEulerAngles = Vector3.back * (mMaxAngle + startAngle);
            //mSigSpinComplete.Dispatch();
        }

#if UNITY_EDITOR
        [Header("Editor Only")]
        [SerializeField] private int _IndexToStop = 1;
        [ContextMenu("TestSpin")]
        void TestSpin()
        {
            OnClickSpin(_IndexToStop);
        }
#endif
    }
}