using UnityEditor;

namespace XcelerateGames.Editor.Build
{
    /// <summary>
    /// Editor tool to enable/disable features of framework of JungleeGames
    /// </summary>
    public class FrameworkFeaturesJunglee : FrameworkFeatures
    {
        [MenuItem(Utilities.MenuName + "Framework Features", false, 3)]
        public new static void OpenFrameworkFeaturesWindow()
        {
            FrameworkFeaturesJunglee instance = GetWindow<FrameworkFeaturesJunglee>(true, "Framework Features", true);
            instance.GetCurrentSettings();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            mDefines.Add("USE_FLUTTER_TO_MAIL_LOGS", false);
            mDefines.Add("AUTOMATION_ENABLED", true);
            mDefines.Add("USING_FLUTTER", false);

            GetCurrentSettings();
        }
    }
}