using TMPro;
using XcelerateGames.UI;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for UiInputItem & TMP_InputField type
    /// </summary>
    public static class InputExtensions
    {
        /// <summary>
        /// Set text of input field from nullable integer. Value is set only if nullable has a value
        /// </summary>
        /// <param name="tmpText">Instance of UiInputItem</param>
        /// <param name="number">nullable int whose value need to be set</param>
        public static void SetText(this UiInputItem tmpText, int? number)
        {
            if (number.HasValue)
                tmpText.text = number.ToString();
        }

        /// <summary>
        /// Set text of input field from nullable float. Value is set only if nullable has a value
        /// </summary>
        /// <param name="tmpText">Instance of UiInputItem</param>
        /// <param name="number">nullable int whose value need to be set</param>
        public static void SetText(this UiInputItem tmpText, float? number)
        {
            if (number.HasValue)
                tmpText.text = number.ToString();
        }

        /// <summary>
        /// Set text of input field from integer.
        /// </summary>
        /// <param name="tmpText">Instance of UiInputItem</param>
        /// <param name="number">value need to be set</param>
        public static void SetText(this UiInputItem tmpText, int number)
        {
            tmpText.text = number.ToString();
        }

        /// <summary>
        /// Set text of input field from nullable integer. Value is set only if nullable has a value
        /// </summary>
        /// <param name="tmpText">Instance of TMP_InputField</param>
        /// <param name="number">nullable int whose value need to be set</param>
        public static void SetText(this TMP_InputField tmpText, int? number)
        {
            if (number.HasValue)
                tmpText.text = number.ToString();
        }

        /// <summary>
        /// Set text of input field from nullable float. Value is set only if nullable has a value
        /// </summary>
        /// <param name="tmpText">Instance of TMP_InputField</param>
        /// <param name="number">nullable int whose value need to be set</param>
        public static void SetText(this TMP_InputField tmpText, float? number)
        {
            if (number.HasValue)
                tmpText.text = number.ToString();
        }

        /// <summary>
        /// Set text of input field from integer.
        /// </summary>
        /// <param name="tmpText">Instance of TMP_InputField</param>
        /// <param name="number">value need to be set</param>
        public static void SetText(this TMP_InputField tmpText, int number)
        {
            tmpText.text = number.ToString();
        }
    }
}
