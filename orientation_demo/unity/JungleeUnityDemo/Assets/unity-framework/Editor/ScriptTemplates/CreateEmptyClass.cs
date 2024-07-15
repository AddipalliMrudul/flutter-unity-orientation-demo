using UnityEditor;

namespace XcelerateGames.Editor.UI
{
    public class CreateEmptyClass : CreateTemplateBase
    {
        [MenuItem("Assets/" + Utilities.MenuName + "Create/Empty Class")]
        static void DoSetAssetBundleName()
        {
            GetWindow<CreateEmptyClass>();
        }

        protected override void Awake()
        {
            base.Awake();
            mTemplateName = "EmptyClassTemplate.txt";
        }
    }
}