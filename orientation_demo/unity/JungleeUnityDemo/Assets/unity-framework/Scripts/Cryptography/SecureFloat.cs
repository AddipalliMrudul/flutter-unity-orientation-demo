namespace XcelerateGames.Cryptography
{
    public class SecureFloat : SecureVarBase
    {
        private float mValue;
        private float mValue2;

        public float Value => (mValue - mOffset);
        public bool IsValid => (Value == (mValue2 - mOffset2));

        public SecureFloat(float value = 0)
        {
            mValue = value + mOffset;
            mValue2 = value + mOffset2;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void SetValue(float val)
        {
            mValue = mOffset + val;
            mValue2 = mOffset2 + val;
        }

        public void Append(float val)
        {
            mValue = mOffset + val + Value;
            mValue2 = mOffset2 + val + Value;
        }

        public static SecureFloat operator +(SecureFloat v1, SecureFloat v2)
        {
            return new SecureFloat(v1.Value + v2.Value);
        }
    }
}
