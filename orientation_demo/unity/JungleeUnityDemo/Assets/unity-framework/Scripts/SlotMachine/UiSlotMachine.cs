using System;
using UnityEngine;
using UnityEngine.Events;
using XcelerateGames.UI;

namespace XcelerateGames.SlotMachine
{
    /// <summary>
    /// Slot machine module, sets number of reels as per _IndicesToStopReelAt provided, sprites in _SpritesPerReel array
    /// Set the width and height from inspector on prefab on Slot, Slot>Reel(width only as height is adjusted with content size fitter),
    /// Slot>Reel>Symbol as per your project requirement, make sure to keep all those elements dimensions same
    /// </summary>
    public class UiSlotMachine : UiMenu
    {
        private const int MIN_RANGE_FOR_SPIN_TIME = 0;
        private const int MAX_RANGE_FOR_SPIN_TIME = 10;
        [Range(MIN_RANGE_FOR_SPIN_TIME, MAX_RANGE_FOR_SPIN_TIME)]
        [SerializeField] protected float _SpinTime;
        /*
        *  Time offset value offset's the subsequent slot's rotation time by _SpinTime plus this value
        *  e.g. if its value is 1 and spin time is 3 then
        *  2nd slot's rotation time will be 3+1
        *  3rd slot's rotation time will be previous slot's rotation time plus time offset i.e. 4+1
        *  similarly when it's value is -1 then
        *  2nd slot's rotation time will be 3-1 = 2
        *  3rd slot's roation time will be 3-2 = 2, Why 2 here? read below note
        *  NOTE: the rotation time's offset will be clamped between MIN_RANGE_FOR_SPIN_TIME and 
        *  MAX_RANGE_FOR_SPIN_TIME 
        *  IMPORTANT: FOR OFFSET TO WORK FOR -1 THE SPIN TIME HAS TO BE GREATER THAN THE NUM
        *  OF SLOTS
        */
        [SerializeField] private float _TimeOffset;
        [SerializeField] int[] _IndicesToStopReelAt = { };
        [SerializeField] private Sprite[] _SpritesPerReel = { };
        [SerializeField] private int _SpaceBetweenTwoSymbolsInSameReel;

        /// <summary>
        /// The init is done in start for demo purpose.
        /// For your specific implementation comment Start and Initiate it from whereever you requiement demands
        /// </summary>
        //protected override void Start()
        //{
        //    KickOff(new int[] { 1, 0, 9, 9, 9 });
        //}

        protected void KickOff(int[] indicesToStopReelAt)
        {
            float timeDelay = 0;
            Invoke("OnSpinComplete", _SpinTime);
            _IndicesToStopReelAt = indicesToStopReelAt;
            foreach (var index in _IndicesToStopReelAt)
            {
                UiSlotBehaviour uiSlot = AddWidget<UiSlotBehaviour>(null);
                uiSlot.Init(ClampRotationTimeForReelUnderConsideration(_SpinTime + timeDelay),
                    index, _SpritesPerReel, _SpaceBetweenTwoSymbolsInSameReel);
                timeDelay += _TimeOffset;
            }
        }

        protected virtual void OnSpinComplete()
        {

        }
        private float ClampRotationTimeForReelUnderConsideration(float rotationTime)
        {
            if (rotationTime < MIN_RANGE_FOR_SPIN_TIME)
                rotationTime = MIN_RANGE_FOR_SPIN_TIME;
            else if (rotationTime > MAX_RANGE_FOR_SPIN_TIME)
                rotationTime = MAX_RANGE_FOR_SPIN_TIME;
            //Debug.Log($"clamped rotation time: { rotationTime }");
            return rotationTime;
        }
    }
}
