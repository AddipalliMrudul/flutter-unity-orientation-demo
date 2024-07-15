using System;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Set/Change the orientation of the device as per the settings in _AllowedOrientations.
    /// Once orientation is set, GameObject is set to inactive.
    /// @note Works only on mobile platform. On other platforms the GameObject is destroyed in Awake function
    /// </summary>
    public class SetOrientation : MonoBehaviour
    {
        /// <summary>
        /// List of allowed orientations
        /// </summary>
        [SerializeField] private ScreenOrientation[] _AllowedOrientations;

        /// <summary>
        /// Check if we are on mobile platform, if no, then delete the GameObject else chnage the orientation as set in 
        /// </summary>
        void Awake()
        {
            if (PlatformUtilities.IsMobile())
            {
                if (ShouldChangeOrientation())
                {
                    Debug.Log($"Changing orientation from: {Screen.orientation} to: {_AllowedOrientations[0]}");
                    Screen.orientation = _AllowedOrientations[0];
                    gameObject.SetActive(false);
                }
                else
                    Debug.Log($"Already in the expected orientation of {Screen.orientation}");
            }
            else
                GameObject.Destroy(gameObject);
        }

        /// <summary>
        /// Check if the curret orientation needs to be changed
        /// </summary>
        /// <returns>true if orientation is different else false</returns>
        bool ShouldChangeOrientation()
        {
            //If no preferred orientation is set, just return false
            if (_AllowedOrientations.Length == 0)
                return false;
            return Array.FindIndex(_AllowedOrientations, e => e == Screen.orientation) == -1;
        }
    }
}
