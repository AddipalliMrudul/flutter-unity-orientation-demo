using UnityEditor;

namespace XcelerateGames.Editor.UI
{
    public class CreateUIItemClass : CreateTemplateBase
    {
        [MenuItem("Assets/" + Utilities.MenuName + "Create/UI Item")]
        static void DoCreateUIItemClass()
        {
            GetWindow<CreateUIItemClass>();
        }

        protected override void Awake()
        {
            base.Awake();
            mTemplateName = "UiItemTemplate.txt";
        }
    }
}