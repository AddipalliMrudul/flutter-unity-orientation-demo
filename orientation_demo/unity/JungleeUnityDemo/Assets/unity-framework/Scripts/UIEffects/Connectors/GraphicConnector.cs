#region

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace DEX.Engine
{
    public class GraphicConnector : BaseConnector
    {
        protected override int priority => 0;

        public override AdditionalCanvasShaderChannels extraChannel => AdditionalCanvasShaderChannels.TexCoord1;
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            AddConnector(new GraphicConnector());
        }

        public override Material GetMaterial(Graphic graphic)
        {
            return graphic ? graphic.material : null;
        }

        public override void SetMaterial(Graphic graphic, Material material)
        {
            if (graphic)
                graphic.material = material;
        }

        public override Shader FindShader(string shaderName)
        {
            return Shader.Find("Hidden/" + shaderName);
        }

        protected override bool IsValid(Graphic graphic)
        {
            return graphic;
        }

        public override void SetVerticesDirty(Graphic graphic)
        {
            if (graphic)
                graphic.SetVerticesDirty();
        }

        public override void SetMaterialDirty(Graphic graphic)
        {
            if (graphic)
                graphic.SetMaterialDirty();
        }

        public override bool IsText(Graphic graphic)
        {
            return graphic && graphic is Text;
        }

        public override void SetExtraChannel(ref UIVertex vertex, Vector2 value)
        {
            vertex.uv1 = value;
        }
    }
}