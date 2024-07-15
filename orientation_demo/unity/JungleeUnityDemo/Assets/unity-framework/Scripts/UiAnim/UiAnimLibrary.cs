using UnityEngine;
using System.Collections.Generic;
using System;
using XcelerateGames.AssetLoading;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace XcelerateGames.UI.Animations
{
    public static class UiAnimLibrary
    {
        private static UiAnim mAnims = null;
        public static bool mInitialized = false;

        #region Getters & Setters

        public static UiAnim pAnims { get { return mAnims; } }

        public static List<string> pAnimNames
        {
            get
            {
                List<string> anims = new List<string>();
                for (int i = 0; i < mAnims._Anims.Count; ++i)
                    anims.Add(mAnims._Anims[i]._Name);
                anims.Sort();
                anims.Insert(0, "Load");
                anims.Insert(1, "None");
                return anims;
            }
        }

        #endregion Getters & Setters

        #region Private methods
#if UNITY_EDITOR
        private static UiAnim Load()
        {
            UiAnim anim3D = null;
            GameObject obj = ResourceManager.LoadFromResources<GameObject>(mAssetName);
            if (obj != null)
            {
                anim3D = obj.GetComponent<UiAnim>();
                if (anim3D == null)
                    XDebug.LogError("UiAnimLibrary:: Could not find Anim3D component under " + obj.name);
            }
            else
                XDebug.LogError($"UiAnimLibrary:: Could not find {mAssetPath}");
            return anim3D;
        }
#else
        private static UiAnim Load()
        {
            UiAnim anim3D = null;
            GameObject obj = ResourceManager.LoadFromResources<GameObject>("AnimLib");
            if (obj != null)
            {
                anim3D = Utilities.Instantiate<UiAnim>(obj, "AnimLib");
                if (anim3D == null)
                    XDebug.LogError("Anim3DLibrary:: Could not find Anim3D component under " + obj.name);
                else
                {
                    if(Application.isPlaying)
                        GameObject.DontDestroyOnLoad(anim3D.gameObject);
                }
            }
            else
                XDebug.LogError("Anim3DLibrary:: Could not find AnimLib under Resources");
            return anim3D;
        }

#endif
        #endregion

        #region Public methods

        public static void Init()
        {
            if (mInitialized && mAnims != null)
                return;
            mAnims = Load();
#if UNITY_EDITOR
            if (mAnims == null)
            {
                UnityEngine.Debug.Log($"{mAssetName} may not exist, Creating one now");
                Create();
            }
#endif
            mInitialized = true;
        }

        /// <summary>
        /// Returns true if the given animation name exists in library, else returns false.
        /// </summary>
        /// <param name="animName"></param>
        /// <returns></returns>
        public static bool Exists(string animName)
        {
            return GetAnim(animName) != null;
        }

        public static UiAnimBase GetAnim(string animName, UiAnimBase dstAnim)
        {
            if (mAnims == null || mAnims._Anims == null)
                return dstAnim;
            UiAnimBase anim = mAnims._Anims.Find(e => Utilities.Equals(e._Name, animName));
            if (anim == null)
            {
                if (Application.isPlaying)
                    XDebug.LogException("Could not find \"" + animName + "\" in Anim Library");
                else
                    UnityEngine.Debug.LogWarning("Could not find \"" + animName + "\" in Anim Library");

                return dstAnim;
            }

            dstAnim._Mode = anim._Mode;
            dstAnim._Name = anim._Name;
            dstAnim._IgnoreTimeScale = anim._IgnoreTimeScale;
            dstAnim._SnapToStart = anim._SnapToStart;
            dstAnim._Category = anim._Category;
            dstAnim._Delay = anim._Delay;
            dstAnim._TimeMultiplier = anim._TimeMultiplier;
            dstAnim._DestroyOnComplete = anim._DestroyOnComplete;
            dstAnim._TriggerEventOverride = anim._TriggerEventOverride;
            //dstAnim._AudioClip = anim._AudioClip;
            //dstAnim._AudioDelay = anim._AudioDelay;

            dstAnim._PositionData = anim._PositionData;
            dstAnim._RotationData = anim._RotationData;
            dstAnim._ScaleData = anim._ScaleData;
            dstAnim._ColorData = anim._ColorData;
            return dstAnim;
        }

        public static UiAnimBase GetAnim(string animName)
        {
            if (mAnims == null || mAnims._Anims == null)
                return null;
            UiAnimBase anim = mAnims._Anims.Find(e => Utilities.Equals(e._Name, animName));
            if (anim == null)
                XDebug.LogException("Could not find \"" + animName + "\" in Anim Library");

            return anim;
        }
        #endregion Public methods

        #region Editor only code

#if UNITY_EDITOR
        public const string mAssetName = "AnimLib";
        public const string mAssetPath = "Assets/Resources/" + mAssetName;

        /// <summary>
        /// Adds the given animation to list & saves if user wants to overwrite existing animation
        /// </summary>
        /// <param name="anim"></param>
        /// <returns></returns>
        public static bool Save(UiAnimBase anim)
        {
            //Animation name cannot be empty.
            if (string.IsNullOrEmpty(anim._Name))
            {
                EditorUtility.DisplayDialog("Error", "Animation name cannot be empty", "Ok");
                return false;
            }
            bool noOverwrite = false;
            //Check if the animation that we are trying to add is already available in the library
            UiAnimBase existingAnim = GetAnim(anim._Name);
            if (existingAnim != null)
            {
                //Animation exists, ask if the user wants to overwrite it.
                noOverwrite = !EditorUtility.DisplayDialog("Error", "Animation with the name \"" + anim._Name + "\" already exists.\n Do you want to overwrite?", "Yes", "No");
                if (noOverwrite)
                {
                    //User chose not to overwrite, just bail out
                    return false;
                }
                else
                {
                    int index = mAnims._Anims.FindIndex(e => e._Name == anim._Name);
                    mAnims._Anims.Remove(existingAnim);
                    mAnims._Anims.Insert(index, anim.Clone());
                }
            }
            else
            {
                //Add the animation to list
                mAnims._Anims.Add(anim.Clone());
            }
            SaveToDisk();
            return true;
        }

        /// <summary>
        /// Saves all animations to disk
        /// </summary>
        /// <returns></returns>
        private static void SaveToDisk()
        {
            UiAnim anim = mAnims;
            UnityEngine.Debug.Log("Saved anim Library : " + anim.name);
            EditorUtility.SetDirty(mAnims.gameObject);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Creates the AnimLib object
        /// </summary>
        private static bool Create()
        {
            GameObject animLib = new GameObject();
            animLib.AddComponent<UiAnim>();
            FileUtilities.CreateDirectoryRecursively(mAssetPath);
            bool result;
            PrefabUtility.SaveAsPrefabAsset(animLib, mAssetPath + ".prefab", out result);
            return result;
        }
#endif

        #endregion Editor only code
    }
}