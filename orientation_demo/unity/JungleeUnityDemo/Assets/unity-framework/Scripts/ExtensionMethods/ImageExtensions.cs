using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for Image type
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Resize the image to given size
        /// </summary>
        /// <param name="image">Image to resize</param>
        /// <param name="targetRect">new size</param>
        /// <param name="centerAlign">Should be scaled from center?</param>
        public static void ScaleToDimension(this Image image, Vector2 targetRect, bool centerAlign = false)
        {
            if (image.sprite == null)
                return;
            float scaleX = targetRect.x / image.sprite.texture.width;
            float scaleY = targetRect.y / image.sprite.texture.height;
            float targetScale = Mathf.Min(scaleX, scaleY);
            float newWidth = image.sprite.texture.width * targetScale;
            float newHeight = image.sprite.texture.height * targetScale;
            image.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
            if (centerAlign)
            {
                image.rectTransform.anchoredPosition = new Vector2(0, -newHeight * 0.5f);
            }
        }

        /// <summary>
        /// Change only alpha of Image
        /// </summary>
        /// <param name="image">Image instance</param>
        /// <param name="alpha">Aplha to set</param>
        public static void SetAlpha(this Image image, float alpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }
    }
}
