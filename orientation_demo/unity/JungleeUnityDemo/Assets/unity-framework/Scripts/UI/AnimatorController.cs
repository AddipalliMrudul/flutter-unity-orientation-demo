using UnityEngine;

namespace XcelerateGames
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorController : MonoBehaviour
    {
        public FloatRange _TimeRange;
        public bool _StartEnabled = false;

        private float mTimer = 0f;

        void Awake()
        {
            GetComponent<Animator>().enabled = _StartEnabled;
            enabled = !_StartEnabled;
            if(!_StartEnabled)
            {
                OnAnimEndEvent();
            }
        }

        void Start()
        {
        }

        void Update()
        {
            mTimer -= Time.deltaTime;
            if(mTimer < 0)
            {
                GetComponent<Animator>().enabled = true;
                enabled = false;
            }
        }

        public void OnAnimEndEvent()
        {
            GetComponent<Animator>().enabled = false;
            mTimer = _TimeRange.GetRandomValue();
            enabled = true;
        }

        //Intensionally added this function, as we have OnAnimEndEvent in TriggerAnimEvent, If both the scripts are attached to same GameObject, then Unity will crash because of recursive loop.
        //Cant rename the above function as its been added in several other places, it will break.
        public void OnAnimEndEventSndMsg()
        {
            OnAnimEndEvent();
        }
    }
}
