using UnityEditor;
using UnityEngine;
using XcelerateGames.Editor.Inspectors;

namespace XcelerateGames.Editor.TextureTools
{
    /// <summary>
    /// Script to change color of all selected textures.
    /// for texture operation to work, the texture format will be set to RGBA32.
    /// </summary>
    public class ChangeColor : EditorWindow
    {
        private Color mColor = new Color(1f, 1f, 1f, 1f);
        private const string mKey = "ChanClrLstVal";

        [MenuItem(Utilities.MenuName + "Image Tools/Change Color")]
        static void CreateWizard()
        {
            ChangeColor window = GetWindow<ChangeColor>();
            window.titleContent.text = "Change Color";
            window.mColor = XGEditorPrefs.GetColor(mKey, Color.white);
        }

        bool ApplySettings(Texture2D texture)
        {
            bool wasReadable = false;
            if (texture != null)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                if (textureImporter != null)
                {
                    wasReadable = textureImporter.isReadable;
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
                else
                    Debug.LogError($"Failed to get TextureImporter for {AssetDatabase.GetAssetPath(texture)}");

                TextureImporterPlatformSettings ps = textureImporter.GetPlatformTextureSettings(EditorUtilities.GetCurrentPlatform());
                if (ps != null)
                {
                    ps.overridden = true;
                    ps.format = TextureImporterFormat.RGBA32;
                    textureImporter.SetPlatformTextureSettings(ps);
                    textureImporter.SaveAndReimport();
                }
                else
                    Debug.LogError("sdsdfsdf");
            }
            return wasReadable;
        }

        private void UndoChanges(UnityEngine.Object obj)
        {
            if (obj == null)
                return;
            TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj)) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.isReadable = false;
                textureImporter.SaveAndReimport();
            }
        }

        private void OnDisable()
        {
            XGEditorPrefs.SetColor(mKey, mColor);
        }

        void OnGUI()
        {
            mColor = EditorGUITools.DrawColor("New Color", mColor);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.Height(32), GUILayout.Width(64)))
            {
                foreach (UnityEngine.Object obj in Selection.objects)
                {
                    Texture2D texture = obj as Texture2D;
                    if (texture != null)
                    {
                        bool wasSetToReadable = ApplySettings(texture);
                        ApplyColor(texture);
                        if (wasSetToReadable)
                            UndoChanges(texture);
                    }
                    else
                        XDebug.LogError($"Could not load {AssetDatabase.GetAssetPath(obj)} as Texture2D");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void ApplyColor(Texture2D texture)
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color color = texture.GetPixel(x, y);
                    color.r = mColor.r;
                    color.g = mColor.g;
                    color.b = mColor.b;
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            TextureExtensions.Save(texture, AssetDatabase.GetAssetPath(texture));
        }
    }
}
