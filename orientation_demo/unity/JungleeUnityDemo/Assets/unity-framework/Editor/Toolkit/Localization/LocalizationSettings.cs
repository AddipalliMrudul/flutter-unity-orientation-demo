using UnityEngine;
using UnityEditor;

namespace XcelerateGames.Editor.Locale
{
	[CreateAssetMenu (fileName = "LocalizationSettings", menuName = Utilities.MenuName + "LocalizationSettings")]
	public class LocalizationSettings : ScriptableObject
	{
		static LocalizationSettings mInstance;

		public static LocalizationSettings	pInstance {
			get {
				if (mInstance == null)
					mInstance = EditorGUIUtility.Load ("LocalizationSettings.asset") as LocalizationSettings;
				return mInstance;
			}
		}

		public string GSpreadsheetSource;
		public string GSheetName = "Localization";

		public string[] Targets;
	}
}
