using System;
using UnityEngine;

namespace XcelerateGames.Audio
{
    /// <summary>
    /// Play Audio on set trigger points
    /// </summary>
    public class PlayAudio : MonoBehaviour
    {
        /// <summary>
        /// Trigger points
        /// </summary>
        [Flags] [SerializeField]
        public enum Trigger
        {
            OnAwake = (1 << 1),     /**<Play on Awake */
            OnStart = (1 << 2),     /**<Play on Start */
            OnEnable = (1 << 3),    /**<Play on object enabled */
            OnDisable = (1 << 4),   /**<Play on object disable */
            OnDestroy = (1 << 5),   /**<Play on objct destroy */
        }

        public AudioVars _Audio = null;

        /// <summary>
        /// Triggers points to play Audio on
        /// </summary>
        [EnumFlag] public Trigger _Trigger = 0x0;

        /// <summary>
        /// Play on Awake if Awake trigger is set
        /// </summary>
        private void Awake()
        {
            Play(Trigger.OnAwake);
        }

        /// <summary>
        /// Play on Start if Start trigger is set
        /// </summary>
        void Start()
        {
            Play(Trigger.OnStart);
        }

        /// <summary>
        /// Play on object being destroyed if OnDestroy trigger is set
        /// </summary>
        private void OnDestroy()
        {
            Play(Trigger.OnDestroy);
        }

        /// <summary>
        /// Play on object being disabled if OnDisable trigger is set
        /// </summary>
        private void OnDisable()
        {
            Play(Trigger.OnDisable);
        }

        /// <summary>
        /// Play on object being enabled if OnEnable trigger is set
        /// </summary>
        private void OnEnable()
        {
            Play(Trigger.OnEnable);
        }

        /// <summary>
        /// Play the audio clip
        /// </summary>
        private void DoPlay()
        {
            _Audio.Play();
        }

        /// <summary>
        /// Validate the trigger points & play if the condition is met
        /// </summary>
        /// <param name="triggerType"></param>
        public void Play(Trigger triggerType)
        {
            if ((_Trigger & triggerType) != 0)
                DoPlay();
        }
    }
}
