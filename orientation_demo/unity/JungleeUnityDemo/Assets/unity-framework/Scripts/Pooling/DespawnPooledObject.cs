using UnityEngine;
/// <summary>
/// Attach this script to the poold objects
/// </summary>

namespace XcelerateGames.Pooling
{

    public class DespawnPooledObject : MonoBehaviour
    {
        public float _Time = 1f;
        private Pool mPool = null;

        public void Init(float time, Pool pool)
        {
            mPool = pool;
            if(time > 0)
                Invoke("Despawn", time);
        }

        public void Init(Pool pool)
        {
            Init(_Time, pool);
        }

        public void Despawn()
        {
            if (mPool != null)
                mPool.Despawn(gameObject);
        }
    }
}