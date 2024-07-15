using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames
{
    /// <summary>
    /// Extension class for GameObjects
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Set GameObject layer recursively
        /// </summary>
        /// <param name="obj">Instance of GameObject</param>
        /// <param name="newLayer">new layer mask</param>
        public static void SetLayerRecursively(this GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, newLayer);
        }

        /// <summary>
        /// Get the MonoBehaviour instances entire hierarchical path in the scene
        /// </summary>
        /// <param name="obj">Instance of MonoBehaviour</param>
        /// <returns>Object path in scene</returns>
        /// @see GetObjectPath(GameObject)
        public static string GetObjectPath(this MonoBehaviour obj)
        {
            return GetObjectPath(obj.gameObject);
        }

        /// <summary>
        /// Get the GameObjects entire hierarchical path in the scene
        /// </summary>
        /// <param name="obj">Instance of GameObject</param>
        /// <returns>Object path in scene</returns>
        /// @see GetObjectPath(MonoBehaviour)
        public static string GetObjectPath(this GameObject gameObject)
        {
            if (gameObject == null)
                return null;
            string objPath = gameObject.name;
            Transform objParent = gameObject.transform.parent;
            while (objParent != null)
            {
                objPath = objParent.name + "/" + objPath;
                objParent = objParent.parent;
            }

            return objPath;
        }
    }
}