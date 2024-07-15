using UnityEngine;

namespace XcelerateGames
{
    public class ObRotate : MonoBehaviour
    {
        public Vector3 _Speed;

        private bool mIsVisible = true;

        private Transform mTransform = null;

        private void Awake()
        {
            mTransform = transform;
        }

        private void OnBecameVisible()
        {
            mIsVisible = true;
        }

        private void OnBecameInVisible()
        {
            mIsVisible = false;
        }

        private void Update()
        {
            if (!mIsVisible)
                return;

            float dt = Time.deltaTime;
            mTransform.Rotate(_Speed.x * dt, _Speed.y * dt, _Speed.z * dt);
        }
    }
}