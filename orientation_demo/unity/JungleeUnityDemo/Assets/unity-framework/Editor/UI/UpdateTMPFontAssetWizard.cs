using TMPro;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public class UpdateTMPFontAssetWizard : ScriptableWizard
    {
        #region SerializeField
        [SerializeField] TMP_FontAsset fontToReplace = null;       //Font Asset to replace
        [SerializeField] TMP_FontAsset newFont = null;             //New Font after replacing
        #endregion

        #region Private functions
        [MenuItem(Utilities.MenuName + "UI/Update Font")]
        static void CreateWizard()
        {
            DisplayWizard<UpdateTMPFontAssetWizard>("Update Font","Update");
        }

        private void OnWizardCreate()
        {
            Object[] arrObj = Selection.objects;
            for(int i =0;i< arrObj.Length;++i)
            {
                Object obj = arrObj[i];
                if (obj is GameObject gObj)
                {
                    TextMeshProUGUI[] textMeshProUGUI = gObj.GetComponentsInChildren<TextMeshProUGUI>(true);
                    if (textMeshProUGUI.Length > 0)
                    {
                        bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(obj);
                        for (int j = 0; j < textMeshProUGUI.Length; ++j)
                        {
                            TextMeshProUGUI text = textMeshProUGUI[j];
                            if (text.font.Equals(fontToReplace))
                            {
                                text.font = newFont;
                                if (isPrefab)
                                {
                                    PrefabUtility.SavePrefabAsset(gObj);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
