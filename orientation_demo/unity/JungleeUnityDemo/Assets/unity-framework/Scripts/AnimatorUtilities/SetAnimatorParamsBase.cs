using UnityEngine;
using XcelerateGames.Audio;

namespace XcelerateGames.AnimatorUtils
{
    public enum EventType
    {
        OnAwake,
        OnEnable,
        OnStart,
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class SetAnimatorParamsBase : MonoBehaviour
    {
        [SerializeField] protected string _Name;
        [SerializeField] protected float _Delay;
        [SerializeField] protected EventType _EventType = EventType.OnAwake;
        [SerializeField] protected AudioVars _Audio = null;

        [Tooltip("-1 is infinite")] [SerializeField] protected int _MaxCount = -1;

        protected int mCurentCount = 0;
        protected Animator mAnimator;

        protected virtual void Awake()
        {
            mAnimator = GetComponent<Animator>();

            if (_EventType == EventType.OnAwake)
                Invoke("Process", _Delay);
        }

        protected virtual void Start()
        {
            if (_EventType == EventType.OnStart)
                Invoke("Process", _Delay);
        }

        protected virtual void OnEnable()
        {
            if (_EventType == EventType.OnEnable)
                Invoke("Process", _Delay);
        }

        protected virtual void Process()
        {
            if (_MaxCount == -1 || mCurentCount < _MaxCount)
            {
                ++mCurentCount;
                SetValue();
            }
        }

        protected virtual void SetValue()
        {
            _Audio.Play();
        }
    }
}
