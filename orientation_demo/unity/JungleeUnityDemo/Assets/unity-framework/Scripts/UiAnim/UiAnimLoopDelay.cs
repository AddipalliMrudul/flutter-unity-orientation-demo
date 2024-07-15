using System;
using UnityEngine;

namespace XcelerateGames.UI.Animations
{
    /// <summary>
    /// - Add this component to the same gameobject having UiAnim Component
    /// - It playes the default anim assigned to UiAnim
    /// - UiAnim shouldn't be looped and mode shouldn't be infite type
    /// - if play on awake is enabled on UIAnim then mark _isInitialDelayRequired as false and when _isInitialDelayRequired is false the initial delay won't work
    /// </summary>
    [RequireComponent(typeof(UiAnim))]
    public class UiAnimLoopDelay : MonoBehaviour
    {
        [SerializeField] private UiAnim _UiAnim = null;
        [SerializeField] private int _LoopDelay = 0;
        [SerializeField] private bool _IsInitialDelayRequired = false;
        [SerializeField] private int _InitialDelay = 0;
        [SerializeField] private string _AnimName = null;

        private void Start()
        {
            if (_UiAnim == null)
                _UiAnim = GetComponent<UiAnim>();

            if (_AnimName.IsNullOrEmpty())
                _AnimName = _UiAnim._DefaultAnim;

            if (_IsInitialDelayRequired)
                Invoke(nameof(PlayAnim), _InitialDelay);

            if (!_AnimName.IsNullOrEmpty())
                _UiAnim._OnAnimationDone.AddListener(OnAnimationDone);
        }

        /// <summary>
        /// Event triggered on animation done
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="animName"></param>
        private void OnAnimationDone(UiAnim anim, string animName)
        {
            if (animName == _AnimName)
                Invoke(nameof(PlayAnim), _LoopDelay);
        }

        /// <summary>
        /// Plays the given animation assigned as string
        /// </summary>
        private void PlayAnim()
        {
            _UiAnim.Play(_AnimName);
        }

        private void OnDestroy()
        {
            CancelInvoke();
            _UiAnim._OnAnimationDone.RemoveListener(OnAnimationDone);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _UiAnim = GetComponent<UiAnim>();
        }
#endif //UNITY_EDIOR
    }
}
