using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for Transform class
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Destroy all children objects of given Transform
        /// </summary>
        /// <param name="t">Instance of Transform</param>
        public static void DestroyChildren(this Transform t)
        {
            bool isPlaying = Application.isPlaying;

            while (t.childCount != 0)
            {
                Transform child = t.GetChild(0);

                if (isPlaying)
                {
                    child.SetParent(null);
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                else UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        /// <summary>
        /// Change parent of the given Transorm & set to a new one along with given local scale & local position
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newParent"></param>
        /// <param name="localPosition"></param>
        /// <param name="localScale"></param>
        public static void ChangeParent(this Transform t, Transform newParent, Vector3 localPosition, Vector3 localScale)
        {
            while (t.childCount != 0)
            {
                Transform child = t.GetChild(0);
                child.SetParent(newParent, localPosition, localScale, false);
            }
        }

        /// <summary>
        /// Set active/inactive the given transform. Helper function for SetActive of GameObject
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetActive(this Transform obj, bool value)
        {
            obj.gameObject.SetActive(value);
        }

        /// <summary>
        /// Returns the list of all immediet child Transforms. Self & all subsequent child transforms are not included
        /// </summary>
        /// <param name="inObject">Parent Transform</param>
        /// <param name="includeInactive">Should we include inactive objects?</param>
        /// <returns></returns>
        public static List<Transform> GetChildList(this Transform inObject, bool includeInactive)
        {
            if (inObject == null)
                return null;

            List<Transform> children = new List<Transform>(inObject.GetComponentsInChildren<Transform>(includeInactive));
            //Remove self(inObject)
            children.Remove(inObject);
            //Remove subsequent child objects.
            children.RemoveAll(e => e.parent != inObject);
            return children;
        }

        /// <summary>
        /// Returns child count of given Transform
        /// </summary>
        /// <param name="obj">Parent Transform</param>
        /// <param name="includeInactive"><Should we include inactive objects?/param>
        /// <returns></returns>
        public static int GetChildCount(this Transform obj, bool includeInactive)
        {
            if (includeInactive)
                return obj.transform.childCount;
            else
                return obj.GetChildList(includeInactive).Count;
        }

        /// <summary>
        /// Shuffle all child Transforms under the given Transform
        /// </summary>
        /// <param name="inObject">Paent Transform</param>
        public static void Shuffle(this Transform inObject)
        {
            if (inObject == null)
                return;

            //List all children
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < inObject.childCount; ++i)
            {
                children.Add(inObject.GetChild(i));
            }

            //Shuffle the list
            children.Shuffle();

            //Set sibling index
            for (int i = 0; i < inObject.childCount; ++i)
            {
                children[i].SetSiblingIndex(i);
            }
        }

        /// <summary>
        /// Set parent for the given Transform to the new parent. Parent can be null as well.
        /// </summary>
        /// <param name="transform">Transform whoese parent needs to be set/updated</param>
        /// <param name="parent">New parent. Can be null as well</param>
        /// <param name="localPosition">Local position to set after parenting</param>
        /// <param name="localScale">Local scale to set after parenting</param>
        /// <param name="worldPositionStays">Should Transform postion be retained after setting new parent?</param>
        public static void SetParent(this Transform transform, Transform parent, Vector3 localPosition, Vector3 localScale, bool worldPositionStays)
        {
            transform.SetParent(parent, worldPositionStays);
            transform.localPosition = localPosition;
            transform.localScale = localScale;
        }

        /// <summary>
        /// Get first child of the given Transform
        /// </summary>
        /// <param name="transform">Parent Transform</param>
        /// <returns>Chaild Transform</returns>
        public static Transform FirstChild(this Transform transform)
        {
            if (transform.childCount == 0)
                return null;
            return transform.GetChild(0);
        }

        /// <summary>
        /// Get last child of the given Transform
        /// </summary>
        /// <param name="transform">Parent Transform</param>
        /// <returns>Chaild Transform</returns>
        public static Transform LastChild(this Transform transform)
        {
            if (transform.childCount == 0)
                return null;
            return transform.GetChild(transform.childCount - 1);
        }

        /// <summary>
        /// Reverse the order of all child transforms
        /// </summary>
        /// <param name="transform">Parent Transform</param>
        public static void ReverseOrderOfChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                transform.GetChild(0).SetSiblingIndex(transform.childCount - 1 - i);
            }
        }

        /// <summary>
        /// Returns size of RectTransform. X is width & Y is height
        /// </summary>
        /// <param name="transform">Instance of Transform</param>
        /// <returns>Height & Width of Transform</returns>
        public static Vector2 GetSize(this Transform transform)
        {
            return transform.GetComponent<RectTransform>().GetSize();
        }

        #region State management
        /// <summary>
        /// Save the state of given Transfrom to PlayerPrefs
        /// </summary>
        /// <param name="transform">Transform to save</param>
        /// <param name="key">Transform state will be saved under this key. Key is pre-pended for position, rotation & scale</param>
        /// <param name="local">Whether to save local properties or world propeties</param>
        public static void SaveState(this Transform transform, string key, bool local)
        {
            PlayerPrefs.SetString(key + "-pos", (local ? transform.localPosition : transform.position).ToJson());
            PlayerPrefs.SetString(key + "-rot", (local ? transform.localRotation.eulerAngles : transform.rotation.eulerAngles).ToJson());
            PlayerPrefs.SetString(key + "-scale", transform.localScale.ToJson());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load the state of Transform from PlayerPrefs
        /// </summary>
        /// <param name="transform">Transform to load</param>
        /// <param name="key">key to be used to load. Key is pre-pended for position, rotation & scale</param>
        /// <param name="local">load local or world properties</param>
        /// <param name="deleteAfterLoading">Should we delete the data after loading?</param>
        /// <returns></returns>
        public static bool LoadState(this Transform transform, string key, bool local, bool deleteAfterLoading = true)
        {
            if (PlayerPrefs.HasKey(key + "-pos"))
            {
                Vector3 data = PlayerPrefs.GetString(key + "-pos").FromJson<Vector3>();
                if (local)
                    transform.localPosition = data;
                else
                    transform.position = data;

                data = PlayerPrefs.GetString(key + "-rot").FromJson<Vector3>();
                if (local)
                    transform.localRotation = Quaternion.Euler(data);
                else
                    transform.rotation = Quaternion.Euler(data);

                transform.localScale = PlayerPrefs.GetString(key + "-scale").FromJson<Vector3>();
                if (deleteAfterLoading)
                {
                    DeleteState(key);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete the Transform state by key
        /// </summary>
        /// <param name="key">Key to delete</param>
        /// <returns></returns>
        public static bool DeleteState(string key)
        {
            if (PlayerPrefs.HasKey(key + "-pos"))
            {
                PlayerPrefs.DeleteKey(key + "-pos");
                PlayerPrefs.DeleteKey(key + "-rot");
                PlayerPrefs.DeleteKey(key + "-scale");
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }
        #endregion State management
    }
}