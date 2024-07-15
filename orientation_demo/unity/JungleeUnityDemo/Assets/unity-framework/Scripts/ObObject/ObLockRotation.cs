using UnityEngine;

/* Author : Altaf
 * Date : Apr 17, 2018
 * Purpose : Locks object rotation irrespective of parent object rotation
*/

namespace XcelerateGames
{
    [ExecuteInEditMode]
    public class ObLockRotation : MonoBehaviour
    {
        public RectTransform _Parent = null;
        private RectTransform mTransform = null;

        void Start()
        {
            mTransform = GetComponent<RectTransform>();
            if (_Parent == null)
                _Parent = mTransform.parent.GetComponent<RectTransform>();
        }

        void Update()
        {
            float angle = 360 - _Parent.rotation.eulerAngles.z;
            mTransform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
