using System;
using System.IO;
using ExifLib;
using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames
{
    public static class TextureExtensions
    {
        /// <summary>
        /// If you want to be able to read pixels, then markNonReadable = false
        /// </summary>
        /// <param name="data"></param>
        /// <param name="markNonReadable"></param>
        /// <returns></returns>
        public static Texture2D CreateTexture2D(this byte[] data, bool markNonReadable)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.name += "Dynamically-loaded-bytes";
            texture.LoadImage(data, markNonReadable);
            texture.Apply();
            Debug.Log("Dynamically-loaded-bytes");
            return texture;
        }

        /// <summary>
        /// Make sure to call this function after a texture is loaded from ResourceManager
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="markNonReadable"></param>
        /// <returns></returns>
        public static Texture2D CreateTextureFromAssetPath(string assetPath, bool markNonReadable)
        {
            byte[] data = ResourceManager.GetBytes(assetPath);
            if (data != null)
                return data.CreateTexture2D(markNonReadable);
            return null;
        }

        /// <summary>
        /// Creates a texture from the given file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="markNonReadable"></param>
        /// <returns></returns>
        public static Texture2D CreateTextureFromFilePath(string filePath, bool markNonReadable)
        {
            byte[] data = File.ReadAllBytes(filePath);
            return data.CreateTexture2D(markNonReadable);
        }

        /// <summary>
        /// Creates a new texture with the given width & height from the source texture
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <param name="deleteSourceTexture"></param>
        /// <returns></returns>
        public static Texture2D ResizeTexture(this Texture2D source, int targetWidth, int targetHeight, bool deleteSourceTexture, bool flipImage)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            result.name = "Dynamically-loaded-resized";
            Debug.Log("Dynamically-loaded-resized");

            Color[] rpixels = result.GetPixels(0);
            float incX = ((float)1 / source.width) * ((float)source.width / targetWidth);
            float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
            if (flipImage)
            {
                Debug.Log("ResizeTexture 1");
                int i = 0;
                for (int px = rpixels.Length - 1; px >= 0; px--)
                {
                    rpixels[i] = source.GetPixelBilinear(incX * ((float)px % targetWidth),
                                      incY * ((float)Mathf.Floor(px / targetWidth)));
                    ++i;
                }
            }
            else
            {
                Debug.Log("ResizeTexture 2");

                for (int px = 0; px < rpixels.Length; px++)
                {
                    rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth),
                                      incY * ((float)Mathf.Floor(px / targetWidth)));
                }
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            if (deleteSourceTexture)
                UnityEngine.Object.DestroyImmediate(source, true);
            return result;
        }

        /// <summary>
        /// Creates a new texture with the given width percentage & height percentage from the source texture
        /// width & height must be between 0 & 1.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="deleteSourceTexture"></param>
        /// <returns></returns>
        public static Texture2D ResizeTexture(this Texture2D source, float width, float height, bool deleteSourceTexture)
        {
            int targetWidth = (int)(source.width * width);
            int targetHeight = (int)(source.height * height);
            Debug.Log($"Source: W: {source.width}, H: {source.height}, Target: W:{targetWidth}, H:{targetHeight}");
            return ResizeTexture(source, targetWidth, targetHeight, deleteSourceTexture);
        }

        /// <summary>
        /// Loads an image file from the path specified in filePath & resizes it to given width & height
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="markNonReadable"></param>
        /// <param name="deleteSourceTexture"></param>
        /// <returns></returns>
        public static Texture2D CreateTextureFromFilePath(string filePath, int width, int height, bool markNonReadable, bool deleteSourceTexture)
        {
            byte[] data = File.ReadAllBytes(filePath);

            if (data != null)
            {
                Texture2D texture = data.CreateTexture2D(markNonReadable);
                Texture2D resizedTexure = ResizeTexture(texture, width, height, deleteSourceTexture);
                UnityEngine.Object.DestroyImmediate(texture, true);
                return resizedTexure;
            }
            return null;
        }

        /// <summary>
        /// Loads an image file from the path specified in filePath & resizes it to given width & height
        /// widthPercentage & heightPercentage must be between 0 & 1.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="widthPercentage"></param>
        /// <param name="heightPercentage"></param>
        /// <param name="markNonReadable"></param>
        /// <param name="deleteSourceTexture"></param>
        /// <returns></returns>
        public static Texture2D CreateTextureFromFilePath(string filePath, float widthPercentage, float heightPercentage, bool markNonReadable, bool deleteSourceTexture)
        {
            if (widthPercentage < 0f || widthPercentage > 1f)
                new ArgumentException("widthPercentage must be between 0 & 1");
            if (heightPercentage < 0f || heightPercentage > 1f)
                new ArgumentException("heightPercentage must be between 0 & 1");

            byte[] data = File.ReadAllBytes(filePath);

            if (data != null)
            {
                Texture2D texture = data.CreateTexture2D(markNonReadable);
                Texture2D resizedTexure = texture.ResizeTexture(widthPercentage, heightPercentage, deleteSourceTexture);
                UnityEngine.Object.DestroyImmediate(texture, true);
                return resizedTexure;
            }
            return null;
        }

        /// <summary>
        /// Rotatte the texture
        /// </summary>
        /// <param name="originalTexture"></param>
        /// <param name="clockwise"></param>
        /// <param name="deleteSourceTexture"></param>
        /// <returns></returns>
        public static Texture2D Rotate(this Texture2D originalTexture, bool clockwise, bool deleteSourceTexture)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            originalTexture.name += "source for rotation";
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            if (deleteSourceTexture)
                UnityEngine.Object.DestroyImmediate(originalTexture, true);
            return rotatedTexture;
        }

        /// <summary>
        /// Rotates the image to the appropriate orientation
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        public static Texture2D CorrectRotation(this Texture2D texture, ExifOrientation orientation)
        {
            Texture2D rotatedTexture = texture;

            switch (orientation)
            {
                case ExifOrientation.TopRight: // Rotate clockwise 90 degrees
                    rotatedTexture = texture.Rotate(true, true);
                    break;
                case ExifOrientation.TopLeft: // Rotate 0 degrees...
                    rotatedTexture = texture;
                    break;
                case ExifOrientation.BottomRight: // Rotate clockwise 180 degrees
                    rotatedTexture = texture.Rotate(true, true);
                    rotatedTexture = rotatedTexture.Rotate(true, true);
                    break;
                case ExifOrientation.BottomLeft: // Rotate clockwise 270 degrees (I think?)...
                    rotatedTexture = texture.Rotate(false, true);
                    break;
                default:
                    Debug.LogError($"error in CorrectRotation, unknown orientation: {orientation}");
                    break;
            }
            return rotatedTexture;
        }

        /// <summary>
        /// Creates a sprite from texture
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Sprite ToSprite(this Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Returns texture size as Vector2
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Vector2 Size(this Texture texture)
        {
            return new Vector2(texture.width, texture.height);
        }

        /// <summary>
        /// Save the given texture to the given file path. File path must be full directory structure with file name & extension.
        /// File extension will be used to determine the type of encoding used
        /// </summary>
        /// <param name="texture">Texture to save</param>
        /// <param name="fullPath">Path to save texture into</param>
        public static bool Save(Texture2D texture, string fullPath)
        {
            try
            {
                string folder = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.WriteAllBytes(fullPath, texture.GetBytes(fullPath));
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.ImportAsset(fullPath);
#endif
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// Returns encoded bytes based on file type.Encoding type is determined by file extension.
        /// </summary>
        /// <param name="texture">Texture to encode</param>
        /// <param name="fileName">file name. Must have extension</param>
        /// <returns></returns>
        public static byte[] GetBytes(this Texture2D texture, string fileName)
        {
            switch (Path.GetExtension(fileName).ToLower())
            {
                case ".png":
                    return texture.EncodeToPNG();
                case ".exr":
                    return texture.EncodeToEXR();
                case ".tga":
                    return texture.EncodeToTGA();
                case ".jpg":
                case ".jpeg":
                    return texture.EncodeToJPG();
                default:
                    Debug.LogError($"Could not identify file extension for : {fileName}");
                    return null;
            }

        }
    }
}
