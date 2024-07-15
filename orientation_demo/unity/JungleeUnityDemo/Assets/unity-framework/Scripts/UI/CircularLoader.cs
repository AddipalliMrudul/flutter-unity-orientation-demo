using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    public class CircularLoader : MonoBehaviour
    {
        #region Serialized Property
        [SerializeField] Image _Image = null;
        [SerializeField] float _Time = 1f;
        float mElapsedTime = 0f;
        #endregion

        #region Unity Callbacks
        void Update()
        {
            mElapsedTime += Time.deltaTime;
            if (mElapsedTime <= _Time)
                _Image.fillAmount = (Mathf.Lerp(0, 1, mElapsedTime / _Time));
            else
                mElapsedTime = 0;
        }
        #endregion
    }
}