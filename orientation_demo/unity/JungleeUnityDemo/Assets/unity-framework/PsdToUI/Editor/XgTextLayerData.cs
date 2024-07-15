using XcelerateGames.EditorTools.PsdToUI.Layer;

namespace XcelerateGames.EditorTools.PsdToUI.Text
{
    class XgTextLayerData : XgImportLayerData
    {
        public XgPsdTextInfo textInfo;

        public XgTextLayerData(XgPsdTextInfo txtInfo)
        {
            layerType = LayerType.Text;
            textInfo = txtInfo;
        }
    }
}
