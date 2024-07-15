using System;
using System.Security.Cryptography;
using UnityEngine;

namespace XcelerateGames.Cryptography
{
    public class SecureVarBase
    {
        protected int mOffset, mOffset2;
        protected string mHash;

        //public int Value => Operator (mValue - mOffset);

        public SecureVarBase()
        {
            mOffset = UnityEngine.Random.Range(1111, 99999);
            mOffset2 = UnityEngine.Random.Range(1111, 99999);
        }

        protected string Hash(string val)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(val)));
            }
        }
    }
}
