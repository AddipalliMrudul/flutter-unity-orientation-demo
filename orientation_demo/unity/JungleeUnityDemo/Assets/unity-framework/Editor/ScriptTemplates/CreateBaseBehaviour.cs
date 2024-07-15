using UnityEditor;

namespace XcelerateGames.Editor.UI
{
    public class CreateBaseBehaviour : CreateTemplateBase
    {
        [MenuItem("Assets/" + Utilities.MenuName + "Create/BaseBehaviour")]
        static void DoSetAssetBundleName()
        {
            GetWindow<CreateBaseBehaviour>();
        }

        protected override void Awake()
        {
            base.Awake();
            mTemplateName = "BaseBehaviourTemplate.txt";
        }
    }
}