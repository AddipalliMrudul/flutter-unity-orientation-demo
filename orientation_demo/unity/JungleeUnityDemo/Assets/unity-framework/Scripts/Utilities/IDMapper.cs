using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using XcelerateGames.Testing;

namespace XcelerateGames
{
    /// <summary>
    /// This class works only when flag enabled AUTOMATION_ENABLED
    /// Creates a locator for Ui IDs and expose it for automation testing
    /// </summary>
    public class IDMapper : BaseBehaviour, IExposeData
    {

        /// <summary>
        /// this class have Dictionary data to expose in IExposeData
        /// </summary>
        [Serializable]
        public class ExposedMappingData
        {
            [JsonProperty("mapping")]
            public Dictionary<string, string> UiItemIDDict = new Dictionary<string, string>();
        }

        private ExposedMappingData mExposedMappingData = new ExposedMappingData(); /**< create an object of ExposedMappingData*/

        #region Static
        private static IDMapper mInstance = null;

        public object ExposeData => mExposedMappingData; /**< IExposeData implements */
        #endregion //Static


        #region Private Methods
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
            if (mInstance == null)
                mInstance = this;
            else
                Destroy(gameObject);

        }

        #endregion //Private Methods
#if AUTOMATION_ENABLED
        #region Public Methods
        /// <summary>
        /// Register id
        /// </summary>
        /// <param name="id">type of string - stores key for ui Gameobect</param>
        /// <param name="path">type of string - store path(Hierarchy) for ui Gameobject</param>
        public static void RegisterID(string id, string path)
        {
            if (mInstance == null)
                return;

            mInstance.mExposedMappingData.UiItemIDDict[id] = path;
            //XDebug.Log($"{mInstance.mExposedMappingData.UiItemIDDict.ToJson()}", XDebug.Mask.Game);
        }

        /// <summary>
        /// Register id
        /// </summary>
        /// <param name="id">type of string - stores key for ui Gameobect</param>
        /// <param name="path">type of string - store path(Hierarchy) for ui Gameobject</param>
        public static void ChangeIDPath(string id, string path)
        {
            if (mInstance == null)
                return;

            if (mInstance.mExposedMappingData.UiItemIDDict.ContainsKey(id))
                mInstance.mExposedMappingData.UiItemIDDict[id] = path;

            //XDebug.Log($"{mInstance.mExposedMappingData.UiItemIDDict.ToJson()}", XDebug.Mask.Game);
        }

        /// <summary>
        /// When an GameObject destroys from herirachy, remove its mapping 
        /// </summary>
        /// <param name="id">type of string - stores key for ui Gameobect</param>
        public static void UnRegisterID(string id)
        {
            if (mInstance == null)
                return;

            if (mInstance.mExposedMappingData.UiItemIDDict.ContainsKey(id))
                mInstance.mExposedMappingData.UiItemIDDict.Remove(id);

            //XDebug.Log($"{mInstance.mExposedMappingData.UiItemIDDict.ToJson()}", XDebug.Mask.Game);
        }
        #endregion //Public Methods
#endif

    }
}
