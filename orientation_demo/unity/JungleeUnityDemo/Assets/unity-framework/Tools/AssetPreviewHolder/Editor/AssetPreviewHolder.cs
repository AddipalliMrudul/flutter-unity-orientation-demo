using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XcelerateGames.EditorTools
{
    public class AssetPreviewHolder : EditorWindow
    {
        #region Data
        //Private
        private const float kZoomMin = 0.5f;
        private const float kZoomMax = 20.0f;
        private Rect _zoomArea = new Rect(0, 0, 0, 0);
        private float _zoom = 1.0f;
        private static AssetPreviewHolder window;
        private Vector2 _zoomCoordsOrigin = Vector2.zero;
        public Texture2D mock;
        [SerializeField] private string path;
        #endregion//============================================================[ Data ]

        #region Unity       
        public void OnGUI()
        {
            if (window == null) window = (AssetPreviewHolder)GetWindow(typeof(AssetPreviewHolder));
            _zoomArea = new Rect(0, 50, window.position.width, window.position.height);
            HandleEvents();
            try
            {
                DrawZoomArea();
            }
            catch (Exception)
            {
            }

            DrawNonZoomArea();
        }
        #endregion//============================================================[ unity ]

        #region Public
        [MenuItem("XcelerateGames/EditorTools/AssetPreview %&#_p", false)]
        private static void MenuMockHolder()
        {
            ShowWindow();
        }
        public static void ShowWindow()
        {
            window = (AssetPreviewHolder)GetWindow(typeof(AssetPreviewHolder), false, "Asset Preview");
            window.Show();
        }

        #endregion//============================================================[ Public ]

        #region Private
        private void DrawNonZoomArea()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField("©2021 Xcelerate Games",
                    new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
            }
            EditorGUILayout.BeginHorizontal();
            if (mock != null && GUILayout.Button(new GUIContent("#", "Reset"), GUILayout.Width(25))) Reset();
            if (mock == null && GUILayout.Button("Load Asset"))
            {
                var path = EditorUtility.OpenFilePanel("Select your mock", this.path, "png");
                if (path.Length != 0) this.path = path;
            }
            if (mock != null && GUILayout.Button("Unload Asset"))
            {
                mock = null;
                path = string.Empty;
            }
            if (!string.IsNullOrWhiteSpace(path) && mock == null)
            {
                Reset();
                mock = LoadPNG(path);
            }
            if (mock != null && !GameObject.Find(mock.GetInstanceID().ToString()))
            {
                if (!EditorApplication.isPlaying &&
                    GUILayout.Button(new GUIContent("+", "Ads To Scene"), GUILayout.Width(25)))
                {
                    var mockObject = new GameObject(mock.GetInstanceID().ToString());
                    mockObject.AddComponent<Canvas>();
                    mockObject.AddComponent<Image>().sprite = Sprite.Create(mock,
                        new Rect(0, 0, mock.width, mock.height), new Vector2(0, 0), 1);
                    mockObject.GetComponent<RectTransform>().sizeDelta = new Vector2(mock.width, mock.height);
                    mockObject.transform.localScale = Vector3.one;
                }
            }
            else if (mock != null && GameObject.Find(mock.GetInstanceID().ToString()))
            {
                if (GUILayout.Button(new GUIContent("-", "Remove From Scene"), GUILayout.Width(25)))
                    DestroyImmediate(GameObject.Find(mock.GetInstanceID().ToString()));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawZoomArea()
        {
            EditorZoomArea.Begin(_zoom, _zoomArea);
            GUILayout.BeginArea(new Rect(_zoomCoordsOrigin.x, _zoomCoordsOrigin.y, position.width, position.height));
            DrawMock();
            GUILayout.EndArea();
            EditorZoomArea.End();
        }

        private void DrawMock()
        {
            if (mock != null)
                GUI.DrawTexture(new Rect(0, 0, position.width, position.height), mock, ScaleMode.ScaleToFit);
        }

        private void Reset()
        {
            _zoom = 0.75f;
            _zoomCoordsOrigin = Vector2.one;
        }

        private void HandleEvents()
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                var screenCoordsMousePos = Event.current.mousePosition;
                var delta = Event.current.delta;
                var zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
                var zoomDelta = -delta.y / 150.0f;
                var oldZoom = _zoom;
                _zoom += zoomDelta;
                _zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
                _zoomCoordsOrigin += zoomCoordsMousePos - _zoomCoordsOrigin -
                                     oldZoom / _zoom * (zoomCoordsMousePos - _zoomCoordsOrigin);
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                var delta = Event.current.delta;
                delta /= _zoom;
                _zoomCoordsOrigin += delta;
                Event.current.Use();
            }
        }

        private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
        {
            return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;
            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(1, 1);
                tex.LoadImage(fileData);
            }
            return tex;
        }
        #endregion//============================================================[ Private ]
    }

    #region Utils
    public static class RectExtensions
    {
        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            var result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }

        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }

        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
        {
            var result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale.x;
            result.xMax *= scale.x;
            result.yMin *= scale.y;
            result.yMax *= scale.y;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
    }

    public class EditorZoomArea
    {
        private const float kEditorWindowTabHeight = 21.0f;
        private static Matrix4x4 _prevGuiMatrix;

        public static Rect Begin(float zoomScale, Rect screenCoordsArea)
        {
            GUI.EndGroup();
            var clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.TopLeft());
            clippedArea.y += kEditorWindowTabHeight;
            GUI.BeginGroup(clippedArea);
            _prevGuiMatrix = GUI.matrix;
            var translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
            var scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
            GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
            return clippedArea;
        }

        public static void End()
        {
            GUI.matrix = _prevGuiMatrix;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0.0f, kEditorWindowTabHeight, Screen.width, Screen.height));
        }
    }
    #endregion//============================================================[ Utils ]
}