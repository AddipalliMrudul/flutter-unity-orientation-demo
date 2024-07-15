using System;
using UnityEngine;

namespace XcelerateGames.UI
{
    public class LoadingTextAnim : MonoBehaviour
    {
        public string _Text = ".";
        public int _MaxLength = 3;
        public float _AmimSpeed = 0.2f;

        public Action<string> OnUpdate = null;
        private float mElapsedTime = 0f;

        private string mText = ".";

        void Start()
        {

        }

        void Update()
        {
            mElapsedTime += Time.deltaTime;
            if(mElapsedTime >= _AmimSpeed)
            {
                mElapsedTime = 0f;
                mText += _Text;
                if (mText.Length > _MaxLength)
                    mText = string.Empty;
                OnUpdate?.Invoke(mText);
            }
        }
    }
}
