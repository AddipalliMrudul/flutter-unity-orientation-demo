using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XcelerateGames
{
    [Serializable]
    [XmlRoot(ElementName = "ShippedAssets", Namespace = "")]
    public partial class ShippedAssets
    {
        [XmlArray(ElementName = "Assets")]
        public List<Asset> Assets = null;
    }

    public partial class Asset
    {
        [XmlElement(ElementName = "Name")]
        public string Name = null;

        [XmlElement(ElementName = "Hash")]
        public string Hash = null;
    }
}
