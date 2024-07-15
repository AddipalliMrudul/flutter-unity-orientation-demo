using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for RawImage
    /// </summary>
    public static class RawImageExtensions
    {
        /// <summary>
        /// Scale RawImage to given dimension
        /// </summary>
        /// <param name="image">Image to scale</param>
        /// <param name="targetRect">Target size</param>
        /// <param name="centerAlign">should be scaled from center?</param>
        public static void ScaleToDimension(this RawImage image, Vector2 targetRect, bool centerAlign = false)
        {
            if (image.texture == null)
                return;
            float scaleX = targetRect.x / image.texture.width;
            float scaleY = targetRect.y / image.texture.height;
            float targetScale = Mathf.Min(scaleX, scaleY);
            float newWidth = image.texture.width * targetScale;
            float newHeight = image.texture.height * targetScale;
            image.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
            if (centerAlign)
            {
                //image.rectTransform.anchoredPosition = new Vector2(0, -newHeight * 0.5f);
                image.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// Resize RawImage to fit to parent 
        /// </summary>
        /// <param name="image">Instance of RawImage</param>
        /// <param name="padding">Padding to use</param>
        /// <returns></returns>
        public static Vector2 SizeToParent(this RawImage image, float padding = 0)
        {
            float w = 0, h = 0;
            var parent = image.GetComponentInParent<RectTransform>();
            var imageTransform = image.GetComponent<RectTransform>();

            // check if there is something to do
            if (image.texture != null)
            {
                if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
                padding = 1 - padding;
                float ratio = image.texture.width / (float)image.texture.height;
                var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
                if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
                {
                    //Invert the bounds if the image is rotated
                    bounds.size = new Vector2(bounds.height, bounds.width);
                }
                //Size by height first
                h = bounds.height * padding;
                w = h * ratio;
                if (w > bounds.width * padding)
                { //If it doesn't fit, fallback to width;
                    w = bounds.width * padding;
                    h = w / ratio;
                }
            }
            imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            return imageTransform.sizeDelta;
        }
    }
}
