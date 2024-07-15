using UnityEditor;

namespace XcelerateGames.Editor.UI
{
    public class CreateCommand : CreateTemplateBase
    {
        [MenuItem("Assets/" + Utilities.MenuName + "Create/Command")]
        static void DoSetAssetBundleName()
        {
            GetWindow<CreateCommand>();
        }

        protected override void Awake()
        {
            base.Awake();
            mTemplateName = "CommandTemplate.txt";
        }
    }
}