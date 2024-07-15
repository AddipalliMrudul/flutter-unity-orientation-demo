using UnityEngine;

namespace XcelerateGames
{
    public class DestroyGameObject : MonoBehaviour
    {
        public void DestroyMe()
        {
            Destroy(gameObject);
        }
    }
}
