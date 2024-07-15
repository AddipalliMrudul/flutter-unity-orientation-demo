#region

using UnityEngine;
using UnityEngine.UI;

#endregion

namespace DEX.Engine
{
    /// <summary>
    ///     Transition effect.
    /// </summary>
    [AddComponentMenu("UI/UIEffects/UITransitionEffect", 5)]
    public class UITransitionEffect : BaseMaterialEffect
    {
        /// <summary>
        ///     Effect mode.
        /// </summary>
        public enum EffectMode
        {
            Fade = 1,
            Cutoff = 2,
            Dissolve = 3
        }

        private const uint k_ShaderId = 5 << 3;
        private static readonly int k_TransitionTexId = Shader.PropertyToID("_TransitionTex");
        private static readonly ParameterTexture s_ParamTex = new ParameterTexture(8, 128, "_ParamTex");
        private static Texture _defaultTransitionTexture;

        private bool _lastKeepAspectRatio;

        [Tooltip("Dissolve edge color.")] [SerializeField] [ColorUsage(false)]
        private Color m_DissolveColor = new Color(0.0f, 0.25f, 1.0f);

        [Tooltip("Dissolve edge softness.")] [SerializeField] [Range(0, 1)]
        private float m_DissolveSoftness = 0.5f;

        [Tooltip("Dissolve edge width.")] [SerializeField] [Range(0, 1)]
        private float m_DissolveWidth = 0.5f;

        [Header("Advanced Option")] [Tooltip("The area for effect.")] [SerializeField]
        private EffectArea m_EffectArea = EffectArea.RectTransform;

        [Tooltip("Effect factor between 0(hidden) and 1(shown).")] [SerializeField] [Range(0, 1)]
        private float m_EffectFactor = 0.5f;

        [Tooltip("Effect mode.")] [SerializeField]
        private EffectMode m_EffectMode = EffectMode.Cutoff;

        [Tooltip("Keep effect aspect ratio.")] [SerializeField]
        private bool m_KeepAspectRatio;

        [Tooltip("Disable the graphic's raycast target on hidden.")] [SerializeField]
        private bool m_PassRayOnHidden;

        [Header("Effect Player")] [SerializeField]
        private EffectPlayer m_Player;

        [Tooltip("Transition texture (single channel texture).")] [SerializeField]
        private Texture m_TransitionTexture;


        /// <summary>
        ///     Effect factor between 0(no effect) and 1(complete effect).
        /// </summary>
        public float effectFactor
        {
            get => m_EffectFactor;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_EffectFactor, value)) return;
                m_EffectFactor = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        ///     Transition texture.
        /// </summary>
        public Texture transitionTexture
        {
            get =>
                m_TransitionTexture
                    ? m_TransitionTexture
                    : defaultTransitionTexture;
            set
            {
                if (m_TransitionTexture == value) return;
                m_TransitionTexture = value;
                SetMaterialDirty();
            }
        }

        private static Texture defaultTransitionTexture =>
            _defaultTransitionTexture
                ? _defaultTransitionTexture
                : _defaultTransitionTexture = Resources.Load<Texture>("Default-Transition");

        /// <summary>
        ///     Effect mode.
        /// </summary>
        public EffectMode effectMode
        {
            get => m_EffectMode;
            set
            {
                if (m_EffectMode == value) return;
                m_EffectMode = value;
                SetMaterialDirty();
            }
        }

        /// <summary>
        ///     Keep aspect ratio.
        /// </summary>
        public bool keepAspectRatio
        {
            get => m_KeepAspectRatio;
            set
            {
                if (m_KeepAspectRatio == value) return;
                m_KeepAspectRatio = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        ///     Gets the parameter texture.
        /// </summary>
        public override ParameterTexture paramTex => s_ParamTex;

        /// <summary>
        ///     Dissolve edge width.
        /// </summary>
        public float dissolveWidth
        {
            get => m_DissolveWidth;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_DissolveWidth, value)) return;
                m_DissolveWidth = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        ///     Dissolve edge softness.
        /// </summary>
        public float dissolveSoftness
        {
            get => m_DissolveSoftness;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_DissolveSoftness, value)) return;
                m_DissolveSoftness = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        ///     Dissolve edge color.
        /// </summary>
        public Color dissolveColor
        {
            get => m_DissolveColor;
            set
            {
                if (m_DissolveColor == value) return;
                m_DissolveColor = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        ///     Disable graphic's raycast target on hidden.
        /// </summary>
        public bool passRayOnHidden
        {
            get => m_PassRayOnHidden;
            set => m_PassRayOnHidden = value;
        }

        private EffectPlayer _player => m_Player ?? (m_Player = new EffectPlayer());

        /// <summary>
        ///     Show transition.
        /// </summary>
        public void Show(bool reset = true)
        {
            _player.loop = false;
            _player.Play(reset, f => effectFactor = f);
        }

        /// <summary>
        ///     Hide transition.
        /// </summary>
        public void Hide(bool reset = true)
        {
            _player.loop = false;
            _player.Play(reset, f => effectFactor = 1 - f);
        }


        public override Hash128 GetMaterialHash(Material material)
        {
            if (!isActiveAndEnabled || !material || !material.shader)
                return k_InvalidHash;

            var shaderVariantId = (uint) ((int) m_EffectMode << 6);
            var resourceId = (uint) transitionTexture.GetInstanceID();
            return new Hash128(
                (uint) material.GetInstanceID(),
                k_ShaderId + shaderVariantId,
                resourceId,
                0
            );
        }

        public override void ModifyMaterial(Material newMaterial, Graphic graphic)
        {
            var connector = BaseConnector.FindConnector(graphic);
            newMaterial.shader = connector.FindShader("UITransition");
            SetShaderVariants(newMaterial, m_EffectMode);

            newMaterial.SetTexture(k_TransitionTexId, transitionTexture);
            paramTex.RegisterMaterial(newMaterial);
        }

        /// <summary>
        ///     Modifies the mesh.
        /// </summary>
        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled) return;

            var normalizedIndex = paramTex.GetNormalizedIndex(this);

            // rect.
            var tex = transitionTexture;
            var aspectRatio = m_KeepAspectRatio && tex ? (float) tex.width / tex.height : -1;
            var rect = m_EffectArea.GetEffectArea(vh, rectTransform.rect, aspectRatio);

            // Set parameters to vertex.
            var vertex = default(UIVertex);
            var count = vh.currentVertCount;
            for (var i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                float x;
                float y;
                connector.GetPositionFactor(m_EffectArea, i, rect, vertex.position, out x, out y);

                vertex.uv0 = new Vector2(
                    Packer.ToFloat(vertex.uv0.x, vertex.uv0.y),
                    Packer.ToFloat(x, y, normalizedIndex)
                );
                vh.SetUIVertex(vertex, i);
            }
        }

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _player.OnEnable();
            _player.loop = false;
        }

        /// <summary>
        ///     This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            _player.OnDisable();
        }

        protected override void SetEffectParamsDirty()
        {
            paramTex.SetData(this, 0, m_EffectFactor); // param1.x : effect factor
            if (m_EffectMode == EffectMode.Dissolve)
            {
                paramTex.SetData(this, 1, m_DissolveWidth); // param1.y : width
                paramTex.SetData(this, 2, m_DissolveSoftness); // param1.z : softness
                paramTex.SetData(this, 4, m_DissolveColor.r); // param2.x : red
                paramTex.SetData(this, 5, m_DissolveColor.g); // param2.y : green
                paramTex.SetData(this, 6, m_DissolveColor.b); // param2.z : blue
            }

            // Disable graphic's raycastTarget on hidden.
            if (m_PassRayOnHidden) graphic.raycastTarget = 0 < m_EffectFactor;
        }

        protected override void SetVerticesDirty()
        {
            base.SetVerticesDirty();

            _lastKeepAspectRatio = m_KeepAspectRatio;
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();

            if (_lastKeepAspectRatio != m_KeepAspectRatio)
                SetVerticesDirty();
        }
    }
}