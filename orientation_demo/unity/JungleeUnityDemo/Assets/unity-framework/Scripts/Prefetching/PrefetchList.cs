using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XcelerateGames
{
    [Serializable]
    [XmlRoot(ElementName = "PrefetchList", Namespace = "")]
    public partial class PrefetchList
    {
        [XmlArray(ElementName = "BundleNames")]
        public List<string> Bundles = null;
    }
}
