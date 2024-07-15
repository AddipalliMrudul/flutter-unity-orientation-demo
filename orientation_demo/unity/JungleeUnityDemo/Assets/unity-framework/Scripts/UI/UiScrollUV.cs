using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.UI
{
    /// <summary>
    /// To scroll an image by changingits UV.
    /// #example: Match making, billboards etc
    /// </summary>

    [RequireComponent(typeof(Image))]
    public class UiScrollUV : MonoBehaviour
    {
        /// <summary>
        /// Scroll direction
        /// </summary>
        public enum ScrollDirection
        {
            Vertical,
            Horizontal
        }

        [SerializeField] ScrollDirection _Direction = ScrollDirection.Vertical;
        [SerializeField] Image _Image;
        [SerializeField] float _Speed = 0.25f;
        [SerializeField] bool _UseUnscaledTime = false;
        [SerializeField] string _PropertyName = "_MainTex";

        Vector2 mTexOffset = Vector2.zero;

        void Start()
        {
        }

        protected virtual void Update()
        {
            float val = (_UseUnscaledTime? Time.unscaledDeltaTime: Time.deltaTime) * _Speed;
            if (_Direction == ScrollDirection.Horizontal)
                mTexOffset.x += val;
            else if(_Direction == ScrollDirection.Vertical)
                mTexOffset.y += val;
            _Image.materialForRendering.SetTextureOffset(_PropertyName, mTexOffset);
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (_Image == null)
                _Image = GetComponent<Image>();
        }
    }
}
