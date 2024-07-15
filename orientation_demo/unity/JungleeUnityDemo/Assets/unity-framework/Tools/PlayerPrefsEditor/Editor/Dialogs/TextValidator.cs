using System;
using System.Text.RegularExpressions;

namespace XcelerateGames.EditorTools
{
    public class TextValidator
    {
        public enum ErrorType
        {
            Invalid = -1,
            Info = 0,
            Warning = 1,
            Error = 2
        }

        [NonSerialized]
        public ErrorType m_errorType = ErrorType.Invalid;

        [NonSerialized]
        private string m_regEx = string.Empty;

        [NonSerialized]
        private Func<string, bool> m_validationFunction;

        [NonSerialized]
        public string m_failureMsg = string.Empty;

        public TextValidator(ErrorType errorType, string failureMsg, string regEx)
        {
            m_errorType = errorType;
            m_failureMsg = failureMsg;
            m_regEx = regEx;
        }

        public TextValidator(ErrorType errorType, string failureMsg, Func<string, bool> validationFunction)
        {
            m_errorType = errorType;
            m_failureMsg = failureMsg;
            m_validationFunction = validationFunction;
        }

        public bool Validate(string srcString)
        {
            if (m_regEx != string.Empty)
                return Regex.IsMatch(srcString, m_regEx);
            else if (m_validationFunction != null)
                return m_validationFunction(srcString);

            return false;
        }
    }
}