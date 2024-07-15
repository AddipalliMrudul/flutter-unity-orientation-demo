namespace XcelerateGames.Cryptography
{
    public class SecureDouble : SecureVarBase
    {
        private double mValue;
        private double mValue2;

        public double Value => (mValue - mOffset);
        public bool IsValid => (Value == (mValue2 - mOffset2));

        public SecureDouble(double value = 0)
        {
            mValue = value + mOffset;
            mValue2 = value + mOffset2;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void SetValue(double val)
        {
            mValue = mOffset + val;
            mValue2 = mOffset2 + val;
        }

        public void Append(double val)
        {
            mValue = mOffset + val + Value;
            mValue2 = mOffset2 + val + Value;
        }

        public static SecureDouble operator +(SecureDouble v1, SecureDouble v2)
        {
            return new SecureDouble(v1.Value + v2.Value);
        }
    }
}
