using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Attach this script to an object to have it float on a consine wave over time
    /// _Speed: speed at which the object floats on each axis
    /// _Distance: distance from origin object moves on each axis
    /// </summary>
    public class ObFloat : MonoBehaviour
    {
        [SerializeField] Vector3 _Speed;
        [SerializeField] Vector3 _Distance;

        [SerializeField] bool _Relative = false;
        [Tooltip("If this is enabled all objects will float at same time")]
        [SerializeField] bool _Sync = false;
        [SerializeField] FloatRange _RandomDelay;

        private Vector3 mStartPosition;

        private Transform mTransform;
        private float mElapsedTime = 0f;

        private void Awake()
        {
            mTransform = transform;
            StartFloating();
        }

        private void Enable()
        {
            enabled = true;
        }

        public void Start()
        {
            if (_Relative)
                mStartPosition = mTransform.localPosition;
            else
                mStartPosition = mTransform.position;
        }

        void Update()
        {
            Vector3 newposition = mStartPosition;
            float curtime = Time.timeSinceLevelLoad;
            if(!_Sync)
            {
                mElapsedTime += Time.deltaTime;
                curtime = mElapsedTime;
            }
            float angle = curtime * Mathf.PI * 2;

            if (_Speed.x != 0 && _Distance.x != 0)
                newposition.x = mStartPosition.x + (-Mathf.Cos(angle * _Speed.x) * _Distance.x);

            if (_Speed.y != 0 && _Distance.y != 0)
                newposition.y = mStartPosition.y + (-Mathf.Cos(angle * _Speed.y) * _Distance.y);

            if (_Speed.z != 0 && _Distance.z != 0)
                newposition.z = mStartPosition.z + (-Mathf.Cos(angle * _Speed.z) * _Distance.z);

            if (_Relative)
                mTransform.localPosition = newposition;
            else
                mTransform.position = newposition;
        }

        public void ResetState()
        {
            if (_Relative)
                mTransform.localPosition = mStartPosition;
            else
                mTransform.position = mStartPosition;
            enabled = false;
        }

        public void StartFloating()
        {
            enabled = false;
            Invoke(nameof(Enable), _RandomDelay.GetRandomValue());
        }


#if UNITY_IOS || UNITY_ANDROID
        //chache here because re-setting position not affecting it.
        void OnEnable()
        {
            if (_Relative)
                mStartPosition = mTransform.localPosition;
            else
                mStartPosition = mTransform.position;

        }
#endif
    }
}
