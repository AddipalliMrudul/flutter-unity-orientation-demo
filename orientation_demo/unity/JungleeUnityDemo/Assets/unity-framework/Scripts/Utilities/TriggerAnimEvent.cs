using System;
using UnityEngine;

/*
 * Author : Altaf
 * Date : June 20, 2018
 * Purpose : To send Anim event from Animation clip.
 * Why? ; Animation events are send to the same GameObject that has the Animator attached. We cannot send AnimEvent to another GameObject
 * How to Use : Attach this script to the GameObject that has animation attached, Drag n Drop the GameObject that needs to listen to the event.
 */

namespace XcelerateGames
{
    [RequireComponent(typeof(Animator))]
    public class TriggerAnimEvent : MonoBehaviour
    {
        public enum Mode
        {
            UseEvent,
            TimeBased,
        }
        [Serializable]
        public class AnimEvent
        {
            public GameObject _Listener;
            public string _MethodName;
            public string _AnimId;

            public void Send(string animId)
            {
                _Listener.SendMessage(_MethodName, animId, SendMessageOptions.RequireReceiver);
            }

            public void Send()
            {
                Send(_AnimId);
            }
        }

        public AnimEvent[] _EventListeners;
        public bool _DisableAnimator = true;
        public bool _DisableGameObject = false;
        public bool _EnableAnimatorOnDisable = false;
        public Mode _Mode = Mode.UseEvent;
        public float _WaitTime = 1f;

        private float mElapsedTime = 0f;

        public void OnAnimEvent(string animId)
        {
            Common();

            for (int i = 0; i < _EventListeners.Length; ++i)
            {
                if (_EventListeners[i] != null)
                {
                    _EventListeners[i].Send(animId);
                }
            }
        }

        public void OnAnimEndEvent()
        {
            Common();

            for (int i = 0; i < _EventListeners.Length; ++i)
            {
                if (_EventListeners[i] != null)
                {
                    _EventListeners[i].Send();
                }
            }
        }

        private void OnDisable()
        {
            if (_EnableAnimatorOnDisable)
                GetComponent<Animator>().enabled = true;
        }

        private void Common()
        {
            if (_DisableAnimator)
                GetComponent<Animator>().enabled = false;
            if (_DisableGameObject)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if(_Mode == Mode.TimeBased)
            {
                mElapsedTime += Time.deltaTime;
                if(mElapsedTime >= _WaitTime)
                {
                    mElapsedTime = 0f;
                    OnAnimEndEvent();
                    enabled = false;
                }
            }
        }
    }
}
