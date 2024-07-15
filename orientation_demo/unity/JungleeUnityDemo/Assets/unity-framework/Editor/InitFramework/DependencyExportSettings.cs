using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XcelerateGames.Editor.AssetBundles;

namespace XcelerateGames.Editor.Build
{
	/// <summary>
	/// To maintain list of assets that needs to be added to export package.
	/// </summary>
	[CreateAssetMenu(fileName = "DependencyExportSettings", menuName = Utilities.MenuName + "DependencyExportSettings")]
	public class DependencyExportSettings : ScriptableObject
	{
		public List<Object> _Assets;    /**<List of all assets to be exported */
		public string _PackageName = "Export.unitypackage"; /**<Name of the package to be exported */
		[HideInInspector] public string mPackagePath = null; /**<Path of the package to exported to. Will be used to save last used name & path*/

		public const string AssetName = "DependencyExportSettings"; /**<Menu name for the asset */

		private static DependencyExportSettings mInstance;
		private static DependencyExportSettings pInstance
		{
			get
			{
				if (mInstance == null)
					mInstance = EditorGUIUtility.Load($"{AssetName}.asset") as DependencyExportSettings;

				return mInstance;
			}
		}

		/// <summary>
		/// Get the list of all assets from the assets selected.
		/// </summary>
		/// <returns></returns>
		static List<string> GetAssetList()
		{
			List<string> assets = new List<string>();

			foreach (Object obj in pInstance._Assets)
			{
				string assetPath = AssetDatabase.GetAssetPath(obj);
				if (FileUtilities.IsDirectory(assetPath))
				{
					List<string> files = EditorUtilities.GetFiles(assetPath, "*.*");
					//Remove meta files
					files.RemoveAll(e => e.EndsWith(".meta"));
					assets.AddRange(files);
				}
				else
					assets.Add(assetPath);
			}

			return assets;
		}

		/// <summary>
		/// Export the package. Package is exported under Assets/Binary folder
		/// </summary>
		public void ExportPackage()
		{
			string mappingsFileName = AssetBundleMappings.GetAssetBundleMappings();
			List<string> assetsToExport = GetAssetList();
			assetsToExport.Add(mappingsFileName);
			//If you are using sort layer than tag manage exporting is needed to resolve <unknownlayer> issue
			assetsToExport.Add("ProjectSettings/TagManager.asset");
			if (!System.IO.Directory.Exists("Binary"))
			{
				System.IO.Directory.CreateDirectory("Binary");
			}
			mPackagePath = EditorUtility.SaveFilePanel("Select Folder", "Binary/", _PackageName, "unitypackage");
			if (!mPackagePath.IsNullOrEmpty())
			{
				Debug.Log($"Saving package to {mPackagePath}");

				AssetDatabase.ExportPackage(assetsToExport.ToArray(), mPackagePath, UnityEditor.ExportPackageOptions.Recurse
					| UnityEditor.ExportPackageOptions.IncludeDependencies);
				EditorUtility.RevealInFinder(mPackagePath);
				_PackageName = System.IO.Path.GetFileName(mPackagePath);
			}
		}

		public static void ExportUnityPackage()
		{
			if (!System.IO.Directory.Exists("Binary"))
			{
				System.IO.Directory.CreateDirectory("Binary");
			}
			if (pInstance._PackageName.IndexOf(".unitypackage") == -1)
				pInstance._PackageName = pInstance._PackageName + ".unitypackage";
			pInstance.mPackagePath = System.IO.Path.Combine("Binary", pInstance._PackageName);
			Debug.Log($"Saving package to {pInstance.mPackagePath}");
			AssetDatabase.ExportPackage(GetAssetList().ToArray(), pInstance.mPackagePath, UnityEditor.ExportPackageOptions.Recurse
				| UnityEditor.ExportPackageOptions.IncludeDependencies);
		}
	}

	/// <summary>
	/// Custom inspector for the exporter
	/// </summary>
	[CustomEditor(typeof(DependencyExportSettings))]
	public class DependencyExportSettingsEditor : UnityEditor.Editor
	{
		private const string PackagePathKey = "ExportPkgPath";  /**<Key to be used to save & load last used package name */
		private DependencyExportSettings mInstance = null;      /**<Instance of DependencyExportSettings */

		/// <summary>
		/// Draw the inspector UI
		/// </summary>
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUILayout.Button("Export", GUILayout.Height(40)))
			{
				mInstance.ExportPackage();
			}
		}

		/// <summary>
		/// Cache the reference & load the last used asset name from editor prefs
		/// </summary>
		private void OnEnable()
		{
			mInstance = (DependencyExportSettings)target;
			mInstance.mPackagePath = XGEditorPrefs.GetString(PackagePathKey, mInstance.mPackagePath);
			mInstance._PackageName = System.IO.Path.GetFileName(mInstance.mPackagePath);
			CheckAssetName();
		}

		/// <summary>
		/// Save the name of the asset to editor prefs
		/// </summary>
		private void OnDisable()
		{
			XGEditorPrefs.SetString(PackagePathKey, mInstance.mPackagePath);
		}

		private void CheckAssetName()
		{
			if (!name.Equals(DependencyExportSettings.AssetName))
			{
				AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(target), DependencyExportSettings.AssetName);
			}
		}
	}
}
