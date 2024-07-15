using UnityEditor;

namespace XcelerateGames.Editor.UI
{
    public class CreateUIClass : CreateTemplateBase
    {
        [MenuItem("Assets/" + Utilities.MenuName + "Create/UI Class")]
        static void DoSetAssetBundleName()
        {
            GetWindow<CreateUIClass>();
        }

        protected override void Awake()
        {
            base.Awake();
            mTemplateName = "UiClassTemplate.txt";
        }
    }
}