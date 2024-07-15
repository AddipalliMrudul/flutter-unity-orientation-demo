using System.Collections.Generic;
using XcelerateGames.PsdImporter;
using XcelerateGames.PsdImporter.Reconstructor;
using UnityEngine;
using XcelerateGames.Editor.UI;
using XcelerateGames.EditorTools.PsdToUI.Layer;

public partial class ReconstructorEnum : Enumeration<ReconstructorEnum>
{
	public static readonly ReconstructorEnum JungleeUI = new ReconstructorEnum("Junglee UI");
}

namespace XcelerateGames.EditorTools.PsdToUI
{
	public class XgUIReconstructor : IReconstructor
    {
		private readonly ReconstructorEnum reconstructorEnum;
		private readonly XgUIObjectsWorkshop uiObjWorkshop;

		public XgUIReconstructor()
        {
			reconstructorEnum = ReconstructorEnum.FromName("Junglee UI");
			uiObjWorkshop = new XgUIObjectsWorkshop();
		}

        public string DisplayName { get { return reconstructorEnum.Name; } }

        public string HelpMessage { get { return "Select PSD"; } }

        public ReconstructorEnum ReconstructorType { get { return reconstructorEnum; } }

        public bool CanReconstruct(GameObject selection)
        {
            return true;
        }

        private Vector2 GetLayerPosition(Rect layerRect, Vector2 layerAnchor)
        {
            Vector2 layerPos = new Vector2(Mathf.Lerp(layerRect.xMin, layerRect.xMax, layerAnchor.x),
                                            Mathf.Lerp(layerRect.yMin, layerRect.yMax, layerAnchor.y));
            return layerPos;
        }

        public GameObject Reconstruct(ImportLayerData root, ReconstructData data, GameObject selection)
		{
			if (selection == null)
				return null;
			if (CanReconstruct(selection) == false)
				return null;

			XgImportLayerData jgLayerData = root as XgImportLayerData;
			var rootT = uiObjWorkshop.CreateObject(jgLayerData, data);// CreateObject(root.name);
			rootT.SetParent(selection.transform);

			rootT.sizeDelta = data.documentSize;
			rootT.pivot = data.documentPivot;
			rootT.localPosition = Vector3.zero;

			// Create a stack that represents the current parent
			// as the hierarchy is being traversed
			Stack<RectTransform> hierarchy = new Stack<RectTransform>();
			// Add the root object as the first parent
			hierarchy.Push(rootT);

			// Calculate the document pivot position
			Vector2 docRoot = data.documentSize;
			docRoot.x *= data.documentPivot.x;
			docRoot.y *= data.documentPivot.y;

			root.Iterate(
				layer =>
				{
					// Only process non group layers or layers marked for import
					if (layer.Childs.Count > 0 || layer.import == false)
						return;

					XgImportLayerData layerData = layer as XgImportLayerData;
					// Create an object
					RectTransform layerT = uiObjWorkshop.CreateObject(layerData, data);

					// And attach it to the last parent
					layerT.SetParent(hierarchy.Peek());
					// Order correctly for UI
					layerT.SetAsFirstSibling();

					// Get the layer position

					Rect layerRect;
					if (data.layerBoundsIndex.TryGetValue(layer.indexId, out layerRect) == false)
						layerRect = Rect.zero;

					Vector2 layerAnchor;
					if (data.spriteAnchors.TryGetValue(layer.indexId, out layerAnchor) == false)
						layerAnchor = Vector2.zero;

					Vector2 layerPos = GetLayerPosition(layerRect, layerAnchor);
					// Express it as a vector
					Vector2 layerVector = layerPos - docRoot;
					// Position using the rootT as reference
					layerT.position = rootT.TransformPoint(layerVector.x, layerVector.y, 0);

					layerT.pivot = layerAnchor;
					layerT.sizeDelta = new Vector2(layerRect.width, layerRect.height);
				},
				checkGroup => checkGroup.import, // Only enter groups if part of the import
				enterGroupCallback: layer =>
				{
					// Enter a group, create an object for it
					RectTransform groupT = uiObjWorkshop.CreateObject(layer as XgImportLayerData, data); //CreateObject(layer.name);

					// Parent to the last hierarchy parent
					groupT.SetParent(hierarchy.Peek());
					groupT.SetAsFirstSibling();

                    groupT.anchorMin = Vector2.zero;
                    groupT.anchorMax = Vector2.one;
                    groupT.offsetMin = Vector2.zero;
                    groupT.offsetMax = Vector2.zero;

                    // Look at me, I'm the hierarchy parent now
                    hierarchy.Push(groupT);
				},
				exitGroupCallback: layer =>
				{
					// Go back to the last parent
					RectTransform groupT = hierarchy.Pop();
					OnExitingGroupLayer(groupT);
				});

			OnExitingGroupLayer(rootT);
            JgUIAnchorDirector anchorDirector = new JgUIAnchorDirector();
            anchorDirector.SetClosestAnchor(rootT);
            return rootT.gameObject;
		}


		private void OnExitingGroupLayer(RectTransform rectT)
		{
			uiObjWorkshop.SanitizeWidget(rectT);
		}
	}

}