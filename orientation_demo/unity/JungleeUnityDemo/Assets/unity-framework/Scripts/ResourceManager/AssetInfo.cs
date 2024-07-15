//This same alias is used in ResourceManager as well.
using ResEvent = System.Action<XcelerateGames.AssetLoading.ResourceEvent, string, object, object>;

namespace XcelerateGames.AssetLoading
{
    /// <summary>
    /// The asset loaded from Asset bundle.
    /// </summary>
    public class AssetInfo
    {
        public string _BundleName = null;   /**< */
        public string _AssetName = null;    /**< */
        public ResourceManager.ResourceType _Type = ResourceManager.ResourceType.NONE;  /**< */
        public object _UserData = null;     /**< */
        public ResEvent _Event = null;      /**< */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleName">Name of the asset bundle</param>
        /// <param name="assetName">Name of the asset within the bundle</param>
        /// <param name="inUserData">Any user data to be passed in callback</param>
        /// <param name="inEvent">Callback function</param>
        /// <param name="inType">Type of the asset</param>
        public AssetInfo(string bundleName, string assetName, object inUserData, ResEvent inEvent, ResourceManager.ResourceType inType)
        {
            _BundleName = bundleName;
            _AssetName = assetName;
            _Type = inType;
            _UserData = inUserData;
            _Event = inEvent;
        }

        /// <summary>
        /// Combined Path of Bundle & asset within the bundle.
        /// </summary>
        /// <returns>Combined path</returns>
        public string Path()
        {
            if (_BundleName.IsNullOrEmpty())
                return _AssetName;
            if (_AssetName.IsNullOrEmpty())
                return _BundleName;
            return $"{_BundleName}/{_AssetName}";
        }
    }
}
