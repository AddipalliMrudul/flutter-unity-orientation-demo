using XcelerateGames.PsdImporter;

namespace XcelerateGames.EditorTools.PsdToUI.Layer
{
	public enum LayerType
	{
		Group,
		Image,
		Text
	}

	public class XgImportLayerData : ImportLayerData
	{
		public LayerType layerType = LayerType.Group;
	}
}
