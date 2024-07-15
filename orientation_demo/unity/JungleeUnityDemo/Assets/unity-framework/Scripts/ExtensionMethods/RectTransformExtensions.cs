using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    /// <summary>
    /// Anchor Presets
    /// </summary>
    /// @see SetAnchor
    public enum AnchorPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight,
        BottomStretch,

        VertStretchLeft,
        VertStretchRight,
        VertStretchCenter,

        HorStretchTop,
        HorStretchMiddle,
        HorStretchBottom,

        StretchAll
    }

    /// <summary>
    /// Pivot Presets
    /// </summary>
    /// @see RectTransformExtensions::SetPivot(RectTransform, PivotPresets)
    public enum PivotPresets
    {
        TopLeft,        /**<Top Left Corner */
        TopCenter,      /**<Top Center */
        TopRight,       /**<Top Right Corner */

        MiddleLeft,     /**<Middle Left */
        MiddleCenter,   /**<Middle Center */
        MiddleRight,    /**<Middle Right */

        BottomLeft,     /**<Bottom Left Corner */
        BottomCenter,   /**<Bottom Center */
        BottomRight,    /**<Bottom Right Corner */
    }


    /// <summary>
    /// Extension class for RectTransform
    /// </summary>
    public static class RectTransformExtensions
    {
        /// <summary>
        /// Set anchor of RectTransform
        /// </summary>
        /// <param name="source">Instance of RectTransform</param>
        /// <param name="allign">AnchorPresets to apply</param>
        /// <param name="offsetX">X Offset</param>
        /// <param name="offsetY">Y offset</param>
        public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
        {
            source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

            switch (allign)
            {
                case (AnchorPresets.TopLeft):
                    {
                        source.anchorMin = new Vector2(0, 1);
                        source.anchorMax = new Vector2(0, 1);
                        break;
                    }
                case (AnchorPresets.TopCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 1);
                        source.anchorMax = new Vector2(0.5f, 1);
                        //source.sizeDelta = new Vector2(Screen.width, 0);
                        break;
                    }
                case (AnchorPresets.TopRight):
                    {
                        source.anchorMin = new Vector2(1, 1);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }

                case (AnchorPresets.MiddleLeft):
                    {
                        source.anchorMin = new Vector2(0, 0.5f);
                        source.anchorMax = new Vector2(0, 0.5f);
                        break;
                    }
                case (AnchorPresets.MiddleCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0.5f);
                        source.anchorMax = new Vector2(0.5f, 0.5f);
                        break;
                    }
                case (AnchorPresets.MiddleRight):
                    {
                        source.anchorMin = new Vector2(1, 0.5f);
                        source.anchorMax = new Vector2(1, 0.5f);
                        break;
                    }

                case (AnchorPresets.BottomLeft):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(0, 0);
                        break;
                    }
                case (AnchorPresets.BottomCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0);
                        source.anchorMax = new Vector2(0.5f, 0);
                        break;
                    }
                case (AnchorPresets.BottomRight):
                    {
                        source.anchorMin = new Vector2(1, 0);
                        source.anchorMax = new Vector2(1, 0);
                        break;
                    }

                case (AnchorPresets.HorStretchTop):
                    {
                        source.anchorMin = new Vector2(0, 1);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }
                case (AnchorPresets.HorStretchMiddle):
                    {
                        source.anchorMin = new Vector2(0, 0.5f);
                        source.anchorMax = new Vector2(1, 0.5f);
                        break;
                    }
                case (AnchorPresets.HorStretchBottom):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(1, 0);
                        break;
                    }

                case (AnchorPresets.VertStretchLeft):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(0, 1);
                        break;
                    }
                case (AnchorPresets.VertStretchCenter):
                    {
                        source.anchorMin = new Vector2(0.5f, 0);
                        source.anchorMax = new Vector2(0.5f, 1);
                        break;
                    }
                case (AnchorPresets.VertStretchRight):
                    {
                        source.anchorMin = new Vector2(1, 0);
                        source.anchorMax = new Vector2(1, 1);
                        break;
                    }

                case (AnchorPresets.StretchAll):
                    {
                        source.anchorMin = new Vector2(0, 0);
                        source.anchorMax = new Vector2(1, 1);
                        source.sizeDelta = Vector2.zero;
                        break;
                    }
            }
        }

        /// <summary>
        /// Get pivot of RectTransform
        /// </summary>
        /// <param name="rectTransform">Instance of RectTransform</param>
        /// <returns>PivotPresets</returns>
        public static PivotPresets GetPivot(this RectTransform rectTransform)
        {
            if (rectTransform.pivot.x == 0 && rectTransform.pivot.y == 1)
                return PivotPresets.TopLeft;
            if (rectTransform.pivot.x == 0.5 && rectTransform.pivot.y == 1)
                return PivotPresets.TopCenter;
            if (rectTransform.pivot.x == 1 && rectTransform.pivot.y == 1)
                return PivotPresets.TopRight;

            if (rectTransform.pivot.x == 0 && rectTransform.pivot.y == 0.5)
                return PivotPresets.MiddleLeft;
            if (rectTransform.pivot.x == 0.5 && rectTransform.pivot.y == 0.5)
                return PivotPresets.MiddleCenter;
            if (rectTransform.pivot.x == 1 && rectTransform.pivot.y == 0.5)
                return PivotPresets.MiddleRight;

            if (rectTransform.pivot.x == 0 && rectTransform.pivot.y == 0)
                return PivotPresets.BottomLeft;
            if (rectTransform.pivot.x == 0.5 && rectTransform.pivot.y == 0)
                return PivotPresets.BottomCenter;
            if (rectTransform.pivot.x == 1 && rectTransform.pivot.y == 0)
                return PivotPresets.BottomRight;

            XDebug.LogException("Could not find Pivot");
            return PivotPresets.MiddleCenter;
        }

        /// <summary>
        /// Get pivot of RectTransform
        /// </summary>
        /// <param name="transform">Instance of Transform</param>
        /// <returns>PivotPresets</returns>
        public static PivotPresets GetPivot(this Transform transform)
        {
            return transform.GetComponent<RectTransform>().GetPivot();
        }

        /// <summary>
        /// Set pivot of RectTransform
        /// </summary>
        /// <param name="source">Instance of RectTransform</param>
        /// <param name="preset">PivotPresets</param>
        public static void SetPivot(this RectTransform source, PivotPresets preset)
        {

            switch (preset)
            {
                case (PivotPresets.TopLeft):
                    {
                        source.pivot = new Vector2(0, 1);
                        break;
                    }
                case (PivotPresets.TopCenter):
                    {
                        source.pivot = new Vector2(0.5f, 1);
                        break;
                    }
                case (PivotPresets.TopRight):
                    {
                        source.pivot = new Vector2(1, 1);
                        break;
                    }

                case (PivotPresets.MiddleLeft):
                    {
                        source.pivot = new Vector2(0, 0.5f);
                        break;
                    }
                case (PivotPresets.MiddleCenter):
                    {
                        source.pivot = new Vector2(0.5f, 0.5f);
                        break;
                    }
                case (PivotPresets.MiddleRight):
                    {
                        source.pivot = new Vector2(1, 0.5f);
                        break;
                    }

                case (PivotPresets.BottomLeft):
                    {
                        source.pivot = new Vector2(0, 0);
                        break;
                    }
                case (PivotPresets.BottomCenter):
                    {
                        source.pivot = new Vector2(0.5f, 0);
                        break;
                    }
                case (PivotPresets.BottomRight):
                    {
                        source.pivot = new Vector2(1, 0);
                        break;
                    }
            }
        }

        /// <summary>
        /// Get world position of given RectTransform
        /// </summary>
        /// <param name="rectTransform">Instance of RectTransform</param>
        /// <returns>World position</returns>
        public static Vector3 GetWorldPosition(this RectTransform rectTransform)
        {
            Vector3[] v = new Vector3[4];
            rectTransform.GetWorldCorners(v);

            Vector3 worldPos = Vector3.zero;

            worldPos.x = (v[0].x + v[2].x) / 2;
            worldPos.y = (v[0].y + v[1].y) / 2;

            return worldPos;
        }

        /// <summary>
        /// X is width & Y is height
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        public static Vector2 GetSize(this RectTransform rectTransform)
        {
            return rectTransform.sizeDelta;
        }

        /// <summary>
        /// Set height of given RectTransform
        /// </summary>
        /// <param name="rectTransform">Instance of RectTransform</param>
        /// <param name="height">height to set</param>
        public static void Height(this RectTransform rectTransform, float height)
        {
            Vector2 size = rectTransform.GetSize();
            size.y = height;
            rectTransform.sizeDelta = size;
        }

        /// <summary>
        /// Set width of the given RectTransform
        /// </summary>
        /// <param name="rectTransform">Instance of RectTransform</param>
        /// <param name="width">width to set</param>
        public static void Width(this RectTransform rectTransform, float width)
        {
            Vector2 size = rectTransform.GetSize();
            size.x = width;
            rectTransform.sizeDelta = size;
        }

        /// <summary>
        /// Auto scroll ScrollRect to adjust itself to snap to given RectTransform
        /// </summary>
        /// <param name="scrollRect">Instance of ScrollRect</param>
        /// <param name="target">Target RectTransform to scroll to</param>
        public static void SnapTo(this ScrollRect scrollRect, RectTransform target)
        {
            Canvas.ForceUpdateCanvases();

            scrollRect.content.anchoredPosition = (Vector2)scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
        }

        ///// <summary>
        ///// Checks if two RectTransforms overlap each other
        ///// </summary>
        ///// <param name="rectTransform1">First RectTransform</param>
        ///// <param name="rectTransform2">Second RectTransform</param>
        ///// <returns>True if overlapping else false</returns>
        public static bool Overlaps(this RectTransform rectTransform1, RectTransform rectTransform2)
        {
            return rectTransform1.WorldRect().Overlaps(rectTransform2.WorldRect());
        }

        /// <summary>
        /// Checks if two RectTransforms overlap each other
        /// </summary>
        /// <param name="rectTransform1">First RectTransform</param>
        /// <param name="rectTransform2">Second RectTransform</param>
        /// <param name="allowInverse">Allo Inverse</param>
        /// <returns>True if overlapping else false</returns>
        public static bool Overlaps(this RectTransform rectTransform1, RectTransform rectTransform2, bool allowInverse)
        {
            return rectTransform1.WorldRect().Overlaps(rectTransform2.WorldRect(), allowInverse);
        }

        /// <summary>
        /// World rect of RectTransform
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <returns>Rect</returns>
        public static Rect WorldRect(this RectTransform rectTransform)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
            float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

            Vector3 position = rectTransform.position;
            return new Rect(position.x - rectTransformWidth / 2f, position.y - rectTransformHeight / 2f, rectTransformWidth, rectTransformHeight);
        }

        /// <summary>
        /// Get a 'Rect' relative to Screen Dimensions
        /// </summary>
        /// <param name="relative">RectTransform of the gameobject</param>
        /// <returns>Rect</returns>
        public static Rect RelativeToReal(Rect relative)
        {
            return new Rect(
                relative.x * Screen.width,
                relative.y * Screen.height,
                relative.width * Screen.width,
                relative.height * Screen.height
            );
        }

        /// <summary>
        /// Get a Rect relative to Screen Resolution
        /// </summary>
        /// <param name="absoluteRect">RectTransform of the gameobject</param>
        /// <returns>Rect</returns>
        public static Rect ToScreenRelativeRect(Rect absoluteRect)
        {
            int w = Screen.currentResolution.width;
            int h = Screen.currentResolution.height;
            return new Rect(
                absoluteRect.x / w,
                absoluteRect.y / h,
                absoluteRect.width / w,
                absoluteRect.height / h
            );
        }

        /// <summary>
        /// Returns the amount of overlap between two 'RectTransforms'. Works in both 'Screenspace-Overlay' and 'Screenspace-Camera'
        /// </summary>
        /// <param name="camera">Camera used for rendering canvas</param>
        /// <param name="rectTransformA">RectTransform A</param>
        /// <param name="rectTransformB">RectTransform B</param>
        /// <returns>Overlap area</returns>
        public static float OverlapArea(Camera camera, RectTransform rectTransformA, RectTransform rectTransformB)
        {
            Vector2 viewportMinCorner;
            Vector2 viewportMaxCorner;
            if (rectTransformB != null)
            {
                Vector3[] v_wcorners = new Vector3[4];
                rectTransformB.GetWorldCorners(v_wcorners);
                viewportMinCorner = camera.WorldToScreenPoint(v_wcorners[0]);
                viewportMaxCorner = camera.WorldToScreenPoint(v_wcorners[2]);
            }
            else
            {
                viewportMinCorner = new Vector2(0, 0);
                viewportMaxCorner = new Vector2(Screen.width, Screen.height);
            }
            viewportMinCorner += Vector2.one;
            viewportMaxCorner -= Vector2.one;
            Vector3[] e_wcorners = new Vector3[4];
            rectTransformA.GetWorldCorners(e_wcorners);
            Vector2 elem_minCorner = camera.WorldToScreenPoint(e_wcorners[0]);
            Vector2 elem_maxCorner = camera.WorldToScreenPoint(e_wcorners[2]);
            var xOverlap = Mathf.Max(0, Mathf.Min(elem_maxCorner.x, viewportMaxCorner.x) - Mathf.Max(elem_minCorner.x, viewportMinCorner.x));
            var yOverlap = Mathf.Max(0, Mathf.Min(elem_maxCorner.y, viewportMaxCorner.y) - Mathf.Max(elem_minCorner.y, viewportMinCorner.y));
            return xOverlap * yOverlap;
        }

        ///// <summary>
        ///// Checks if two RectTransforms overlap each other on world 
        ///// </summary>
        ///// <param name="rectTransform1">First RectTransform</param>
        ///// <param name="rectTransform2">Second RectTransform</param>
        ///// <returns>True if overlapping else false</returns>
        public static bool OverlapsWorldRect(this RectTransform rectTransform1, RectTransform rectTransform2)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform1.GetWorldCorners(corners);
            Rect rec = new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);

            Vector3[] corners2 = new Vector3[4];
            rectTransform2.GetWorldCorners(corners2);
            Rect rec2 = new Rect(corners2[0].x, corners2[0].y, corners2[2].x - corners2[0].x, corners2[2].y - corners2[0].y);

            if (rec.Overlaps(rec2))
            {
                return true;
            }

            return false;
        }
    }
}