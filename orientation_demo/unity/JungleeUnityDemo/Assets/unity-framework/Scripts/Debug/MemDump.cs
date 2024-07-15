using UnityEngine;
using System.Collections.Generic;
using XcelerateGames.AssetLoading;

namespace XcelerateGames.Debugging
{
    public class MemDump
    {
        public class MemDumpData
        {
            public string _Name = "";
            public string _Info = "";
            public long _Size = 0;
            public string _RefererringObjects = null;
        }
        private static bool mSortBySize = true;
        private static System.IO.StreamWriter fOut = null;
        private static List<MemDumpData> mTextures = null;
        private static List<MemDumpData> mMeshes = null;
        //	private static List<MemDumpData> mAssetBundles = null;
        private static List<MemDumpData> mAudioClip = null;
        private static List<MemDumpData> mMaterials = null;
        private static List<MemDumpData> mAnimationClips = null;
        //	private static List<MemDumpData> mObjects = null;


        public static string WriteToFile(string identifier = "")
        {
#if NETFX_CORE
        return "";
#else
            mTextures = new List<MemDumpData>();
            mMeshes = new List<MemDumpData>();
            //		mAssetBundles = new List<MemDumpData>();
            mAudioClip = new List<MemDumpData>();
            mMaterials = new List<MemDumpData>();
            mAnimationClips = new List<MemDumpData>();
            //		mObjects = new List<MemDumpData>();

            long totalMem = 0;
#if UNITY_EDITOR_OSX
		string fileName = "Assets/" + "MemDump_" + ResourceManager.pCurrentScene + identifier + ".txt"; 
#elif UNITY_EDITOR_WIN
            string fileName = "MemDump_" + ResourceManager.pCurrentScene + identifier + ".txt";
#else
            string fileName = Application.persistentDataPath + "/" + "MemDump_" + ResourceManager.pCurrentScene + identifier + ".txt"; 
#endif

            fOut = new System.IO.StreamWriter(fileName);
            fOut.WriteLine("Platform : " + Application.platform);
            fOut.WriteLine("Time : " + System.DateTime.Now);
            fOut.WriteLine("");
            fOut.WriteLine("---------------------------------------------------------------");

            long mem = 0;
            //		mem = Enlist<AssetBundle>(mAssetBundles);
            //		fOut.WriteLine("Total Memory of all AssetBundles : " + GetFormattedSize(mem));
            //		totalMem += mem;

            mem = Enlist<Texture>(mTextures);
            fOut.WriteLine("Total Memory of all Textures : " + GetFormattedSize(mem));
            totalMem += mem;

            mem = Enlist<AudioClip>(mAudioClip);
            fOut.WriteLine("Total Memory of all AudioClip : " + GetFormattedSize(mem));
            totalMem += mem;

            mem = Enlist<Mesh>(mMeshes);
            fOut.WriteLine("Total Memory of all Meshes : " + GetFormattedSize(mem));
            totalMem += mem;

            mem = Enlist<Material>(mMaterials);
            fOut.WriteLine("Total Memory of all Materials : " + GetFormattedSize(mem));
            totalMem += mem;

            mem = Enlist<AnimationClip>(mAnimationClips);
            fOut.WriteLine("Total Memory of all AnimationClips : " + GetFormattedSize(mem));
            totalMem += mem;

            //		mem = Enlist<Object>(mObjects);
            //		fOut.WriteLine("Total Memory of all Objects : " + GetFormattedSize(mem));
            //		totalMem += mem;

            fOut.WriteLine("---------------------------------------------------------------");
            fOut.WriteLine("Used Heap Size : " + GetFormattedSize(UnityEngine.Profiling.Profiler.usedHeapSizeLong));
            fOut.WriteLine("Mono Used Size : " + GetFormattedSize(UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong()));
            fOut.WriteLine("Total Reserved Size : " + GetFormattedSize(UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong()));
            fOut.WriteLine("Total Unused Size : " + GetFormattedSize(UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong()));

            fOut.WriteLine("---------------------------------------------------------------");
            fOut.WriteLine("Total Memory of all Assets in Memory : " + GetFormattedSize(totalMem));

            if (mSortBySize)
            {
                mTextures.Sort(SortBySize);
                mMeshes.Sort(SortBySize);
                //		mAssetBundles.Sort(SortBySize);
                mAudioClip.Sort(SortBySize);
                mMaterials.Sort(SortBySize);
                mAnimationClips.Sort(SortBySize);
                //		mObjects.Sort(SortBySize);
            }
            else
            {
                mTextures.Sort(SortByName);
                mMeshes.Sort(SortByName);
                //		mAssetBundles.Sort(SortByName);
                mAudioClip.Sort(SortByName);
                mMaterials.Sort(SortByName);
                mAnimationClips.Sort(SortByName);
                //		mObjects.Sort(SortByName);
            }

            //		Dump("AssetBundles", mAssetBundles);
            Dump("Textures", mTextures);
            Dump("Meshes", mMeshes);
            Dump("AudioClip", mAudioClip);
            Dump("Materials", mMaterials);
            Dump("AnimationClips", mAnimationClips);
            //Dump("Objects", mObjects);

            DumpResources();

            fOut.Close();

            UnityEngine.Debug.Log("Memory usage dumped to : " + fileName);

            return fileName;
#endif
        }

        static int SortBySize(MemDumpData data1, MemDumpData data2)
        {
            return data2._Size.CompareTo(data1._Size);
        }

        static int SortByName(MemDumpData data1, MemDumpData data2)
        {
            return data2._Name.CompareTo(data1._Name);
        }

        static string GetFormattedSize(double inSize)
        {
            string format = " Bytes";
            if (inSize >= 1024)
            {
                inSize /= 1024;
                format = " KB";

                if (inSize >= 1024)
                {
                    inSize /= 1024;
                    format = " MB";
                }
            }

            return (inSize.ToString("0.00") + format);
        }

        static long Enlist<TYPE>(List<MemDumpData> inData) where TYPE : class
        {
            TYPE[] objectList = Resources.FindObjectsOfTypeAll(typeof(TYPE)) as TYPE[];
            long memUsed = 0;
            foreach (TYPE obj in objectList)
            {
                string info = "";
                long memUsedByAsset = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(obj as Object);
                if (obj is Texture)
                {
                    Texture tex = obj as Texture;
                    if (tex != null)
                    {
                        info = tex.width + "x" + tex.height;
                        memUsedByAsset = AssetUtilities.CalculateTextureSizeBytes(obj as Texture);
                        Texture2D tex2D = obj as Texture2D;
                        if (tex2D != null)
                            info += " (" + tex2D.format.ToString() + ")";
                    }
                }
                //else if(obj is Mesh)
                //{
                //	Mesh msh = obj as Mesh;
                //	if(msh != null)
                //		info = "Vertices " + msh.vertexCount + ", Triangles " + msh.triangles.Length;
                //}
                else if (obj is Material)
                {
                    Material mat = obj as Material;
                    if (mat != null)
                    {
                        info = mat.shader.name;
                        if (mat.mainTexture != null)
                            info += ", Texture : " + mat.mainTexture.name;
                    }
                }

                memUsed += memUsedByAsset;
                MemDumpData mdd = new MemDumpData();
                mdd._Size = memUsedByAsset;
                mdd._Name = (obj as Object).name;
                mdd._Info = info;
                inData.Add(mdd);
            }

            return memUsed;
        }

        static void Dump(string tag, List<MemDumpData> inData)
        {
            long totalSize = 0;
            fOut.WriteLine("");
            fOut.WriteLine("");
            fOut.WriteLine("======================================================");
            fOut.WriteLine("======================================================");
            fOut.WriteLine("Total " + tag + " : " + inData.Count);
            foreach (MemDumpData mmd in inData)
            {
                fOut.WriteLine("{0,-35}{1,25} : {2,20:F}", mmd._Name, mmd._Info, Utilities.FormatBytes(mmd._Size));
                totalSize += mmd._Size;
            }
            fOut.WriteLine("------------------------------------------------------");
            fOut.WriteLine("Total " + tag + " Memory : " + " : " + Utilities.FormatBytes(totalSize));
        }

        static void DumpResources()
        {
            ResourceManager.Dump(fOut, true);
        }
    }
}