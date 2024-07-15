using static XcelerateGames.PlatformUtilities;

namespace XcelerateGames
{
    /// <summary>
    /// Data structure to hold environment specific data
    /// </summary>
    [System.Serializable]
    public class EnvironmentPathData
    {
        public Environment _Environment = Environment.None;
        public string _Path = null;
    }

    /// <summary>
    /// This class is to have paths based on environement. \n 
    /// For platform spcific (Android, iOS etc) data use PlatformVars
    /// @see EnvironmentPathData, PlatformVars 
    /// </summary>
    [System.Serializable]
    public class EnvironmentPath
    {
        /// <summary>
        /// List of all paths by environment
        /// </summary>
        public EnvironmentPathData[] _Paths = null;

        /// <summary>
        /// Get the path by current environment setting
        /// </summary>
        public string Path
        {
            get
            {
                EnvironmentPathData pathData = System.Array.Find(_Paths, e => e._Environment == PlatformUtilities.GetEnvironment());
                if(pathData != null)
                    return pathData._Path;
                else
                {
                    XDebug.LogError($"Could not find path for {PlatformUtilities.GetEnvironment()}");
                    return null;
                }
            }
        }

        public bool IsEmpty()
        {
            return _Paths.IsNullOrEmpty();
        }
    }
}
