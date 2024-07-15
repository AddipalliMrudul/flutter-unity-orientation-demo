using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace XcelerateGames.Editor
{
    /// <summary>
    /// This class handles all compiler flags to be added/removed in the csc file
    /// </summary>
    public static class CSCFileHandler
    {
        /// <summary>
        /// List of all features in the csc file
        /// </summary>
        private static List<string> mFeatures = null;

        /// <summary>
        /// File name & path of csc file
        /// </summary>
        private const string mFileName = "Assets/csc.rsp";

        /// <summary>
        /// Read the current state of csc file
        /// </summary>
        /// <returns></returns>
        public static bool Read()
        {
            if (File.Exists(mFileName))
                mFeatures = new List<string>(File.ReadAllLines(mFileName));
            else
                mFeatures = new List<string>();
            return true;
        }

        /// <summary>
        /// Write all flags back to csc file & import it to make Unity compile the code
        /// </summary>
        /// <returns></returns>
        public static bool Commit()
        {
            File.WriteAllLines(mFileName, mFeatures);
            AssetDatabase.ImportAsset(mFileName);

            return true;
        }

        /// <summary>
        /// Add a feature to the list
        /// </summary>
        /// <param name="feature">feature flag</param>
        /// <param name="isDefine">is it a #deefine?</param>
        public static void Add(string feature, bool isDefine)
        {
            if (!Contains(feature))
            {
                if(isDefine)
                    mFeatures.Add($"-define:{feature}");
                else
                    mFeatures.Add($"-{feature}+");
            }
        }

        /// <summary>
        /// Remove a feature from the list
        /// </summary>
        /// <param name="feature">feature flag</param>
        public static void Remove(string feature)
        {
            mFeatures.RemoveAll(e => e.Contains(feature));
        }

        /// <summary>
        /// Check if the feature is already in the list
        /// </summary>
        /// <param name="feature">feature flag</param>
        /// <returns></returns>
        public static bool Contains(string feature)
        {
            if (mFeatures == null)
                return false;
            return !mFeatures.Find(e => e.Contains(feature)).IsNullOrEmpty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"><feature flag/param>
        /// <param name="add">add or remove the feature?</param>
        /// <param name="isDefine">is it a #deefine?</param>
        public static void AddOrRemove(string feature, bool add, bool isDefine)
        {
            if (add)
                Add(feature, isDefine);
            else
                Remove(feature);
        }
    }
}
