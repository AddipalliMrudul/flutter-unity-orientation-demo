using UnityEngine;
using static XcelerateGames.PlatformUtilities;

namespace XcelerateGames
{
    /// <summary>
    /// Data structure to hold platform specific data
    /// @see PlatformVars
    /// </summary>
    [System.Serializable]
    public class PlatformVarData
    {
        public Platform _Platform = Platform.NONE;
        public string _Value = null;
    }

    /// <summary>
    /// Platform specific values can be added here. For ex, One value is different for Android & iOS. We can use this class in such places.
    /// For environment spcific (dev, qa, prod etc) data use EnvironmentPath
    /// @see PlatformVarData, EnvironmentPath
    /// </summary>
    [System.Serializable]
    public class PlatformVars
    {
        /// <summary>
        /// Data for all platforms
        /// </summary>
        [SerializeField] private PlatformVarData[] _Values = null;

        /// <summary>
        /// Returns the value based on current platform
        /// </summary>
        public string Value
        {
            get
            {
                PlatformVarData data = System.Array.Find(_Values, e => e._Platform == GetCurrentPlatform());
                if(data != null)
                    return data._Value;
                else
                {
                    XDebug.LogError($"Could not find value for {GetCurrentPlatform()}");
                    return null;
                }
            }
        }
    }
}
