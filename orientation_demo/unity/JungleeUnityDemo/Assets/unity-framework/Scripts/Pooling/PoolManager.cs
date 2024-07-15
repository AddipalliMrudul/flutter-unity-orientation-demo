using UnityEngine;
using System.Collections.Generic;

namespace XcelerateGames.Pooling
{

    public class PoolManager
    {
        private const int mDefaultPoolSize = 5;

        private static Dictionary<string, Pool> mPooledObjects = new Dictionary<string, Pool>();
        private static Transform mPoolRoot = null;

        public static Transform pPoolRoot
        {
            get
            {
                if (mPoolRoot == null)
                {
                    mPoolRoot = new GameObject("PoolRoot").transform;
                    mPoolRoot.gameObject.SetActive(false);
                    Object.DontDestroyOnLoad(mPoolRoot.gameObject);
                }
                return mPoolRoot;
            }
        }

        public static bool IsPoolCreated(string poolName)
        {
            return mPooledObjects.ContainsKey(poolName);
        }

        public static Pool GetPool(string poolName)
        {
            if(IsPoolCreated(poolName))
                return mPooledObjects[poolName];
            return null;
        }

        public static Pool CreatePool(string poolName, int poolSize, GameObject prefab, bool dontDestroyOnLoad)
        {
            if (IsPoolCreated(poolName))
            {
                Debug.LogWarning($"Error! Pool {poolName} already exists");
                return GetPool(poolName);
            }

            if (string.IsNullOrEmpty(poolName))
            {
                Debug.LogError("Error! poolName cannot be empty");
                return null;
            }

            if (prefab == null)
            {
                Debug.LogError("Error! prefab cannot be null");
                return null;
            }

            if (poolSize <= 0)
            {
                Debug.LogError("Error! poolSize should be greater than zero");
                return null;
            }

            mPooledObjects[poolName] = new Pool(poolName, poolSize, prefab, dontDestroyOnLoad);

            return mPooledObjects[poolName];
        }

        public static bool ReleasePool(Pool pool)
        {
            if (pool == null)
            {
                Debug.LogError("Error! Pool is null");
                return false;
            }
            if(XDebug.CanLog(XDebug.Mask.Game))
                Debug.Log($"Releasing pool: {pool.Name}");
            pool.Destroy();
            mPooledObjects.Remove(pool.Name);
            pool = null;

            return true;
        }
    }
}
