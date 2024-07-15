using UnityEngine;
using UnityEngine.Events;

namespace XcelerateGames.UI.Animations
{
    public class UiAnimSequencer : MonoBehaviour
    {
        public enum EventType
        {
            Awake,
            Start,
            OnEnable,
            Custom      //Use this to play it via script
        }

        public float _StartDelay = 0f;
        public float _SequenceDelay = 0.5f;
        public string _AnimName = null;
        public EventType _EventType = EventType.Awake;

        public UiAnim[] _Anims;

        public UnityEvent _OnAnimDone;

        private void Awake()
        {
            Play(EventType.Awake);
        }

        void Start()
        {
            Play(EventType.Start);
        }

        private void OnEnable()
        {
            Play(EventType.OnEnable);
        }

        public void Play(EventType inEvent)
        {
            if (_EventType == inEvent)
                StartPlaying();
        }

        [ContextMenu("Play")]
        public void StartPlaying()
        {
            float delay = _SequenceDelay;
            System.Array.ForEach(_Anims, (UiAnim obj) =>
            {
                obj.Play(_AnimName, delay);
                delay += _SequenceDelay;
            });

            //TODO:Take anim duration in to account.
            Invoke("TriggerEvent", delay);
        }

        private void TriggerEvent()
        {
            _OnAnimDone?.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("Cache")]
        private void Cache()
        {
            _Anims = transform.GetComponentsInChildren<UiAnim>();
        }
#endif
    }
}