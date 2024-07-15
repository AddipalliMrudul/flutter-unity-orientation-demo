namespace XcelerateGames.EditorTools
{
    [System.Serializable]
    public class PreferenceEntry
    {
        public enum PrefTypes
        {
            String = 0,
            Int = 1,
            Float = 2
        }

        public PrefTypes m_typeSelection;
        public string m_key;
        public string m_strValue;
        public int m_intValue;
        public float m_floatValue;
    }
}