using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames
{
    /// <summary>
    /// Utility class for color related functionality
    /// </summary>
    public static class ColorUtilities
    {
        /// <summary>
        /// Set color by hex value including alpha
        /// https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html
        /// </summary>
        /// <param name="graphic">Graphic on which color needs to be applied</param>
        /// <param name="hexRGBA">hex color in RGBA format</param>
        /// <returns>true is color applied else false</returns>
        public static bool Color(this Graphic graphic, string hexRGBA)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(hexRGBA, out color))
            {
                graphic.color = color;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set color by hex value excluding alpha, Alpha is passed separately
        /// https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html
        /// </summary>
        /// <param name="graphic">Graphic on which color needs to be applied</param>
        /// <param name="hexRGBA">hex color in RGB format</param>
        /// <returns>true is color applied else false</returns>
        public static bool Color(this Graphic graphic, string hexRGB, float alpha)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(hexRGB, out color))
            {
                color.a = alpha;
                graphic.color = color;
                return true;
            }
            return false;
        }
}
}