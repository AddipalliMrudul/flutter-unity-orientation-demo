using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor.TextureTools
{
    public class InvertAlpha : EditorWindow
    {
        private Texture2D _Image = null;
        private Texture2D _NewImage = null;
        private Color mCanvasColor = new Color(1f, 1f, 1f, 0f);
        private int mCurrentWidth, mCurrentHeight, mNewWidth, mNewHeight;
        private bool? mWasReadsble = null;
        private UnityEngine.Object mLastObject = null;

        [MenuItem(Utilities.MenuName + "Image Tools/Invert Alpha")]
        static void CreateWizard()
        {
            InvertAlpha window = GetWindow<InvertAlpha>();
            window.titleContent.text = "Invert Alpha";
            window.Init();
        }

        private void OnFocus()
        {
            Init();
        }

        void Init()
        {
            if (Selection.activeObject != null)
            {
                _Image = Selection.activeObject as Texture2D;
                if (_Image == null)
                    return;
                if(!mWasReadsble.HasValue || mLastObject != Selection.activeObject)
                {
                    UndoChanges();
                    TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject)) as TextureImporter;
                    if (textureImporter != null)
                    {
                        mWasReadsble = textureImporter.isReadable;
                        textureImporter.isReadable = true;
                        textureImporter.SaveAndReimport();
                    }
                }
                mLastObject = Selection.activeObject;
                mCurrentWidth = _Image.width;
                mCurrentHeight = _Image.height;
            }
        }

        private void UndoChanges()
        {
            if (mLastObject == null)
                return;
            if(mWasReadsble.HasValue && !mWasReadsble.Value)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mLastObject)) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.isReadable = false;
                    textureImporter.SaveAndReimport();
                }
            }
        }

        private void OnDisable()
        {
            UndoChanges();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.Height(32), GUILayout.Width(64)))
            {
                CreateTexture();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.Height(32), GUILayout.Width(128)))
            {
                Save();
            }
            if (GUILayout.Button("Save As", GUILayout.Height(32), GUILayout.Width(128)))
            {
                SaveAs();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void Save()
        {
            bool overrite = EditorUtility.DisplayDialog("Warning!","Are you sure you want to overwrite?", "Yes", "No");
            if(overrite)
            {
                string filePath = AssetDatabase.GetAssetPath(_Image);
                WriteToFile(Path.GetFileName(filePath), _NewImage.EncodeToPNG(), Path.GetDirectoryName(filePath));
                AssetDatabase.ImportAsset(filePath);
            }
        }

        private void SaveAs()
        {
            string filePath = AssetDatabase.GetAssetPath(_Image);

            string path = EditorUtility.SaveFilePanel("Choose Save Path", Path.GetDirectoryName(filePath), Path.GetFileName(filePath), "png");
            if(!string.IsNullOrEmpty(path))
            {
                WriteToFile(Path.GetFileName(path), _NewImage.EncodeToPNG(), Path.GetDirectoryName(path));
                string importPath = path.Replace(Application.dataPath, "Assets");
                AssetDatabase.ImportAsset(importPath);
            }
        }

        private void CreateTexture()
        {
            if (_NewImage != null)
                DestroyImmediate(_NewImage);
            //Create a new texture to hold combined image
            _NewImage = new Texture2D(mCurrentWidth, mCurrentHeight, TextureFormat.ARGB32, false, false);

            for (int x = 0; x < _Image.width; x++)
            {
                for (int y = 0; y < _Image.height; y++)
                {
                    Color color = _Image.GetPixel(x, y);
                    color.a = 1 - color.a;
                    _NewImage.SetPixel(x, y, color);
                }
            }

            _NewImage.Apply();
        }

        public static void WriteToFile(string fileName, byte[] data, string fullPath)
        {
            try
            {
                fullPath += "/" + fileName;
                string folder = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.WriteAllBytes(fullPath, data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
