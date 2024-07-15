using UnityEngine;

namespace XcelerateGames
{
    public static class AssetUtilities
    {
        //Code copied from ResourceChecker : https://github.com/handcircus/Unity-Resource-Checker
        public static int CalculateTextureSizeBytes(Texture tTexture)
        {
            int tWidth = tTexture.width;
            int tHeight = tTexture.height;
            if (tTexture is Texture2D)
            {
                Texture2D tTex2D = tTexture as Texture2D;
                int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
                int mipMapCount = tTex2D.mipmapCount;
                int mipLevel = 1;
                int tSize = 0;
                while (mipLevel <= mipMapCount)
                {
                    tSize += tWidth * tHeight * bitsPerPixel / 8;
                    tWidth = tWidth / 2;
                    tHeight = tHeight / 2;
                    mipLevel++;
                }
                return tSize;
            }
            if (tTexture is Texture2DArray)
            {
                Texture2DArray tTex2D = tTexture as Texture2DArray;
                int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
                int mipMapCount = 10;
                int mipLevel = 1;
                int tSize = 0;
                while (mipLevel <= mipMapCount)
                {
                    tSize += tWidth * tHeight * bitsPerPixel / 8;
                    tWidth = tWidth / 2;
                    tHeight = tHeight / 2;
                    mipLevel++;
                }
                return tSize * ((Texture2DArray)tTex2D).depth;
            }
            if (tTexture is Cubemap)
            {
                Cubemap tCubemap = tTexture as Cubemap;
                int bitsPerPixel = GetBitsPerPixel(tCubemap.format);
                return tWidth * tHeight * 6 * bitsPerPixel / 8;
            }
            return 0;
        }

        public static int GetBitsPerPixel(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8: //	 Alpha-only texture format.
                    return 8;
                case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
                    return 16;
                case TextureFormat.RGBA4444: //	 A 16 bits/pixel texture format.
                    return 16;
                case TextureFormat.RGB24:   // A color texture format.
                    return 24;
                case TextureFormat.RGBA32:  //Color with an alpha channel texture format.
                    return 32;
                case TextureFormat.ARGB32:  //Color with an alpha channel texture format.
                    return 32;
                case TextureFormat.RGB565:  //	 A 16 bit color texture format.
                    return 16;
                case TextureFormat.DXT1:    // Compressed color texture format.
                    return 4;
                case TextureFormat.DXT5:    // Compressed color with alpha channel texture format.
                    return 8;
                case TextureFormat.PVRTC_RGB2://	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
                    return 2;
                case TextureFormat.PVRTC_RGBA2://	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
                    return 2;
                case TextureFormat.PVRTC_RGB4://	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
                    return 4;
                case TextureFormat.PVRTC_RGBA4://	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
                    return 4;
                case TextureFormat.ETC_RGB4://	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
                    return 4;
                case TextureFormat.ETC2_RGBA8://	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
                    return 8;
                case TextureFormat.BGRA32://	 Format returned by iPhone camera
                    return 32;
            }
            return 0;
        }

        public static long GetTextureMemoryInt(Texture inTex)
        {
            if (inTex == null)
                return -1;
            return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(inTex);
        }

        public static string GetTextureMemory(Texture2D inTex)
        {
            if (inTex == null)
                return "NA";

            return Utilities.FormatBytes((int)(inTex.width * inTex.height * GetBpp(inTex.format)));
        }

        public static float GetBpp(TextureFormat texFormat)
        {
            switch (texFormat)
            {
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                    return 0.25f;

                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.DXT1:
                    return 0.5f;

                case TextureFormat.Alpha8:
                case TextureFormat.DXT5:
                    return 1f;

                //      case TextureFormat.RGB16:
                //      case TextureFormat.RGBA16:
                //          return 2f;

                case TextureFormat.RGB24:
                    return 3f;

                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.ETC_RGB4:
                    return 4f;

                case TextureFormat.ETC2_RGBA8:
                    return 8f;
            }

            //      Debug.LogWarning("Format not included : " + texFormat);
            return 1f;
        }
    }
}

