using System.Collections.Generic;
using UnityEngine;
using XcelerateGames.UI;

namespace XcelerateGames.EditorTools.PsdToUI
{
	public class XgUIRect
    {
		private struct JGUIAnchorPosition
		{
			public AnchorPresets anchor;
			public Vector2 position;
		}

		public XgUIRect(Vector2 worldPosition, Vector2 size)
        {
			AddAnchors(worldPosition, size);
		}

		List<JGUIAnchorPosition> anchors = new List<JGUIAnchorPosition>();

		/// <summary>
        /// Adding all the anchors and its positions
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="size"></param>
		private void AddAnchors(Vector2 worldPosition, Vector2 size)
        {
			float halfWidth = size.x / 2;
			float halfHeight = size.y / 2;
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.TopLeft, position = new Vector2(worldPosition.x - halfWidth, worldPosition.y + halfHeight) });
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.TopCenter, position = new Vector2(worldPosition.x, worldPosition.y + halfHeight) });
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.TopRight, position = new Vector2(worldPosition.x + halfWidth, worldPosition.y + halfHeight) });
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.MiddleLeft, position = new Vector2(worldPosition.x - halfWidth, worldPosition.y) });
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.MiddleCenter, position = new Vector2(worldPosition.x, worldPosition.y) });
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.MiddleRight, position = new Vector2(worldPosition.x + halfWidth, worldPosition.y) });
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.BottomLeft, position = new Vector2(worldPosition.x - halfWidth, worldPosition.y - halfHeight) });
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.BottomCenter, position = new Vector2(worldPosition.x, worldPosition.y - halfHeight) });
			anchors.Add(new JGUIAnchorPosition() { anchor = AnchorPresets.BottomRight, position = new Vector2(worldPosition.x + halfWidth, worldPosition.y - halfHeight) });
		}

		/// <summary>
        /// Finding the closest anchor in the parent with distance check
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
		public AnchorPresets GetClosestAnchor(Vector2 worldPosition)
        {
			AnchorPresets closestAnchor = AnchorPresets.MiddleCenter;
			float minAnchorDistance = float.MaxValue;
			foreach (JGUIAnchorPosition anchorPos in anchors)
			{
				float distance = Vector2.Distance(anchorPos.position, worldPosition);
				if (distance < minAnchorDistance)
				{
					closestAnchor = anchorPos.anchor;
					minAnchorDistance = distance;
				}
			}

			return closestAnchor;
        }
	}

    public class JgUIAnchorDirector
    {
		public void SetClosestAnchor(RectTransform rootT)
		{
			if (!rootT.name.StartsWith("Panel"))
			{
				AnchorPresets anchor = FindClosestAnchor(rootT);
				Vector3 position = rootT.position;
                if (anchor > AnchorPresets.BottomStretch)
                    SetStretchAnchorValues(rootT);
                rootT.SetAnchor(anchor);
				rootT.position = position;
				
			}

			if (rootT.childCount > 0)
			{
				for (int idx = 0; idx < rootT.childCount; idx++)
				{
					RectTransform childT = rootT.GetChild(idx) as RectTransform;
					SetClosestAnchor(childT);
				}
			}

		}

		private void SetStretchAnchorValues(RectTransform rectT)
        {
			RectTransform parentRect = rectT.parent as RectTransform;
			Vector2 parentSize = parentRect.rect.size;
			Vector2 rectSize = rectT.rect.size;
			Vector2 diff = parentSize - rectSize;
			rectT.offsetMin = new Vector2(diff.x/2, rectT.offsetMin.y);
			rectT.offsetMax = new Vector2(-diff.x/2, rectT.offsetMax.y);
		}

		private AnchorPresets FindClosestAnchor(RectTransform rectT)
		{
			RectTransform pRect = rectT.parent as RectTransform;
			XgUIRect parentRect = new XgUIRect(pRect.position, pRect.rect.size);
			AnchorPresets closestAnchor = parentRect.GetClosestAnchor(rectT.position);

			//Checking if stretching is required or not
            CheckStretchRequirement(rectT, out bool horizontal, out bool vertical);
            if (closestAnchor == AnchorPresets.MiddleCenter && horizontal && vertical)
                closestAnchor = AnchorPresets.StretchAll;
            else if (horizontal)
            {
                if (closestAnchor <= AnchorPresets.TopRight)
                    closestAnchor = AnchorPresets.HorStretchTop;
                else if (closestAnchor <= AnchorPresets.MiddleRight)
                    closestAnchor = AnchorPresets.HorStretchMiddle;
                else
                    closestAnchor = AnchorPresets.HorStretchBottom;
            }
            else if (vertical)
            {
                if (closestAnchor == AnchorPresets.TopLeft || closestAnchor == AnchorPresets.MiddleLeft || closestAnchor == AnchorPresets.BottomLeft)
                    closestAnchor = AnchorPresets.VertStretchLeft;
                else if (closestAnchor == AnchorPresets.TopCenter || closestAnchor == AnchorPresets.MiddleCenter || closestAnchor == AnchorPresets.BottomCenter)
                    closestAnchor = AnchorPresets.VertStretchCenter;
                else
                    closestAnchor = AnchorPresets.VertStretchRight;
            }
            return closestAnchor;
		}

		private void CheckStretchRequirement(RectTransform rectT, out bool horizontal, out bool vertical)
        {
			RectTransform parentRect = rectT.parent as RectTransform;
			horizontal = rectT.rect.size.x / parentRect.rect.size.x >= 0.9f;
			vertical = rectT.rect.size.y / parentRect.rect.size.y >= 0.9f;
		}
	}
}