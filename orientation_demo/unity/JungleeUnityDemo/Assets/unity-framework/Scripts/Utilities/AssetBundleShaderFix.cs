using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace XcelerateGames
{
    /// <summary>
    /// AssetBundle shader fix.
    /// Add this component to the bundled prefab. This is only on IDE, else shader will be missing.
    /// </summary>
    [DisallowMultipleComponent]
    public class AssetBundleShaderFix : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool recursive = true;
        public float _WaitTimer = 0.5f;

        void OnEnable()
        {
            Fix();
        }
        [ContextMenu("Fix")]
        private void Fix()
        {
            Renderer[] renderers = null;

            if (recursive)
                renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            else
            {
                if (GetComponent<Renderer>() != null)
                    renderers = new Renderer[1] { GetComponent<Renderer>() };
            }

            if (renderers != null && renderers.Length > 0)
            {
                foreach (Renderer r in renderers)
                {
                    for (int i = 0; i < r.materials.Length; i++)
                        r.materials[i].shader = Shader.Find(r.materials[i].shader.name);
                }
            }

            Image[] images = null;

            images = gameObject.GetComponentsInChildren<Image>(true);

            if (images != null && images.Length > 0)
            {
                foreach (Image r in images)
                {
                    if (r.material != null)
                        r.material.shader = Shader.Find(r.material.shader.name);
                }
            }

            TextMeshProUGUI[] texts = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach(TextMeshProUGUI text in texts)
            {
                if(text.fontSharedMaterial == null)
                    continue;
                text.fontSharedMaterial.shader = Shader.Find(text.fontSharedMaterial.shader.name);
            }

            TMP_SubMeshUI[] subMash = gameObject.GetComponentsInChildren<TMP_SubMeshUI>(true);
            foreach (TMP_SubMeshUI text in subMash)
            {
                if (text.sharedMaterial == null)
                    continue;
                text.sharedMaterial.shader = Shader.Find(text.sharedMaterial.shader.name);
            }
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(_WaitTimer);
            Fix();
        }

#endif

        public static Material FixShader(Material m)
        {
#if UNITY_EDITOR
            if (m != null)
                m.shader = Shader.Find(m.shader.name);
#endif
            return m;
        }
    }
}
