using System;

namespace JungleeGames.Editor
{
    public class AssetInfo : IComparable
    {
        public string _Name = null;
        public string _Hash = null;
        public long _Size = 0;
        //Is the asset shipped with app or downloaded on demand
        public bool _BundledInApp = false;
        public bool _HasDownloadableDependencies = false;
        public string _ModuleName = null;

        public AssetInfo(string name, long size, bool bundledInApp, string hash, string moduleName)
        {
            _Name = name;
            _Hash = hash;
            _Size = size;
            _BundledInApp = bundledInApp;
            _ModuleName = moduleName;
        }

        public int CompareTo(object obj)
        {
            AssetInfo ai = obj as AssetInfo;
            return ai._Size.CompareTo(_Size);
        }
    }
}
