using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.UI;

namespace XcelerateGames.SlotMachine
{
    struct ReelUnits
    {
        internal int UnitsToMove;
        internal int YStopPos;
    }

    /// <summary>
    /// Sets up each slot's reels and UiReelBehaviour further sets up symbols for each reel
    /// </summary>
    public class UiSlotBehaviour : UiItem
    {
        [SerializeField] private AnimationCurve _AnimationCurve;
        [SerializeField] private UiReelBehaviour _UiReelBehaviour;
        [SerializeField] private bool _HasSpringEffect = true;
        [SerializeField] private float _SpringEffectDistance = 5f;
        [SerializeField] private float _SpringEffectDuration = 5f;
        [SerializeField] private float _SpringEffectSpeed = 5f;

        private Dictionary<int, ReelUnits> mReelIndexUnitsPair = new Dictionary<int, ReelUnits>();
        private int mMaxYPosReelCanGoTo = 0;

        public void Init(float rotationTime, int valueToStopAt, Sprite[] spritesPerReel,
            int spaceBetweenTwoSymbolsInSameReel)
        {
            Vector2 slotSize = transform.GetComponent<RectTransform>().sizeDelta;
            mMaxYPosReelCanGoTo = Mathf.CeilToInt(spritesPerReel.Length * (slotSize.y + spaceBetweenTwoSymbolsInSameReel));
            PopulateReelStopPositions(slotSize.y, spritesPerReel.Length, spaceBetweenTwoSymbolsInSameReel);
            _UiReelBehaviour.Init(spritesPerReel, spaceBetweenTwoSymbolsInSameReel,
                () => StartCoroutine(RotateReelCoroutine(rotationTime, valueToStopAt)));
        }

        /// <summary>
        /// Reel stop position and units to move to reach at desired point is
        /// calculated dynamically as per your slot(symbol) size and stored in dictionary
        /// </summary>
        /// <param name="heightOfEachSymbol">float</param>
        /// <param name="numOfSymbolsPerReel">int</param>
        /// <param name="spaceBetweenTwoSymbolsInSameReel">int</param>
        private void PopulateReelStopPositions(float heightOfEachSymbol, int numOfSymbolsPerReel,
            int spaceBetweenTwoSymbolsInSameReel)
        {
            int middleSymbolIndex = Mathf.CeilToInt(numOfSymbolsPerReel / 2);
            int lengthOfReel = (int)(heightOfEachSymbol + spaceBetweenTwoSymbolsInSameReel) * numOfSymbolsPerReel;
            for (int i = 0; i < numOfSymbolsPerReel; i++)
            {
                //for initial half the units to move and stop pos is less than the total length of reel
                //e.g. is length of reel is 1000 units, and each symbol takes 80 units and spacing is 20 units
                //then for 0th index UnitsToMove is 1000, YStopPos = 0
                //for 1th index UnitsToMove is 900, YStopPos = 100
                //for 5th index UnitsToMove is 500, YStopPos = 500
                if (i <= middleSymbolIndex)
                {
                    ReelUnits reelUnits;
                    reelUnits.UnitsToMove = (int)(lengthOfReel -
                        (i * (heightOfEachSymbol + spaceBetweenTwoSymbolsInSameReel)));
                    reelUnits.YStopPos = lengthOfReel - reelUnits.UnitsToMove;
                    mReelIndexUnitsPair.Add(i, reelUnits);
                    //Debug.LogError($"for index {i} UnitsToMove {reelUnits.UnitsToMove} and YStopPos {reelUnits.YStopPos}");
                }
                //for remaining half the units to move and stop pos is greater than the total length of reel
                //similarly for 6th index UnitsToMove is 1400, YStopPos = 600
                //similarly for 7th index UnitsToMove is 1300, YStopPos = 700
                //similarly for 9th index UnitsToMove is 1100, YStopPos = 900
                else
                {
                    ReelUnits reelUnits;
                    reelUnits.UnitsToMove = (int)(lengthOfReel +
                        ((numOfSymbolsPerReel - i) * (heightOfEachSymbol + spaceBetweenTwoSymbolsInSameReel)));
                    reelUnits.YStopPos = lengthOfReel - (reelUnits.UnitsToMove - lengthOfReel);
                    mReelIndexUnitsPair.Add(i, reelUnits);
                    //Debug.LogError($"for index {i} UnitsToMove {reelUnits.UnitsToMove} and YStopPos {reelUnits.YStopPos}");
                }
            }
        }

        private IEnumerator RotateReelCoroutine(float rotationTime, int valueToStopAt)
        {
            float timer = 0.0f;
            float delta = 0f;
            ReelUnits reelUnits = mReelIndexUnitsPair[valueToStopAt];
            float totalUnitsToMove = reelUnits.UnitsToMove;
            float deltaSum = 0;
            while (timer < rotationTime && deltaSum < totalUnitsToMove)
            {
                if (_UiReelBehaviour.transform.localPosition.y <= 0)
                    _UiReelBehaviour.transform.localPosition = new Vector2(_UiReelBehaviour.transform.localPosition.x, mMaxYPosReelCanGoTo);

                delta = (((totalUnitsToMove * Time.deltaTime) * _AnimationCurve.Evaluate(timer / rotationTime)) * 2f / rotationTime);
                deltaSum += delta;
                timer += Time.deltaTime;
                _UiReelBehaviour.transform.localPosition = new Vector2(_UiReelBehaviour.transform.localPosition.x, _UiReelBehaviour.transform.localPosition.y -
                   (delta));
                yield return new WaitForEndOfFrame();
            }
            if (_HasSpringEffect)
            {
                timer = 0f;
                Vector3 startPos = _UiReelBehaviour.transform.localPosition;
                Vector3 endPos = startPos;
                endPos.y -= _SpringEffectDistance;
                while (timer < _SpringEffectDuration)
                {
                    _UiReelBehaviour.transform.localPosition = Vector3.Lerp(startPos, endPos, timer / _SpringEffectDuration);
                    timer += Time.deltaTime * _SpringEffectSpeed;
                    yield return new WaitForEndOfFrame();
                }

                timer = 0f;
                startPos = endPos;
                endPos.y += _SpringEffectDistance;
                while (timer < _SpringEffectDuration)
                {
                    _UiReelBehaviour.transform.localPosition = Vector3.Lerp(startPos, endPos, timer / _SpringEffectDuration);
                    timer += Time.deltaTime * _SpringEffectSpeed;
                    yield return new WaitForEndOfFrame();
                }
                SnapToExactPosition(reelUnits.YStopPos);
            }
            else
            {
                SnapToExactPosition(reelUnits.YStopPos);
            }
        }

        /// <summary>
        /// When rotation is complete it snaps the reel at exact position
        /// </summary>
        /// <param name="yStopPos"></param>
        private void SnapToExactPosition(int yStopPos)
        {
            _UiReelBehaviour.transform.localPosition = new Vector2(_UiReelBehaviour.transform.localPosition.x, yStopPos);
        }
    }
}
