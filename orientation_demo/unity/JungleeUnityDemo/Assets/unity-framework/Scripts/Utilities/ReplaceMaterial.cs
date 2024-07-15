using UnityEngine;

namespace XcelerateGames
{
    public class ReplaceMaterial : MonoBehaviour
    {
        [SerializeField] float _Delay = 1f;
        [SerializeField] Material _Material = null;
        [SerializeField] bool _Recursive = true;

        private void Start()
        {
            Invoke(nameof(ChangeMaterial), _Delay);
        }

        [ContextMenu("Change Now")]
        private void ChangeMaterial()
        {
            if (_Material != null)
            {
                Shader shader = Shader.Find(_Material.shader.name);
                MeshRenderer[] meshRenderers = null;
                if (_Recursive)
                    meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
                else
                    meshRenderers = gameObject.GetComponents<MeshRenderer>();
                for (int i = 0; i < meshRenderers.Length; ++i)
                {
                    meshRenderers[i].sharedMaterial = _Material;
                    meshRenderers[i].sharedMaterial.shader = shader;
                }

                XDebug.Log($"Updated {meshRenderers.Length} MeshRenderers under : {gameObject.GetObjectPath()}");
            }
            else
                XDebug.LogWarning("Warning! _Material is null.");

            enabled = false;
        }
    }
}
