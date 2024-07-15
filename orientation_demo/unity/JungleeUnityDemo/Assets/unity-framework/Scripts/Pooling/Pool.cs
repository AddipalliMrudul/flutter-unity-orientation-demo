using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.Pooling
{
    public class Pool
    {
        private List<GameObject> mPooledObjects = null;
        private GameObject mPoolTemplate = null;
        private Transform mRoot = null;

        public string Name { get; private set; }

        public Pool(string poolName, int poolSize, GameObject poolTemplate, bool dontDestroyOnLoad)
        {
            Name = poolName;
            mRoot = new GameObject(poolName).transform;
            if(dontDestroyOnLoad)
            {
                Object.DontDestroyOnLoad(mRoot);
                Object.DontDestroyOnLoad(poolTemplate);
            }
            mRoot.SetParent(PoolManager.pPoolRoot, false);
            mPoolTemplate = poolTemplate;
            mPooledObjects = new List<GameObject>();
            for (int i = 0; i < poolSize; ++i)
            {
                Create();
            }
        }
    
        public GameObject Spawn()
        {
            if (mPooledObjects == null)
                return null;
            GameObject go = null;
            for (int i = 0; i < mPooledObjects.Count; ++i)
            {
                go = mPooledObjects[i];
                if (go == null)
                {
                    XDebug.Log("Error! Pool object is null. Do not destroy pooled objects, Call Despawn instead of Destroy. Pool Name : " + Name, XDebug.Mask.Resources);
                    go = CreateRaw();
                    go.SetActive(true);
                    mPooledObjects[i] = go;
                    return go;
                }
                if (!go.activeSelf)
                {
                    go.SetActive(true);
                    return go;
                }
            }
            Debug.LogFormat("We ran out of pooled objects for pool {0}, create & add new one", Name);
            //We ran out of pooled objects, create & add new one
            go = Create();
            go.SetActive(true);
            return go;
        }

        public void Despawn(GameObject inObject)
        {
            if (inObject == null)
            {
                Debug.LogError("Error! object cannot be null");
                return;
            }

            inObject.SetActive(false);
            inObject.transform.SetParent(mRoot, false);
        }

        public void DespawnAll()
        {
            for (int i = 0; i < mPooledObjects.Count; ++i)
            {
                if(mPooledObjects[i] != null)
                    Despawn(mPooledObjects[i]);
            }
        }

        private GameObject Create()
        {
            GameObject go = CreateRaw();
            mPooledObjects.Add(go);
            return go;
        }

        private GameObject CreateRaw()
        {
            GameObject go = GameObject.Instantiate(mPoolTemplate) as GameObject;
            go.name = mPoolTemplate.name + "-" + (mPooledObjects.Count + 1);
            //Animator is by default not optimized for use in pooling, Lets enable that
            //Animator[] animators = go.GetComponentsInChildren<Animator>();
            //foreach (Animator animator in animators)
                //animator.keepAnimatorControllerStateOnDisable = true;
            go.transform.SetParent(mRoot, false);
            go.SetActive(false);
            return go;
        }

        internal void Destroy()
        {
            if(mRoot != null)
                GameObject.Destroy(mRoot.gameObject);
            if(mPooledObjects != null)
                mPooledObjects.Clear();
            mPooledObjects = null;
        }
    }
}