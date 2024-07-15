#region

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#endregion

namespace DEX.Engine
{
    /// <summary>
    ///     Dissolve effect for uGUI.
    /// </summary>
    [AddComponentMenu("UI/UIEffects/UIDissolve", 3)]
    public class UIDissolve : BaseMaterialEffect, IMaterialModifier
    {
        private const uint k_ShaderId = 0 << 3;
        private static readonly ParameterTexture s_ParamTex = new ParameterTexture(8, 128, "_ParamTex");
        private static readonly int k_TransitionTexId = Shader.PropertyToID("_TransitionTex");
        private static Texture _defaultTransitionTexture;
        private EffectArea _lastEffectArea;

        private bool _lastKeepAspectRatio;

        [Tooltip("Edge color.")] [SerializeField] [ColorUsage(false)]
        private Color m_Color = new Color(0.0f, 0.25f, 1.0f);

        [Tooltip("Edge color effect mode.")] [SerializeField]
        private ColorMode m_ColorMode = ColorMode.Add;

        [Header("Advanced Option")] [Tooltip("The area for effect.")] [SerializeField]
        protected EffectArea m_EffectArea;

        [Tooltip("Current location[0-1] for dissolve effect. 0 is not dissolved, 1 is completely dissolved.")]
        [FormerlySerializedAs("m_Location")]
        [SerializeField]
        [Range(0, 1)]
        private float m_EffectFactor = 0.5f;

        [Tooltip("Keep effect aspect ratio.")] [SerializeField]
        private bool m_KeepAspectRatio;

        [Header("Effect Player")] [SerializeField]
        private EffectPlayer m_Player;

        [Tooltip("Reverse the dissolve effect.")] [FormerlySerializedAs("m_ReverseAnimation")] [SerializeField]
        private bool m_Reverse = false;

        [Tooltip("Edge softness.")] [SerializeField] [Range(0, 1)]
        private float m_Softness = 0.5f;

        [Tooltip("Noise texture for dissolving (single channel texture).")]
        [SerializeField]
        [FormerlySerializedAs("m_NoiseTexture")]
        private Texture m_TransitionTexture;

        [Tooltip("Edge width.")] [SerializeField] [Range(0, 1)]
        private float m_Width = 0.5f;

        /// <summary>
        ///     Effect factor between 0(start) and 1(end).
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
        ///     Edge width.
        /// </summary>
        public float width
        {
            get => m_Width;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Width, value)) return;
                m_Width = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        ///     Edge softness.
        /// </summary>
        public float softness
        {
            get => m_Softness;
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (Mathf.Approximately(m_Softness, value)) return;
                m_Softness = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        ///     Edge color.
        /// </summary>
        public Color color
        {
            get => m_Color;
            set
            {
                if (m_Color == value) return;
                m_Color = value;
                SetEffectParamsDirty();
            }
        }

        /// <summary>
        ///     Noise texture.
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
        ///     The area for effect.
        /// </summary>
        public EffectArea effectArea
        {
            get => m_EffectArea;
            set
            {
                if (m_EffectArea == value) return;
                m_EffectArea = value;
                SetVerticesDirty();
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
        ///     Color effect mode.
        /// </summary>
        public ColorMode colorMode
        {
            get => m_ColorMode;
            set
            {
                if (m_ColorMode == value) return;
                m_ColorMode = value;
                SetMaterialDirty();
            }
        }

        /// <summary>
        ///     Gets the parameter texture.
        /// </summary>
        public override ParameterTexture paramTex => s_ParamTex;

        private EffectPlayer _player => m_Player ?? (m_Player = new EffectPlayer());

        public override Hash128 GetMaterialHash(Material material)
        {
            if (!isActiveAndEnabled || !material || !material.shader)
                return k_InvalidHash;

            var shaderVariantId = (uint) ((int) m_ColorMode << 6);
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
            newMaterial.shader = connector.FindShader("UIDissolve");
            SetShaderVariants(newMaterial, m_ColorMode);

            newMaterial.SetTexture(k_TransitionTexId, transitionTexture);
            paramTex.RegisterMaterial(newMaterial);
        }

        /// <summary>
        ///     Modifies the mesh.
        /// </summary>
        public override void ModifyMesh(VertexHelper vh, Graphic graphic)
        {
            if (!isActiveAndEnabled)
                return;

            // bool isText = isTMPro || graphic is Text;
            var normalizedIndex = paramTex.GetNormalizedIndex(this);

            // rect.
            var tex = transitionTexture;
            var aspectRatio = m_KeepAspectRatio && tex ? (float) tex.width / tex.height : -1;
            var rect = m_EffectArea.GetEffectArea(vh, rectTransform.rect, aspectRatio);

            // Calculate vertex position.
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

        protected override void SetEffectParamsDirty()
        {
            paramTex.SetData(this, 0, m_EffectFactor); // param1.x : location
            paramTex.SetData(this, 1, m_Width); // param1.y : width
            paramTex.SetData(this, 2, m_Softness); // param1.z : softness
            paramTex.SetData(this, 4, m_Color.r); // param2.x : red
            paramTex.SetData(this, 5, m_Color.g); // param2.y : green
            paramTex.SetData(this, 6, m_Color.b); // param2.z : blue
        }

        protected override void SetVerticesDirty()
        {
            base.SetVerticesDirty();

            _lastKeepAspectRatio = m_KeepAspectRatio;
            _lastEffectArea = m_EffectArea;
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();

            if (_lastKeepAspectRatio != m_KeepAspectRatio
                || _lastEffectArea != m_EffectArea)
                SetVerticesDirty();
        }

        /// <summary>
        ///     Play effect.
        /// </summary>
        public void Play(bool reset = true)
        {
            _player.Play(reset);
        }

        /// <summary>
        ///     Stop effect.
        /// </summary>
        public void Stop(bool reset = true)
        {
            _player.Stop(reset);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _player.OnEnable(f => effectFactor = m_Reverse ? 1f - f : f);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _player.OnDisable();
        }
    }
}