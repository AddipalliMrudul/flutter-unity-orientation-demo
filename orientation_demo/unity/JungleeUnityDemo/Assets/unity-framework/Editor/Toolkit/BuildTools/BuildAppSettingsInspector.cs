using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor.Build
{
	/// <summary>
	/// Custom inspector for the exporter
	/// </summary>
	[CustomEditor(typeof(BuildAppSettings))]
	public class BuildAppSettingsInspector : UnityEditor.Editor
	{
		private BuildAppSettings mInstance = null;      /**<Instance of BuildAppSettings */

		/// <summary>
		/// Draw the inspector UI
		/// </summary>
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			//GUIColor.Push(Color.red);
			//GUILayout.Label("Errro");
			//GUIColor.Pop();
		}

		/// <summary>
		/// Cache the reference & load the last used asset name from editor prefs
		/// </summary>
		private void OnEnable()
		{
			mInstance = (BuildAppSettings)target;
			if(mInstance._BuildStepsDoc != null)
            {
				string assetPath = AssetDatabase.GetAssetPath(mInstance._BuildStepsDoc);
				if(mInstance._BuildStepsDoc.name.Contains(" "))
                {
					string result = AssetDatabase.RenameAsset(assetPath, mInstance._BuildStepsDoc.name.Replace(" ", ""));
					if (result.IsNullOrEmpty())
						Debug.LogWarning("Renamed build step doc by removing spaces");
					else
						Debug.LogError($"Failed to rename build step doc: {result}");
                }
            }
		}
	}
}
