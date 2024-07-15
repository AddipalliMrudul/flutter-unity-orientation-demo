using System.Collections.Generic;
using XcelerateGames.IOC;

namespace XcelerateGames.UpdateManager
{
    public class SigAssetUpdated : Signal<List<string>> { }
    public class SigCheckForAssetUpdate : Signal { }
}
