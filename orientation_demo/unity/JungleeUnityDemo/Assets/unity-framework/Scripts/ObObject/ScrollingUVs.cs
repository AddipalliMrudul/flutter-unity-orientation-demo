using UnityEngine;

namespace XcelerateGames
{
    public class ScrollingUVs : MonoBehaviour
    {
        public int materialIndex = 0;
        public Vector2 uvAnimationRate = new Vector2(1.0f, 0.0f);
        public string _TextureName = "_MainTex";

        Vector2 uvOffset = Vector2.zero;
        Vector2 mDefaultUVOffset = Vector2.zero;

        private Material mMaterial = null;

        private void Awake()
        {
            mMaterial = gameObject.GetComponent<Renderer>().materials[materialIndex];
            mDefaultUVOffset = mMaterial.GetTextureOffset(_TextureName);
        }

        void LateUpdate()
        {
            uvOffset += (uvAnimationRate * Time.deltaTime);
            mMaterial.SetTextureOffset(_TextureName, uvOffset);
        }

        public void ResetOffset()
        {
            mMaterial.SetTextureOffset(_TextureName, mDefaultUVOffset);
        }
    }
}
