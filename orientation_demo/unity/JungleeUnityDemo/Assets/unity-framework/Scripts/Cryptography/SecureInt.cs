namespace XcelerateGames.Cryptography
{
    public class SecureInt : SecureVarBase
    {
        private int mValue;
        private int mValue2;

        public int Value => (mValue - mOffset);
        public bool IsValid => (Value == (mValue2 - mOffset2));

        public SecureInt(int value = 0)
        {
            mValue = value + mOffset;
            mValue2 = value + mOffset2;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void SetValue(int val)
        {
            mValue = mOffset + val;
            mValue2 = mOffset2 + val;
        }

        public void Append(int val)
        {
            mValue = mOffset + val + Value;
            mValue2 = mOffset2 + val + Value;
        }

        public static SecureInt operator +(SecureInt v1, SecureInt v2)
        {
            return new SecureInt(v1.Value + v2.Value);
        }
    }
}
