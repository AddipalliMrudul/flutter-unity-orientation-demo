using TMPro;

namespace XcelerateGames
{
    /// <summary>
    /// Extension methods for TextMeshPro
    /// </summary>
    public static class TMPExtensions
    {
        /// <summary>
        /// Set text if the value exists
        /// </summary>
        /// <param name="tmpText">TextMeshProUGUI object</param>
        /// <param name="number">nullable number</param>
        public static void SetText(this TextMeshProUGUI tmpText, int? number)
        {
            if (number.HasValue)
                tmpText.text = number.ToString();
        }

        /// <summary>
        /// Set text if the value exists
        /// </summary>
        /// <param name="tmpText">TextMeshProUGUI object</param>
        /// <param name="number">nullable number</param>
        public static void SetText(this TextMeshProUGUI tmpText, float? number)
        {
            if (number.HasValue)
                tmpText.text = number.ToString();
        }
    }
}
