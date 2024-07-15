using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XcelerateGames.Editor.TextureTools
{
    public class ResizeCanvas : EditorWindow
    {
        public enum Anchor
        {
            TopLeft,
            TopCenter,
            TopRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }
        private Texture2D _Image = null;
        private Texture2D _NewImage = null;
        private Color mCanvasColor = new Color(1f, 1f, 1f, 0f);
        private int mCurrentWidth, mCurrentHeight, mNewWidth, mNewHeight;
        private bool? mWasReadsble = null;
        private UnityEngine.Object mLastObject = null;
        private Anchor mAnchor = Anchor.MiddleCenter;
        private static Texture m0Icon, m45Icon, m90Icon, m135Icon, m180Icon, m225Icon, m270Icon, m315Icon;

        [MenuItem(Utilities.MenuName + "Image Tools/Resize Canvas")]
        static void CreateWizard()
        {
            ResizeCanvas window = GetWindow<ResizeCanvas>();
            window.titleContent.text = "Resize Canvas";
            window.Init();
        }

        private void OnFocus()
        {
            Init();
        }

        void Init()
        {
            CacheButtonIcons();
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
                mNewWidth = GetWidth(mCurrentWidth);
                mNewHeight = GetWidth(mCurrentHeight);
                CreateTexture(Anchor.MiddleCenter);
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

        private int GetWidth(int inWidth)
        {
            return Mathf.NextPowerOfTwo(inWidth);
        }

        private void CacheButtonIcons()
        {
            if (m0Icon == null) m0Icon = (Texture)EditorGUIUtility.Load("ArrowIcons/0Arrow.png");
            if (m45Icon == null) m45Icon = (Texture)EditorGUIUtility.Load("ArrowIcons/45Arrow.png");
            if (m90Icon == null) m90Icon = (Texture)EditorGUIUtility.Load("ArrowIcons/90Arrow.png");
            if (m135Icon == null) m135Icon = (Texture)EditorGUIUtility.Load("ArrowIcons/135Arrow.png");
            if (m180Icon == null) m180Icon = (Texture)EditorGUIUtility.Load("ArrowIcons/180Arrow.png");
            if (m225Icon == null) m225Icon = (Texture)EditorGUIUtility.Load("ArrowIcons/225Arrow.png");
            if (m270Icon == null) m270Icon = (Texture)EditorGUIUtility.Load("ArrowIcons/270Arrow.png");
            if (m315Icon == null) m315Icon = (Texture)EditorGUIUtility.Load("ArrowIcons/315Arrow.png");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            #region Current size
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Width");
            EditorGUILayout.LabelField(mCurrentWidth.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Height");
            EditorGUILayout.LabelField(mCurrentHeight.ToString());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            #endregion

            #region New Size
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Width");
            mNewWidth = EditorGUILayout.IntField(mNewWidth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Height");
            mNewHeight = EditorGUILayout.IntField(mNewHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (GUILayout.Button("Apply", GUILayout.Height(32), GUILayout.Width(64)))
            {
                CreateTexture(mAnchor);
            }

            EditorGUILayout.EndHorizontal();
            #endregion
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Canvas Color");
            mCanvasColor = EditorGUILayout.ColorField(mCanvasColor);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Button(_Image, GUILayout.Height(128), GUILayout.Width(128));
            DrawAnchorPoints();
            GUILayout.Button(_NewImage, GUILayout.Height(128), GUILayout.Width(128));
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

        private void GetAnchoredPosition(Anchor anchor, out int offsetX, out int offsetY)
        {
            if (anchor == Anchor.TopLeft)
            {
                offsetX = 0;
                offsetY = mNewHeight - mCurrentHeight;
            }
            else if (anchor == Anchor.TopCenter)
            {
                offsetX = (mNewWidth / 2) - (mCurrentWidth / 2);
                offsetY = mNewHeight - mCurrentHeight;
            }
            else if (anchor == Anchor.TopRight)
            {
                offsetX = mNewWidth - mCurrentWidth;
                offsetY = mNewHeight - mCurrentHeight;
            }
            else if (anchor == Anchor.MiddleLeft)
            {
                offsetX = 0;
                offsetY = (mNewHeight - mCurrentHeight) / 2;
            }
            else if(anchor == Anchor.MiddleCenter)
            {
                offsetX = (mNewWidth / 2) - (mCurrentWidth / 2);
                offsetY = (mNewHeight - mCurrentHeight) / 2;
            }
            else if (anchor == Anchor.MiddleRight)
            {
                offsetX = mNewWidth - mCurrentWidth;
                offsetY = (mNewHeight - mCurrentHeight) / 2;
            }
            else if (anchor == Anchor.BottomCenter)
            {
                offsetX = (mNewWidth / 2) - (mCurrentWidth / 2);
                offsetY = 0;
            }
            else if (anchor == Anchor.BottomRight)
            {
                offsetX = mNewWidth - mCurrentWidth;
                offsetY = 0;
            }
            else //BottomLeft
            {
                offsetX = offsetY = 0;
            }
        }

        private void DrawAnchorPoints()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(m315Icon, GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.TopLeft);
            }
            if (GUILayout.Button(m0Icon, GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.TopCenter);
            }
            if (GUILayout.Button(m45Icon, GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.TopRight);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(m270Icon, GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.MiddleLeft);
            }
            if (GUILayout.Button("", GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.MiddleCenter);
            }
            if (GUILayout.Button(m90Icon, GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.MiddleRight);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(m225Icon, GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.BottomLeft);
            }
            if (GUILayout.Button(m180Icon, GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.BottomCenter);
            }
            if (GUILayout.Button(m135Icon, GUILayout.Height(32), GUILayout.Width(32)))
            {
                CreateTexture(Anchor.BottomRight);
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

        private void CreateTexture(Anchor anchor)
        {
            mAnchor = anchor;
            if (_NewImage != null)
                DestroyImmediate(_NewImage);
            int offsetX, offsetY;
            GetAnchoredPosition(anchor, out offsetX, out offsetY);
            //Create a new texture to hold combined image
            _NewImage = new Texture2D(mNewWidth, mNewHeight, TextureFormat.ARGB32, false, false);

            for (int x = 0; x < _NewImage.width; x++)
                for (int y = 0; y < _NewImage.height; y++)
                    _NewImage.SetPixel(x, y, mCanvasColor);
            //Debug.Log(offsetX + " " + offsetY);
            for (int x = 0; x < _Image.width; x++)
                for (int y = 0; y < _Image.height; y++)
                    _NewImage.SetPixel(x + offsetX, y + offsetY, _Image.GetPixel(x, y));

            _NewImage.Apply();
        }

        public Texture2D AddWatermark(Texture2D background, Texture2D watermark)
        {
            int startX = 0;
            int startY = background.height - watermark.height;

            for (int x = startX; x < background.width; x++)
            {

                for (int y = startY; y < background.height; y++)
                {
                    Color bgColor = background.GetPixel(x, y);
                    Color wmColor = watermark.GetPixel(x - startX, y - startY);

                    Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

                    background.SetPixel(x, y, final_color);
                }
            }

            background.Apply();
            return background;
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
