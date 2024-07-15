using UnityEditor;

namespace XcelerateGames.Editor.UI
{
    public class CreateModel : CreateTemplateBase
    {
        [MenuItem("Assets/" + Utilities.MenuName + "Create/Model")]
        static void DoSetAssetBundleName()
        {
            GetWindow<CreateModel>();
        }

        protected override void Awake()
        {
            base.Awake();
            mTemplateName = "ModelTemplate.txt";
        }
    }
}